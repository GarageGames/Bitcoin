using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace MinerLib_cs
{
    public class Connection
    {
        public enum ConnectionState
        {
            Connecting,
            Connected,
            Disconnected
        };

        uint mHashChunkSize;
        string mMember;
        string mProduct;
        string mPlatform;
        ConnectionState mState;
        Socket mSocket;
        bool mCanRead;
        bool mCanWrite;
        DateTime mAliveTime;
        byte[] mInputBuffer;
        int mInputBufferBytes;
        Work mWorkBlock;

        public Connection(string memberName, string productName, string platform, uint requestedHashes = 5000)
        {
            mWorkBlock = null;

            mInputBuffer = new byte[1024 * 8];
            mInputBufferBytes = 0;

            mHashChunkSize = requestedHashes;
            mMember = memberName;
            mProduct = productName;
            mPlatform = platform;
            mState = ConnectionState.Disconnected;

            SetupSocket();
        }

        public void Destroy()
        {
            Disconnect();
        }

        void SetupSocket()
        {
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mSocket.Blocking = false;
            mSocket.NoDelay = true;
            mCanRead = false;
            mCanWrite = false;
        }

        public void Update()
        {
            List<Socket> recvList = new List<Socket>();
            List<Socket> sendList = new List<Socket>();
            List<Socket> exceptList = new List<Socket>();

            recvList.Add(mSocket);
            sendList.Add(mSocket);
            exceptList.Add(mSocket);

            Socket.Select(recvList, sendList, exceptList, 1000);
            int result = recvList.Count + sendList.Count + exceptList.Count;
            if (result > 0)
            {
                mCanRead = recvList.Count > 0;
                mCanWrite = sendList.Count > 0;

                if (mState == ConnectionState.Connecting)
                {
                    if (sendList.Count > 0)
                    {
                        mState = ConnectionState.Connected;
                        SendIdentityPacket();
                    }
                    else if (exceptList.Count > 0)
                    {
                        mState = ConnectionState.Disconnected;
                    }
                }
            }

            if (mCanRead && mState == ConnectionState.Connected)
            {
                try
                {
                    byte[] dataBuffer = new byte[1024];
                    int bytesRead = mSocket.Receive(dataBuffer, 0);
                    mAliveTime = DateTime.Now;
                    if( bytesRead > 0 )
                    {
                        Array.Copy(dataBuffer, 0, mInputBuffer, mInputBufferBytes, bytesRead);
                        mInputBufferBytes += bytesRead;
                        ProcessPackets();
                    
                    }
                    else if( bytesRead == 0 )
                    {
                        // Connection closed
                        Disconnect();
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Disconnect();
                }
            }

            if( mState == ConnectionState.Connected )
            {
                TimeSpan timeSinceSeen = DateTime.Now - mAliveTime;
                if( timeSinceSeen.TotalSeconds > 5 )
                {
                    SendPing();
                    mAliveTime = DateTime.Now;
                }
            }
        }

        void ProcessPackets()
        {
            int bytesInBuffer = mInputBufferBytes;
            int currOffset = 0;
            while (bytesInBuffer > 0)
            {
                int consumed = 0;
                byte command = mInputBuffer[currOffset];
                switch (command)
                {
                    case 3:     // Work Command
                        if (bytesInBuffer < 269)
                        {
                            Array.Copy(mInputBuffer, currOffset, mInputBuffer, 0, bytesInBuffer);
                            mInputBufferBytes = bytesInBuffer;
                            return;
                        }
                        byte[] workData = new byte[268];
                        Array.Copy(mInputBuffer, currOffset + 1, workData, 0, 268);
                        ProcessWorkCommand(workData);
                        consumed = 269;
                        break;
                    case 4:     // Stop Command
                        break;
                    case 5:     // Ping
                        SendPing();
                        consumed = 1;
                        break;
                }
                bytesInBuffer -= consumed;
                currOffset += consumed;
            }
            mInputBufferBytes = 0;
        }

        void SendIdentityPacket()
        {
            int agentSize = mMember.Length + 1;
            int platformSize = mPlatform.Length + 1;
            int locationSize = mProduct.Length + 1;

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write((byte)1);      // Identity Packet
            bw.Write((byte)1);      // Network Version
            bw.Write((byte)1);      // Type = C#

            bw.Write(mHashChunkSize);
            bw.Write(mMember.ToCharArray()); bw.Write((byte)0);
            bw.Write(mPlatform.ToCharArray()); bw.Write((byte)0);
            bw.Write(mProduct.ToCharArray()); bw.Write((byte)0);

            SendPacket(ms.ToArray());
            bw.Close();
        }

        void SendPing()
        {
            byte[] pingPacket = new byte[1];
            pingPacket[0] = 5;
            SendPacket(pingPacket);
        }

        void SendPacket(byte[] data)
        {
            if( mCanWrite )
            {
                int sentBytes = mSocket.Send(data);
                if( sentBytes < 0 )
                {
                }
            }
        }

        void ProcessWorkCommand(byte[] data)
        {
            Work work = new Work();

            MemoryStream ms = new MemoryStream(data);
            BinaryReader br = new BinaryReader(ms);

            work.hashStart = br.ReadUInt32();
            work.hashCount = br.ReadUInt32();
            uint c = br.ReadUInt32();
            if (c == 0)
                work.hashAlgorithm = Work.Algorithm.SHA256;
            else
                work.hashAlgorithm = Work.Algorithm.Scrypt;
            byte[] target = br.ReadBytes(32);
            Buffer.BlockCopy(target, 0, work.target, 0, 32);
            
            byte[] blockHeader = br.ReadBytes(128);
            Buffer.BlockCopy(blockHeader, 0, work.data, 0, 128);

            mWorkBlock = work;

            br.Close();
        }

        public void ConnectTo(string hostName, ushort port = 80)
        {
            // Disconnect from any existing connection
            Disconnect();
    
            // Get the IP address from the host name string
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            if( addresses.Length > 0 )
            {
                try
                {
                    mSocket.Connect(addresses[0], port);
                    mState = ConnectionState.Connecting;
                }
                catch( SocketException ex )
                {
                    if (ex.ErrorCode == 10035)
                    {
                        // Would Block
                        mState = ConnectionState.Connecting;
                    }
                    else
                        Console.WriteLine(ex.Message);
                }
            }
        }

        public void Disconnect()
        {
            if( mState != ConnectionState.Disconnected )
            {
                // close the connection
                mSocket.Close();
        
                // Reset socket to be used again
                SetupSocket();

                // Change state
                mState = ConnectionState.Disconnected;
            }
        }

        public void SendWorkComplete(bool solutionFound, uint solution, uint hashesDone)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write((byte)2);      // Work Complete Packet
            bw.Write((byte)(solutionFound ? 1 : 0));
            bw.Write(solution);
            bw.Write(hashesDone);

            SendPacket(ms.ToArray());
            bw.Close();
        }

        public Work GetWork()
        {
            Work ret = null;
            if (mWorkBlock != null)
            {
                ret = mWorkBlock;
                mWorkBlock = null;
            }
            return ret;
        }

        public ConnectionState State
        {
            get { return mState; }
        }
    }
}

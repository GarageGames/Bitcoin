using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;

namespace CentralMine.NET
{
    public class Client
    {
        enum Type
        {
            Javascript,
            CSharp,
            Cpp,
            Unknown,
        };

        public enum State
        {
            New,
            Ready,
            Busy,
            Stopping,
        };

        ClientManager mTheMan;
        TcpClient mClient;
        Type mType;
        DateTime mLastSeen;
        public HashManager.HashBlock mHashBlock = null;
        public State mState;

        public uint mDesiredHashes;
        public uint mHashesDone;
        public ulong mTotalHashesDone;

        public Block mCurrentBlock;

        public Client(TcpClient tcp, ClientManager manager)
        {
            mTheMan = manager;
            mClient = tcp;
            mClient.NoDelay = true;
            mState = State.New;
            mType = Type.Unknown;

            mHashesDone = 0;
            mTotalHashesDone = 0;
            mCurrentBlock = null;

            mLastSeen = DateTime.Now;
        }

        public void SendWork(HashManager.HashBlock hashBlock, Block block)
        {
            mHashBlock = hashBlock;

            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write((byte)3);
            bw.Write(hashBlock.Start);
            bw.Write(hashBlock.Count);
            bw.Write(block.midstate);
            bw.Write(block.data64);
            bw.Write(block.target);

            SendPacket(stream.ToArray());
            bw.Close();

            mCurrentBlock = block;
            mState = State.Busy;
        }

        public void StopWork()
        {
            byte[] data = new byte[1];
            data[1] = 4;
            SendPacket(data);
            mState = State.Stopping;
        }

        void SendPacket(byte[] data)
        {
            if (!mClient.Connected)
                return;

            if (mType == Type.Javascript)
            {
                // Encode the data for websocket
                MemoryStream stream = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(stream);

                // Write the data type
                bw.Write((byte)130);
                
                // Write the length
                if (data.Length <= 125)
                {
                    bw.Write((byte)data.Length);
                }
                else if (data.Length >= 126 && data.Length <= 65535)
                {
                    bw.Write((byte)126);
                    bw.Write((ushort)IPAddress.NetworkToHostOrder((short)data.Length));
                }
                else
                {
                    bw.Write((byte)127);
                    bw.Write((ulong)IPAddress.NetworkToHostOrder(data.LongLength));
                }

                // Write the data
                bw.Write(data);

                // Send to the client
                byte[] output = stream.ToArray();
                mClient.GetStream().Write(output, 0, output.Length);
            }
            else
            {
                // Just send the data
                mClient.GetStream().Write(data, 0, data.Length);
            }
        }

        public bool Update()
        {
            if (!mClient.Connected)
                return false;

            try
            {
                TimeSpan span = DateTime.Now - mLastSeen;
                if (span.TotalSeconds > 5)
                {
                    // Send ping
                    byte[] ping = { 5 };
                    SendPacket(ping);
                }
            }
            catch (Exception)
            {
                return false;
            }

            try
            {
                NetworkStream stream = mClient.GetStream();
                if (stream.DataAvailable)
                {
                    mLastSeen = DateTime.Now;
                    int command = stream.ReadByte();
                    switch (command)
                    {
                        case 1:
                            ProcessIdentity(stream);
                            break;
                        case 2:
                            ProcessWorkComplete(stream);
                            break;
                        case 5: // Ping
                            break;
                        case 71:
                            ProcessWebsocketConnect(stream);
                            break;
                        case 130:
                            ProcessWebsocketPacket(stream);
                            break;
                        case 136:
                            // websocket close?
                            mClient.Close();
                            return false;
                        default:
                            byte[] temp = new byte[4096];
                            int read = stream.Read(temp, 0, (int)temp.Length);
                            Console.WriteLine("Read unknown bytes(" + (read + 1) + ")");
                            Console.Write((char)command);
                            for (int i = 0; i < read; i++)
                            {
                                Console.Write((char)temp[i]);
                            }
                            Console.WriteLine("");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true; // still alive
        }

        void ProcessIdentity(Stream stream, bool byteswap = false)
        {
            BinaryReader br = new BinaryReader(stream);

            byte clientType = br.ReadByte();
            switch (clientType)
            {
                case 0: mType = Type.Javascript; break;
                case 1: mType = Type.CSharp; break;
                case 2: mType = Type.Cpp; break;
                default: mType = Type.Unknown; break;
            }
            mDesiredHashes = br.ReadUInt32();

            if (byteswap)
                mDesiredHashes = (uint)IPAddress.NetworkToHostOrder((int)mDesiredHashes);

            mState = State.Ready;
        }

        void ProcessWorkComplete(Stream stream, bool byteswap = false)
        {
            BinaryReader br = new BinaryReader(stream);

            bool solutionFound = (br.ReadByte() != 0);
            uint solutionValue = br.ReadUInt32();
            mHashesDone = br.ReadUInt32();

            if (byteswap)
            {
                solutionValue = (uint)IPAddress.NetworkToHostOrder((int)solutionValue);
                mHashesDone = (uint)IPAddress.NetworkToHostOrder((int)mHashesDone);
            }

            mTotalHashesDone += mHashesDone;
            mTheMan.WorkComplete(this, solutionFound, solutionValue);

            mState = State.Ready;
        }

        void ProcessWebsocketPacket(NetworkStream stream)
        {
            byte[] temp = new byte[4096];
            temp[0] = 130;
            int read = stream.Read(temp, 1, (int)temp.Length - 1) + 1;

            int length = temp[1] & 127;
            //Console.WriteLine("Websocket packet length: " + length);

            byte[] dataBytes = new byte[length];
            for (int i = 6, j = 0; i < read && j < length; i++, j++)
            {
                int mask = j % 4;
                dataBytes[j] = (byte)(temp[i] ^ temp[2 + mask]);
            }

            MemoryStream str = new MemoryStream(dataBytes);
            str.Position = 1;
            switch (dataBytes[0])
            {
                case 1:
                    ProcessIdentity(str, true);
                    break;
                case 2:
                    ProcessWorkComplete(str);
                    break;
                case 5: // Ping
                    break;
            }
        }

        void ProcessWebsocketConnect(NetworkStream stream)
        {
            byte[] temp = new byte[4096];
            temp[0] = 71;
            int read = stream.Read(temp, 1, (int)temp.Length - 1);
            MemoryStream memstream = new MemoryStream(temp);
            StreamReader reader = new StreamReader(memstream);

            var headers = new Dictionary<string, string>();
            string line;
            while ((line = reader.ReadLine()) != string.Empty)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var tokens = line.Split(new char[] { ':' }, 2);
                    if (!string.IsNullOrWhiteSpace(line) && tokens.Length > 1)
                    {
                        headers[tokens[0]] = tokens[1].Trim();
                    }
                }
            }

            String secWebSocketAccept = ComputeWebSocketHandshakeSecurityHash09(headers["Sec-WebSocket-Key"]);

            // send handshake to this client only
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine("HTTP/1.1 101 Web Socket Protocol Handshake");
            writer.WriteLine("Upgrade: WebSocket");
            writer.WriteLine("Connection: Upgrade");
            writer.WriteLine("Sec-WebSocket-Accept: " + secWebSocketAccept);
            writer.WriteLine("");
            writer.Flush();           
        }

        public static String ComputeWebSocketHandshakeSecurityHash09(String secWebSocketKey)
        {
            const String MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            String secWebSocketAccept = String.Empty;

            // 1. Combine the request Sec-WebSocket-Key with magic key.
            String ret = secWebSocketKey + MagicKEY;

            // 2. Compute the SHA1 hash
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));

            // 3. Base64 encode the hash
            secWebSocketAccept = Convert.ToBase64String(sha1Hash);

            return secWebSocketAccept;
        }        
    }
}

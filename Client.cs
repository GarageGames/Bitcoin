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
        DateTime mWorkSent;        
        public HashManager.HashBlock mHashBlock = null;
        public State mState;

        public uint mDesiredHashes;
        public uint mHashesDone;
        public ulong mTotalHashesDone;
        public double mHashrate;

        public string mAgent;
        public string mPlatform;
        public string mLocation;

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
            mHashrate = 0;

            mLastSeen = DateTime.Now;
        }

        public override string ToString()
        {
            string str = "";

            IPEndPoint remoteIP = mClient.Client.RemoteEndPoint as IPEndPoint;
            str += remoteIP.ToString() + "(" + (mClient.Connected ? "connected" : "disconnected") + ") - ";

            str += "state:";
            switch (mState)
            {
                case State.Busy:    str += "busy "; break;
                case State.New:     str += "new "; break;
                case State.Ready:   str += "ready "; break;
                default:            str += "unknown "; break;
            }

            str += "type: ";
            switch (mType)
            {
                case Type.Cpp: str += "cpp "; break;
                case Type.CSharp: str += "c# "; break;
                case Type.Javascript: str += "js "; break;
                default: str += "unknown "; break;
            }

            str += "hashrate: " + mHashrate + " ";

            str += "lastSeen: " + mLastSeen.ToString() + " ";

            if( mHashBlock != null )
            {
                TimeSpan elapsed = DateTime.Now - mWorkSent;
                str += "TimeInBlock: " + elapsed + " ";
            }

            return str;
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
            mWorkSent = DateTime.Now;
        }

        public void StopWork()
        {
            byte[] data = new byte[1];
            data[1] = 4;
            SendPacket(data);
            mState = State.Stopping;
        }

        void SendCB(IAsyncResult ar)
        {
            if (mClient.Connected)
            {
                try
                {
                    mClient.GetStream().EndWrite(ar);
                }
                catch (Exception)
                {
                }
            }
        }

        void SendPacket(byte[] data)
        {
            if (!mClient.Connected || !mClient.GetStream().CanWrite)
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
                mClient.GetStream().BeginWrite(output, 0, output.Length, new AsyncCallback(SendCB), this);
            }
            else
            {
                // Just send the data
                mClient.GetStream().BeginWrite(data, 0, data.Length, new AsyncCallback(SendCB), this);
            }
        }

        public bool Update()
        {
            if (!mClient.Connected)
                return false;

            if (mHashBlock != null)
            {
                TimeSpan s = DateTime.Now - mWorkSent;
                if (s.TotalSeconds > 60)
                {
                    // This work took to long, just close the connection and force this client to reconnect
                    mClient.Close();
                    return false;   
                }
            }

            if (mState != State.New)
            {
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
            }
            else
            {
                TimeSpan span = DateTime.Now - mLastSeen;
                if (span.TotalSeconds > 5)
                {
                    // No identity packet, kill the connection
                    mClient.Close();
                    return false;
                }
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
                            byte[] temp = new byte[16 * 1024];
                            int read = stream.Read(temp, 0, (int)temp.Length);
                            Console.WriteLine("Read unknown bytes(" + (read + 1) + ")");
                            /*
                            Console.Write((char)command);
                            for (int i = 0; i < read; i++)
                            {
                                Console.Write((char)temp[i]);
                            }
                            Console.WriteLine("");
                            */
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

        string ReadStr(BinaryReader br)
        {
            string str = "";

            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                byte c = br.ReadByte();
                if (c == 0)
                    break;

                str += (char)c;
            }

            return str;
        }

        void ProcessIdentity(Stream stream, bool byteswap = false)
        {
            byte[] buffer = new byte[8 * 1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            MemoryStream mem = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(mem);

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

            mAgent = ReadStr(br);
            mPlatform = ReadStr(br);
            mLocation = ReadStr(br);

            mState = State.Ready;
        }

        void ProcessWorkComplete(Stream stream, bool byteswap = false)
        {
            TimeSpan elapsed = DateTime.Now - mWorkSent;            

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

            mHashrate = mHashesDone / elapsed.TotalSeconds;
            //Console.WriteLine("Hashes: {0}  Time: {1}  Hashrate: {0:N}", mHashesDone, elapsed.TotalSeconds, mHashrate);
            mState = State.Ready;
        }

        void ProcessWebsocketPacket(NetworkStream stream)
        {
            byte[] temp = new byte[4096];
            temp[0] = 130;
            int read = stream.Read(temp, 1, (int)temp.Length - 1) + 1;

            int lengthBytes = 1;
            long length = temp[1] & 127;
            if (length == 126)
            {
                length = (temp[2] << 8);
                length = length | temp[3];
                lengthBytes += 2;
            }
            else if (length == 127)
            {
                length = (temp[2] << 56) | (temp[3] << 48) | (temp[4] << 40) | (temp[5] << 32) | (temp[6] << 24) | (temp[7] << 16) | (temp[8] << 8) | temp[9];
                lengthBytes += 8;
            }

            Console.WriteLine(read + ": Websocket packet length: " + length);

            byte[] dataBytes = new byte[length];
            for (int i = 5 + lengthBytes, j = 0; i < read && j < length; i++, j++)
            {
                int mask = j % 4;
                byte maskByte = temp[1 + lengthBytes + mask];
                dataBytes[j] = (byte)(temp[i] ^ maskByte);
            }

            MemoryStream str = new MemoryStream(dataBytes);
            str.Position = 1;
            switch (dataBytes[0])
            {
                case 1:
                    ProcessIdentity(str, true);
                    break;
                case 2:
                    ProcessWorkComplete(str, true);
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
            string line = reader.ReadLine();
            while( !string.IsNullOrEmpty(line) )
            {
                var tokens = line.Split(new char[] { ':' }, 2);
                if (!string.IsNullOrWhiteSpace(line) && tokens.Length > 1)
                {
                    headers[tokens[0]] = tokens[1].Trim();
                }
                line = reader.ReadLine();
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

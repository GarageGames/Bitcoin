using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace HashrateCalculator
{
    public class NodeConnection
    {
        delegate void PacketProcessor(byte[] packetPayload);

        Dictionary<string, PacketProcessor> mPacketProcessors;
        Socket mSocket;
        Thread mThread;
        Bitcoin mOwner;
        public uint mProtocolVersion;
        uint mNetworkID;
        ushort mPort;
        bool mConnected = false;

        const uint cMainNetwork = 0xD9B4BEF9;


        uint mRemoteVersion;
        ulong mRemoteServices;
        ulong mRemoteTimestamp;
        ulong mRemoteNodeNOnce;
        string mRemoteUserAgent;
        public uint mRemoteHeight;

        public NodeConnection(Bitcoin owner, string nodeAddress, ushort port = 8333, uint protocolVersion = 0x00011171, uint networkID = 0xD9B4BEF9)
        {
            mOwner = owner;
            mProtocolVersion = protocolVersion;
            mPort = port;
            mNetworkID = networkID;

            mPacketProcessors = new Dictionary<string, PacketProcessor>();
            mPacketProcessors.Add("version", PH_Version);
            mPacketProcessors.Add("verack", PH_Verack);
            mPacketProcessors.Add("addr", PH_Addr);
            mPacketProcessors.Add("inv", PH_Inv);
            mPacketProcessors.Add("getdata", PH_GetData);
            mPacketProcessors.Add("notfound", PH_NotFound);
            mPacketProcessors.Add("getblocks", PH_GetBlocks);
            mPacketProcessors.Add("getheaders", PH_GetHeaders);
            mPacketProcessors.Add("tx", PH_Tx);
            mPacketProcessors.Add("block", PH_Block);
            mPacketProcessors.Add("headers", PH_Headers);
            mPacketProcessors.Add("getaddr", PH_GetAddr);
            mPacketProcessors.Add("mempool", PH_Mempool);
            mPacketProcessors.Add("checkorder", PH_CheckOrder);
            mPacketProcessors.Add("submitorder", PH_Subimtorder);
            mPacketProcessors.Add("reply", PH_Reply);
            mPacketProcessors.Add("ping", PH_Ping);
            mPacketProcessors.Add("pong", PH_Pong);
            mPacketProcessors.Add("alert", PH_Alert);

            mThread = new Thread(new ThreadStart(ReadSocket));

            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Connect(nodeAddress, port);
        }

        public void Destroy()
        {
            mSocket.Close();
            mThread.Abort();
        }

        public bool IsConnected() { return mConnected; }

        void Connect(string nodeAddress, ushort port = 8333)
        {
            // Connect to remote host
            try
            {
                Console.WriteLine("Connecting to: " + nodeAddress);                
                mSocket.Connect(nodeAddress, port);
                //IAsyncResult connAsync = mSocket.BeginConnect(nodeAddress, port, null, null);
                //bool success = connAsync.AsyncWaitHandle.WaitOne(3000, true);
                //if (success)
                //{
                    Console.WriteLine("connected to: " + nodeAddress);

                    // Start the read thread
                    mThread.Start();

                    // Send version packet
                    SendVersionPacket();
                //}
                //else
                //{
                //    Console.WriteLine("connnection timeout");
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void SendVersionPacket()
        {
            IPEndPoint remote = (IPEndPoint)mSocket.RemoteEndPoint;
            IPEndPoint local = (IPEndPoint)mSocket.LocalEndPoint;

            // Send version packet
            MemoryStream stream = new MemoryStream();
            BinaryWriter w = new BinaryWriter(stream);

            // -- PAYLOAD --
            // version
            w.Write(mProtocolVersion);

            // Services
            Int64 services = 0x0000000000000001;
            w.Write(services);

            // Timestamp
            UInt64 timestamp = Program.UnixTime();
            w.Write(timestamp);

            // addr_recv
            w.Write(services);
            w.Write((UInt64)0);
            w.Write((ushort)0);
            w.Write((ushort)0xFFFF);
            byte[] remoteBytes = remote.Address.GetAddressBytes();
            w.Write(remoteBytes);
            w.Write(Program.Byteswap((short)mPort));

            // addr_from
            w.Write(services);
            w.Write((UInt64)0);
            w.Write((ushort)0);
            w.Write((ushort)0xFFFF);
            byte[] localBytes = local.Address.GetAddressBytes();
            w.Write(localBytes);
            w.Write(Program.Byteswap((short)mPort));

            // nonce
            w.Write((UInt64)0xC4ACFF3D04805523);

            // user_agent
            w.Write((byte)0xF);
            w.Write("/Satoshi:0.8.6/".ToArray());

            // start_height
            w.Write(mOwner.mCurrentHeight);

            byte[] packetData = stream.ToArray();
            SendPacket("version", packetData);
            w.Close();
        }

        public void SendPacket(string packetID, byte[] payload)
        {
            byte[] hash = Program.GenerateHash(payload);

            MemoryStream stream = new MemoryStream();
            BinaryWriter w = new BinaryWriter(stream);

            w.Write(mNetworkID);

            for (int i = 0; i < 12; i++)
            {
                if (i < packetID.Length)
                    w.Write((byte)packetID[i]);
                else
                    w.Write((byte)0);
            }

            // Payload len
            w.Write(payload.Length);

            // Checksum
            w.Write(hash[0]);
            w.Write(hash[1]);
            w.Write(hash[2]);
            w.Write(hash[3]);

            w.Write(payload);

            byte[] packetData = stream.ToArray();
            /*
            Console.WriteLine("Send Packet - " + packetID);
            for (int i = 0; i < packetData.Length; i++)
            {
                Console.Write("{0:X2} ", packetData[i]);
                if (i > 0 && ((i + 1) % 16) == 0)
                    Console.Write("\n");
            }
            Console.Write("\n");
            */
            mSocket.Send(packetData);
            w.Close();
        }

        bool CompareChecksum(uint checksum, byte[] hash)
        {
            uint hashVal = (uint)(hash[3] << 24) | (uint)(hash[2] << 16) | (uint)(hash[1] << 8) | (uint)hash[0];
            return (hashVal == checksum);
        }

        public void RequestBlocks(BlockHeader[] headers)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);

            int count = headers.Length;
            Program.WriteVarInt(bw, (ulong)count);
            foreach (BlockHeader b in headers)
            {
                const int blockType = 2;
                bw.Write(blockType);
                bw.Write(b.mHash);
            }
            SendPacket("getdata", stream.ToArray());

            bw.Close();
        }

        public void RequestHeaders(byte[] origin)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);

            if (origin == null)
            {
                origin = new byte[32];
            }


            bw.Write(mProtocolVersion);
            Program.WriteVarInt(bw, (ulong)1);
            bw.Write(origin);
            
            byte[] zeros = new byte[32];
            bw.Write(zeros);
            SendPacket("getheaders", stream.ToArray());
            bw.Close();
        }

        void PH_Version(byte[] payload)
        {
            MemoryStream stream = new MemoryStream(payload);
            BinaryReader br = new BinaryReader(stream);

            mRemoteVersion = br.ReadUInt32();
            mRemoteServices = br.ReadUInt64();
            mRemoteTimestamp = br.ReadUInt64();

            {
                //uint addrA_time = br.ReadUInt32();
                ulong addrA_services = br.ReadUInt64();
                byte[] addrA_ip = br.ReadBytes(16);
                ushort addrA_port = br.ReadUInt16();
            }

            {
                //uint addrB_time = br.ReadUInt32();
                ulong addrB_services = br.ReadUInt64();
                byte[] addrB_ip = br.ReadBytes(16);
                ushort addrB_port = br.ReadUInt16();
            }
            mRemoteNodeNOnce = br.ReadUInt64();

            ulong nameLen = Program.ReadVarInt(br);
            char[] clientName = br.ReadChars((int)nameLen);
            mRemoteUserAgent = new string(clientName);

            mRemoteHeight = br.ReadUInt32();
            br.Close();

            // Send verack
            byte[] verackData = new byte[0];
            SendPacket("verack", verackData);

            mOwner.VersionRecieved(this);
            Console.WriteLine("Remote Version: " + mRemoteVersion);
        }

        void PH_Verack(byte[] payload)
        {
            Console.WriteLine("PH_Verack");
            mConnected = true;
        }

        void PH_Addr(byte[] payload)
        {
            Console.WriteLine("PH_Addr");
        }

        void PH_Inv(byte[] payload)
        {
            Console.WriteLine("PH_Inv");
            mOwner.HandleInvPacket(this, payload);
        }

        void PH_GetData(byte[] payload)
        {
            Console.WriteLine("PH_GetData");
        }

        void PH_NotFound(byte[] payload)
        {
            Console.WriteLine("PH_NotFound");
        }

        void PH_GetBlocks(byte[] payload)
        {
            Console.WriteLine("PH_GetBlocks");
        }

        void PH_GetHeaders(byte[] payload)
        {
            Console.WriteLine("PH_GetHeaders");
        }

        void PH_Tx(byte[] payload)
        {
            Console.WriteLine("PH_Tx");
        }

        void PH_Block(byte[] payload)
        {
            mOwner.HandleBlockPacket(this, payload);
        }

        void PH_Headers(byte[] payload)
        {
            mOwner.HandleHeadersPacket(this, payload);
        }

        void PH_GetAddr(byte[] payload)
        {
            Console.WriteLine("PH_GetAddr");
        }

        void PH_Mempool(byte[] payload)
        {
            Console.WriteLine("PH_Mempool");
        }

        void PH_CheckOrder(byte[] payload)
        {
            Console.WriteLine("PH_CheckOrder");
        }

        void PH_Subimtorder(byte[] payload)
        {
            Console.WriteLine("PH_Subimtorder");
        }

        void PH_Reply(byte[] payload)
        {
            Console.WriteLine("PH_Reply");
        }

        void PH_Ping(byte[] payload)
        {
            Console.WriteLine("PH_Ping");
        }

        void PH_Pong(byte[] payload)
        {
            Console.WriteLine("PH_Pong");
        }

        void PH_Alert(byte[] payload)
        {
            MemoryStream stream = new MemoryStream(payload);
            BinaryReader br = new BinaryReader(stream);
            UInt64 msgLen = Program.ReadVarInt(br);
            string msg = Program.ReadString(br, (int)msgLen);
            UInt64 sigLen = Program.ReadVarInt(br);
            string sig = Program.ReadString(br, (int)sigLen);
            Console.WriteLine("Alert: " + msg + " from: " + sig);
            br.Close();
        }

        int DisptachPacket(byte[] packet, long size)
        {
            MemoryStream stream = new MemoryStream(packet);
            BinaryReader br = new BinaryReader(stream);

            int bytesConsumed = 0;

            while (stream.Position + 24 < size)
            {
                uint networkID = br.ReadUInt32();
                string packetID = Program.ReadString(br, 12);
                uint length = br.ReadUInt32();
                uint checksum = br.ReadUInt32();
                if (stream.Position + length > size)
                {
                    break;
                }

                Console.WriteLine(packetID);

                if (networkID == mNetworkID)
                {
                    byte[] payload = br.ReadBytes((int)length);

                    byte[] hash = Program.GenerateHash(payload);
                    if (CompareChecksum(checksum, hash))
                    {
                        // valid packet, get the handler
                        if (mPacketProcessors.ContainsKey(packetID))
                        {
                            PacketProcessor p = mPacketProcessors[packetID];
                            p(payload);
                        }
                        else
                        {
                            Console.WriteLine("Unhandled packet: " + packetID);
                        }
                    }
                    else
                        Console.WriteLine("Failed checksum!");
                }
                else
                    Console.WriteLine("Failed ID Check, Network ID: " + networkID);

                bytesConsumed += 24 + (int)length;
            }

            br.Close();
            return bytesConsumed;
        }

        void ReadSocket()
        {
            byte[] readBuffer = new byte[1024 * 256];
            int offset = 0;
            while (true)
            {
                int recieved = mSocket.Receive(readBuffer, offset, readBuffer.Length - offset, SocketFlags.None);
                if (recieved > 0)
                {
                    int dataSize = offset + recieved;
                    int consumed = DisptachPacket(readBuffer, dataSize);
                    if (consumed < dataSize)
                    {
                        int remaining = dataSize - consumed;
                        byte[] temp = new byte[remaining];
                        Array.Copy(readBuffer, consumed, temp, 0, remaining);
                        Array.Copy(temp, readBuffer, remaining);
                        offset = remaining;
                    }
                    else
                        offset = 0;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace BitCoinClient
{
    public class NodeConnection
    {
        delegate void PacketProcessor(byte[] packetPayload);

        Dictionary<string, PacketProcessor> mPacketProcessors;
        Socket mSocket;
        Thread mThread;
        Bitcoin mOwner;

        const uint cMainNetwork = 0xD9B4BEF9;


        uint mRemoteVersion;
        ulong mRemoteServices;
        ulong mRemoteTimestamp;
        ulong mRemoteNodeID;
        string mRemoteUserAgent;
        uint mRemoteLastBlock;

        public NodeConnection(Bitcoin owner, string nodeAddress, ushort port = 8333)
        {
            mOwner = owner;

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

        void Connect(string nodeAddress, ushort port = 8333)
        {
            // Connect to remote host
            mSocket.Connect(nodeAddress, port);

            // Start the read thread
            mThread.Start();

            // Send version packet
            SendVersionPacket();
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
            w.Write(0x00011171);

            // Services
            Int64 services = 0x0000000000000001;
            w.Write(services);

            // Timestamp
            UInt64 timestamp = Program.UnixTime();
            timestamp = 0x52F95D8E;
            w.Write(timestamp);

            // addr_recv
            w.Write(services);
            w.Write((UInt64)0);
            w.Write((ushort)0);
            w.Write((ushort)0xFFFF);
            byte[] remoteBytes = remote.Address.GetAddressBytes();
            w.Write(remoteBytes);
            w.Write(Program.Byteswap((short)8333));

            // addr_from
            w.Write(services);
            w.Write((UInt64)0);
            w.Write((ushort)0);
            w.Write((ushort)0xFFFF);
            byte[] localBytes = local.Address.GetAddressBytes();
            w.Write(localBytes);
            w.Write(Program.Byteswap((short)8333));

            // nonce
            w.Write((UInt64)0xC4ACFF3D04805523);

            // user_agent
            w.Write((byte)0xF);
            w.Write("/Satoshi:0.8.6/".ToArray());

            // start_height
            w.Write((int)0x00045768);

            byte[] packetData = stream.ToArray();
            SendPacket("version", packetData);
            w.Close();
        }

        void SendPacket(string packetID, byte[] payload)
        {
            byte[] hash = Program.GenerateHash(payload);

            MemoryStream stream = new MemoryStream();
            BinaryWriter w = new BinaryWriter(stream);

            w.Write(cMainNetwork);

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

        public void RequestData(List<Transaction> transactions, List<Block> blocks)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);

            int count = transactions.Count + blocks.Count;
            Program.WriteVarInt(bw, (ulong)count);
            foreach (Transaction t in transactions)
            {
                const int transactionType = 1;
                bw.Write(transactionType);
                bw.Write(t.Hash);
            }
            foreach (Block b in blocks)
            {
                const int blockType = 2;
                bw.Write(blockType);
                bw.Write(b.Hash);
            }
            SendPacket("getdata", stream.ToArray());

            bw.Close();
        }

        void PH_Version(byte[] payload)
        {
            MemoryStream stream = new MemoryStream(payload);
            BinaryReader br = new BinaryReader(stream);

            mRemoteVersion = br.ReadUInt32();
            mRemoteServices = br.ReadUInt64();
            mRemoteTimestamp = br.ReadUInt64();
            mRemoteNodeID = br.ReadUInt64();

            byte clientNameLen = br.ReadByte();
            char[] clientName = br.ReadChars(clientNameLen);
            mRemoteUserAgent = new string(clientName);

            mRemoteLastBlock = br.ReadUInt32();
            br.Close();

            // Send verack
            byte[] verackData = new byte[0];
            SendPacket("verack", verackData);
        }

        void PH_Verack(byte[] payload)
        {
        }

        void PH_Addr(byte[] payload)
        {
            Console.WriteLine("PH_Addr");
        }

        void PH_Inv(byte[] payload)
        {
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
            mOwner.LoadTransaction(payload);
        }

        void PH_Block(byte[] payload)
        {
            Console.WriteLine("PH_Block");
        }

        void PH_Headers(byte[] payload)
        {
            Console.WriteLine("PH_Headers");
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
            Console.WriteLine("PH_Alert");
        }

        void DisptachPacket(byte[] packet)
        {
            MemoryStream stream = new MemoryStream(packet);
            BinaryReader br = new BinaryReader(stream);

            uint magic = br.ReadUInt32();
            string packetID = Program.ReadString(br, 12);
            uint length = br.ReadUInt32();
            uint checksum = br.ReadUInt32();

            if (magic == cMainNetwork)
            {
                byte[] payload = br.ReadBytes((int)length);

                byte[] hash = Program.GenerateHash(payload);
                if (CompareChecksum(checksum, hash))
                {
                    // valid packet, get the handler
                    if( mPacketProcessors.ContainsKey(packetID) )
                    {
                        PacketProcessor p = mPacketProcessors[packetID];
                        p(payload);
                    }
                    else
                    {
                        Console.WriteLine("Unhandled packet: " + packetID);
                    }
                }
            }

            br.Close();
        }

        void ReadSocket()
        {
            byte[] readBuffer = new byte[1024 * 8];
            while (true)
            {
                int recieved = mSocket.Receive(readBuffer);
                if (recieved > 0)
                {
                    byte[] packet = new byte[recieved];
                    Array.Copy(readBuffer, packet, recieved);
                    DisptachPacket(packet);
                }
            }
        }
    }
}

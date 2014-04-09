using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace HashrateCalculator
{
    public class Bitcoin
    {
        Dictionary<string, NodeConnection> mNodes;
        
        bool mDownloadingHeaders = false;
        List<BlockHeader> mHeaders;

        public Bitcoin()
        {
            mNodes = new Dictionary<string, NodeConnection>();
            mHeaders = new List<BlockHeader>();
        }

        public void AddNode(string nodeAddr, ushort nodePort = 8333, uint version = 70001, uint networkID = 0xD9B4BEF9)
        {
            string key = nodeAddr + "_" + nodePort.ToString();

            if (!mNodes.ContainsKey(key))
            {
                NodeConnection node = new NodeConnection(this, nodeAddr, nodePort, version, networkID);
                mNodes[key] = node;
            }
        }

        public bool IsConnected()
        {
            foreach (KeyValuePair<string, NodeConnection> kvp in mNodes)
            {
                if (kvp.Value.IsConnected())
                    return true;
            }

            return false;
        }

        public void HandleInvPacket(NodeConnection node, byte[] payload)
        {
            
        }

        public void HandleHeadersPacket(NodeConnection node, byte[] payload)
        {
            MemoryStream stream = new MemoryStream(payload);
            BinaryReader br = new BinaryReader(stream);

            ulong count = Program.ReadVarInt(br);
            for (ulong i = 0; i < count; i++)
            {
                uint version = br.ReadUInt32();
                byte[] prevBlock = br.ReadBytes(32);
                byte[] merkle = br.ReadBytes(32);
                uint time = br.ReadUInt32();
                uint bits = br.ReadUInt32();
                uint nonce = br.ReadUInt32();
                ulong txn = Program.ReadVarInt(br);

                if (node.mProtocolVersion >= 60012 && node.mProtocolVersion < 70000)
                {
                    ulong siglen = Program.ReadVarInt(br);
                }

                BlockHeader header = new BlockHeader(version, prevBlock, merkle, time, bits, nonce);
                if (mHeaders.Count > 0)
                    mHeaders[mHeaders.Count - 1].mHash = prevBlock;
                mHeaders.Add(header);
            }
            br.Close();

            if (count < 2000)
                mDownloadingHeaders = false;
            else
            {
                node.RequestHeaders(mHeaders[mHeaders.Count - 1].mPrevBlock);
                mHeaders.RemoveAt(mHeaders.Count - 1);
            }
            Console.WriteLine("HeaderCount: " + mHeaders.Count);
        }

        public void RequestHeaders(byte[] inputHash)
        {
            if (!mDownloadingHeaders)
            {
                NodeConnection node = mNodes.First().Value;
                if (node != null)
                {
                    if (inputHash != null)
                    {
                        node.RequestHeaders(inputHash);
                    }
                    else
                    {
                        byte[] zeros = new byte[32];
                        node.RequestHeaders(zeros);
                    }
                    mDownloadingHeaders = true;
                }
            }
        }

        public void WaitForHeaders()
        {
            Console.WriteLine("Waiting for headers");
            while (mDownloadingHeaders)
            {
                Thread.Sleep(50);
            }
            Console.WriteLine("Done fetching headers");
        }

        public BlockHeader[] GetRecentBlockHeaders(bool ignoreDiffDifference)
        {
            List<BlockHeader> blocks = new List<BlockHeader>();

            uint diff = mHeaders[mHeaders.Count - 1].mDifficultyBits;
            UInt64 now = Program.UnixTime();
            const UInt64 oneDay = 60 * 60 * 24;
            int i;
            for (i = mHeaders.Count - 1; i >= 0; i--)
            {
                if (now < mHeaders[i].mTimestamp)
                    continue;
                if (now - mHeaders[i].mTimestamp > oneDay || (mHeaders[i].mDifficultyBits != diff && !ignoreDiffDifference))
                {
                    i++;
                    break;
                }
            }
            if (i < 0)
                i = 0;

            for (; i < mHeaders.Count; i++)
            {
                blocks.Add(mHeaders[i]);   
            }

            return blocks.ToArray();
        }
    }
}

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
        NodeConnection mLastNode = null;
        public uint mCurrentHeight = 0;
        List<Block> mBlocks;
        Mutex mBlockLock;

        bool mPullingHeaders = false;

        // Maximum age of blocks kept in memory
        public ulong mHistoryTime = ((60 * 60 * 24) * 365) * 10;
                
        public bool mWantsTransactionData = false;
        public bool mIgnoreSigLen = false;
        public bool mScryptBlockHash = false;
        
        public Bitcoin()
        {
            mNodes = new Dictionary<string, NodeConnection>();
            mBlocks = new List<Block>();
            mBlockLock = new Mutex();
        }

        public void Destroy()
        {
            foreach (KeyValuePair<string, NodeConnection> node in mNodes)
            {
                node.Value.Destroy();
            }
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

        public void RemoveOldBlocks()
        {
            mBlockLock.WaitOne();
            UInt64 oldest = Program.UnixTime() - mHistoryTime;
            List<Block> removeList = new List<Block>();
            foreach (Block b in mBlocks)
            {
                if (b.mHeader.mTimestamp < oldest)
                    removeList.Add(b);
            }
            if (removeList.Count == mBlocks.Count)
                mBlocks.Clear();
            else
            {
                foreach (Block b in removeList)
                {
                    mBlocks.Remove(b);
                }
            }
            mBlockLock.ReleaseMutex();
        }

        public Block[] GetBlocks()
        {
            if (mPullingHeaders)
                return null;

            RemoveOldBlocks();
            PullHeaders();
            return mBlocks.ToArray();
        }

        void PullHeaders()
        {
            if (mLastNode != null && !mPullingHeaders)
            {
                // Get headers
                mPullingHeaders = true;
                byte[] hash = null;
                if (mBlocks.Count > 0)
                    hash = mBlocks[mBlocks.Count - 1].mHeader.mHash;
                mLastNode.RequestHeaders(hash);
            }
        }

        public void VersionRecieved(NodeConnection node)
        {
            mLastNode = node;
            PullHeaders();           
        }

        public Block FindBlock(string hash)
        {
            Block retBlock = null;
            mBlockLock.WaitOne();
            foreach (Block b in mBlocks)
            {
                if (b.mHash == hash)
                {
                    retBlock = b;
                    break;
                }
            }
            mBlockLock.ReleaseMutex();
            return retBlock;
        }

        public void HandleHeadersPacket(NodeConnection node, byte[] payload)
        {
            MemoryStream stream = new MemoryStream(payload);
            BinaryReader br = new BinaryReader(stream);

            List<BlockHeader> headers = new List<BlockHeader>();

            ulong count = Program.ReadVarInt(br);
            if (count == 0)
            {
                mPullingHeaders = false;
                return;
            }
            for (ulong i = 0; i < count; i++)
            {
                uint version = br.ReadUInt32();
                byte[] prevBlock = br.ReadBytes(32);
                byte[] merkle = br.ReadBytes(32);
                uint time = br.ReadUInt32();
                uint bits = br.ReadUInt32();
                uint nonce = br.ReadUInt32();
                ulong txn = Program.ReadVarInt(br);

                if( !mIgnoreSigLen )
                {
                    ulong siglen = Program.ReadVarInt(br);
                }

                BlockHeader header = new BlockHeader(version, prevBlock, merkle, time, bits, nonce);
                if (headers.Count > 0)
                    headers[headers.Count - 1].mHash = prevBlock;
                headers.Add(header);
            }
            br.Close();

            if( headers[0].mHash != null )
                headers[0].ValidateHash();

            mBlockLock.WaitOne();
            foreach (BlockHeader h in headers)
            {
                Block b = new Block();
                b.mHeader = h;
                if (b.mHeader.mHash == null)
                    b.mHeader.ComputeHash(mScryptBlockHash);
                b.mHash = Program.HashToString(b.mHeader.mHash);
                mBlocks.Add(b);
            }
            mCurrentHeight += (uint)headers.Count;

            if (count < 2000)
            {
                // End of the list
                // Remove all blocks that are older than we care about
                RemoveOldBlocks();

                // If we want transaction data, request the full blocks
                if (mWantsTransactionData)
                {
                    headers.Clear();
                    mBlockLock.WaitOne();
                    foreach( Block b in mBlocks )
                    {
                        if( b.mTransactions.Count < 1)
                            headers.Add(b.mHeader);
                    }
                    mBlockLock.ReleaseMutex();
                    node.RequestBlocks(headers.ToArray());
                }
                mPullingHeaders = false;
            }
            else
            {
                // Request again til we get all of them
                node.RequestHeaders(mBlocks[mBlocks.Count - 1].mHeader.mHash);
            }
            mBlockLock.ReleaseMutex();

            Console.WriteLine("Blocks: " + mCurrentHeight);
        }

        public void HandleBlockPacket(NodeConnection node, byte[] payload)
        {
            MemoryStream stream = new MemoryStream(payload);
            BinaryReader br = new BinaryReader(stream);

            uint version = br.ReadUInt32();
            byte[] prevBlock = br.ReadBytes(32);
            byte[] merkle = br.ReadBytes(32);
            uint time = br.ReadUInt32();
            uint bits = br.ReadUInt32();
            uint nonce = br.ReadUInt32();
            ulong txn = Program.ReadVarInt(br);
            
            BlockHeader h = new BlockHeader(version, prevBlock, merkle, time, bits, nonce);
            h.ComputeHash(mScryptBlockHash);
            string hash = Program.HashToString(h.mHash);
            
            Block block = FindBlock(hash);            
            if( block == null )
            {
                // Create a new block
                block = new Block();
                block.mHeader = h;
                mBlockLock.WaitOne();
                mBlocks.Add(block);
                mBlockLock.ReleaseMutex();
            }

            block.mTransactions.Clear();
            if (mWantsTransactionData)
            {
                for (ulong j = 0; j < txn; j++)
                {
                    Transaction tx = new Transaction();
                    tx.mVersion = br.ReadUInt32();
                    tx.mTimestamp = br.ReadUInt32();

                    ulong inCount = Program.ReadVarInt(br);
                    for (ulong k = 0; k < inCount; k++)
                    {
                        TransactionInput ti = new TransactionInput();
                        ti.mPrevOuptutHash = br.ReadBytes(32);
                        ti.mPrevOutputIndex = br.ReadUInt32();

                        ulong scriptLen = Program.ReadVarInt(br);
                        ti.mScript = br.ReadBytes((int)scriptLen);

                        ti.mSequence = br.ReadUInt32();

                        tx.mInputs.Add(ti);
                    }

                    ulong outCount = Program.ReadVarInt(br);
                    for (ulong k = 0; k < outCount; k++)
                    {
                        TransactionOutput to = new TransactionOutput();
                        to.mValue = br.ReadUInt64();
                        to.mRealValue = (double)to.mValue / 1000000.0;

                        ulong scriptLen = Program.ReadVarInt(br);
                        to.mScript = br.ReadBytes((int)scriptLen);

                        tx.mOutputs.Add(to);
                    }
                    tx.mLockTime = br.ReadUInt32();
                    if (tx.mVersion > 1)
                    {
                        ulong commentLen = Program.ReadVarInt(br);
                        tx.mComment = br.ReadBytes((int)commentLen);
                    }

                    block.mTransactions.Add(tx);
                }
            }
            br.Close();
        }

        public void HandleInvPacket(NodeConnection node, byte[] payload)
        {
            /*
            MemoryStream ms = new MemoryStream(payload);
            BinaryReader br = new BinaryReader(ms);

            MemoryStream outstream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(outstream);

            ulong count = Program.ReadVarInt(br);
            for (ulong i = 0; i < count; i++)
            {
                uint type = br.ReadUInt32();
                byte[] hash = br.ReadBytes(32);
                if (type == 2)  // block
                {
                    string hashStr = Program.HashToString(hash);
                    Block block = FindBlock(hashStr);
                    if (block == null)
                    {
                        // A block we dont have - request the block
                        bw.Write(type);
                        bw.Write(hash);
                    }
                }
            }

            br.Close();

            if (outstream.Length > 0)
            {
                node.SendPacket("getdata", outstream.ToArray());
            }
            */
        }

        public void Load(BinaryReader br)
        {
            int version = br.ReadInt32();
            mCurrentHeight = br.ReadUInt32();

            mBlockLock.WaitOne();
            mBlocks.Clear();
            int blockCount = br.ReadInt32();
            for (int i = 0; i < blockCount; i++)
            {
                BlockHeader h = new BlockHeader();
                h.Load(br);
                Block b = new Block();
                b.mHeader = h;
                b.mHash = Program.HashToString(h.mHash);
                mBlocks.Add(b);
            }
            mBlockLock.ReleaseMutex();
        }

        public void Save(BinaryWriter bw)
        {
            const int version = 1;
            bw.Write(version);
            bw.Write(mCurrentHeight);

            mBlockLock.WaitOne();
            bw.Write(mBlocks.Count);
            foreach (Block b in mBlocks)
            {
                b.mHeader.Save(bw);
            }
            mBlockLock.ReleaseMutex();
        }
    }
}

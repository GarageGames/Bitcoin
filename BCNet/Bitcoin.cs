using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace BCNet
{
    public class Bitcoin
    {
        Thread mThread;
        bool mGettingHeaders = false;
        DateTime mLastHeaderRequest;
        List<BlockHeader> mPendingHeaders;
        bool mPickedHashType = false;
        bool mUseScryptHash;

        NodeConnection mLastNode;
        List<NodeConnection> mDeadNodes;
        List<NodeConnection> mNodes;
        Mutex mNodeLock;
        List<Block> mBlocks;
        Mutex mBlockLock;
        StreamWriter mLogWriter = null;

        int mCurrentHeight;
        int mMemoryHeight;
        int mDiskHeight;
        int mArchiveMarker;

        string mName;
        string mDataPath;

        uint mNetworkVersion = 0;
        uint mNetworkID;
        bool mIgnoreSigLen;
        bool mTransactionTimeStamp;
        string mIRCChannelName;

        public Bitcoin(string name, string path)
        {
            mArchiveMarker = 0;

            mDeadNodes = new List<NodeConnection>();
            mNodes = new List<NodeConnection>();
            mNodeLock = new Mutex();
            mBlocks = new List<Block>();
            mBlockLock = new Mutex();

            mName = name;
            mDataPath = path;

            LoadInfo();

            mThread = new Thread(new ThreadStart(BCUpdateThread));
            mThread.Name = name;
            mThread.Start();
        }

        public void Destroy()
        {
            foreach (NodeConnection node in mNodes)
            {
                node.Destroy();
            }

            if (mLogWriter != null)
            {
                mLogWriter.Close();
                mLogWriter = null;
            }
            mThread.Abort();

            FlushBlocks();
        }

        public void AddNode(string address, ushort port)
        {
            mNodeLock.WaitOne();
            foreach (NodeConnection n in mNodes)
            {
                if (n.Address == address && n.Port == port)
                    return;
            }

            NodeConnection node = new NodeConnection(this, address, port, mNetworkVersion, mNetworkID);
            mNodes.Add(node);
            mNodeLock.ReleaseMutex();
        }

        void Log(string logLine)
        {
            try
            {
                if (mLogWriter == null)
                {
                    FileStream fs = File.OpenWrite(mDataPath + "/log.txt");
                    fs.Seek(0, SeekOrigin.End);
                    mLogWriter = new StreamWriter(fs);
                    mLogWriter.AutoFlush = true;
                }
                mLogWriter.WriteLine(logLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        void LoadInfo()
        {
            try
            {
                FileStream fs = File.OpenRead(mDataPath + "/info");
                StreamReader sr = new StreamReader(fs);

                int infoVersion = Convert.ToInt32(sr.ReadLine());
                mNetworkVersion = Convert.ToUInt32(sr.ReadLine());
                mNetworkID = Convert.ToUInt32(sr.ReadLine());
                mIgnoreSigLen = (sr.ReadLine() != "0");
                mTransactionTimeStamp = (sr.ReadLine() != "0");
                mIRCChannelName = sr.ReadLine();

                sr.Close();
            }
            catch (Exception ex)
            {
                Log("Failed to load info file\n" + ex.Message);
            }
        }

        void LoadBlocks()
        {
            mBlocks.Clear();
            mCurrentHeight = 1;
            mArchiveMarker = 0;

            while (true)
            {
                string filename = String.Format("/{0:D5}.blocks", mArchiveMarker / 10000);
                string filepath = mDataPath + filename;
                if (!File.Exists(filepath))
                {
                    // File doesnt exist, likely didnt hit a partial block so load the last archive chunk
                    if (mArchiveMarker > 0 && mBlocks.Count == 0)
                    {
                        filename = String.Format("/{0:D5}.blocks", (mArchiveMarker - 10000) / 10000);
                        filepath = mDataPath + filename;
                        try
                        {
                            FileStream fs = File.OpenRead(filepath);
                            BinaryReader br = new BinaryReader(fs);

                            int version = br.ReadInt32();
                            int blockCount = br.ReadInt32();
                            mCurrentHeight -= 10000;
                            for (int i = 0; i < blockCount; i++)
                            {
                                Block b = new Block();
                                b.Load(br);
                                b.mHeight = mCurrentHeight++;
                                mBlocks.Add(b);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    break;
                }

                try
                {
                    FileStream fs = File.OpenRead(filepath);
                    BinaryReader br = new BinaryReader(fs);

                    int version = br.ReadInt32();
                    int blockCount = br.ReadInt32();
                    if( blockCount == 10000 || (blockCount == 9999 && mArchiveMarker == 0 ) )
                    {
                        // This is a fully archived chunk, just skip past it
                        mCurrentHeight += 10000;
                        mArchiveMarker += 10000;
                    }
                    else
                    {
                        // Load all the blocks in this chunk
                        for (int i = 0; i < blockCount; i++)
                        {
                            Block b = new Block();
                            b.Load(br);
                            b.mHeight = mCurrentHeight++;
                            mBlocks.Add(b);
                        }

                        // Break out of the larger loop since this is an incomplete chunk it must be the last one.
                        break;
                    }
                    

                    br.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }

        void SaveInfo()
        {
            try
            {
                FileStream fs = File.Open(mDataPath + "/info", FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);

                sw.WriteLine("0");                              // Info Version
                sw.WriteLine(mNetworkVersion);                  // BCNet protocol version
                sw.WriteLine(mNetworkID);                       // BCNet magic value
                sw.WriteLine(mIgnoreSigLen ? "1" : "0");
                sw.WriteLine(mTransactionTimeStamp ? "1" : "0");
                sw.WriteLine(mIRCChannelName);

                sw.Close();
            }
            catch (Exception ex)
            {
                Log("Failed to save info\n" + ex.Message);
            }
        }

        void MergePendingHeaders()
        {
            mBlockLock.WaitOne();
            if (mBlocks.Count > 0)
            {
                for (int i = mBlocks.Count - 1; i >= 0; i--)
                {
                    if (Utils.HashCompare(mBlocks[i].mHeader.mHash, mPendingHeaders[0].mPrevBlock))
                    {
                        if (i < (mBlocks.Count - 1))
                        {
                            Console.WriteLine("Stop here and figure out how to handle this case!!!");
                        }
                        else
                        {
                            foreach (BlockHeader h in mPendingHeaders)
                            {
                                Block b = new Block();
                                b.mHeader = h;
                                mBlocks.Add(b);
                                b.mHeight = mCurrentHeight++;
                            }
                        }
                    }
                }
            }
            else
            {
                // No blocks yet, just dump all these into the block list
                foreach (BlockHeader h in mPendingHeaders)
                {
                    Block b = new Block();
                    b.mHeader = h;
                    mBlocks.Add(b);
                    b.mHeight = mCurrentHeight++;
                }
            }
            mBlockLock.ReleaseMutex();            
        }

        void RequestBlocks()
        {
            List<BlockHeader> requests = new List<BlockHeader>();
            mBlockLock.WaitOne();
            foreach (Block b in mBlocks)
            {
                if (b.mTransactions.Count == 0)
                {
                    TimeSpan s = DateTime.Now - b.mLastRequestTime;
                    if (s.TotalSeconds > 30)
                    {
                        requests.Add(b.mHeader);
                        b.mLastRequestTime = DateTime.Now;
                    }
                }
            }
            mBlockLock.ReleaseMutex();

            if (requests.Count > 0)
            {
                mLastNode.RequestBlocks(requests.ToArray());
            }
        }

        void WriteArchive(List<Block> archive)
        {
            try
            {
                string filename = String.Format("/{0:D5}.blocks", mArchiveMarker / 10000);
                FileStream fs = File.Open(mDataPath + filename, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);

                int version = 0;
                bw.Write(version);
                bw.Write(archive.Count);

                for (int i = 0; i < archive.Count; i++)
                {
                    int height = mArchiveMarker + i;
                    if (height == 0)
                        continue;
                    foreach (Block b in archive)
                    {
                        if (b.mHeight == height)
                        {
                            b.Save(bw);
                            mBlocks.Remove(b);
                            break;
                        }
                    }
                }

                bw.Close();
                mArchiveMarker += 10000;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Archive issue: " + ex.Message);
            }
        }

        void ArchiveBlocks()
        {
            if (mBlocks.Count > 10000)
            {
                mBlockLock.WaitOne();

                int archiveEnd = mArchiveMarker + 10000;
                List<Block> archive = new List<Block>();
                foreach (Block b in mBlocks)
                {
                    if (b.mHeight >= mArchiveMarker && b.mHeight < archiveEnd)
                    {
                        if( b.mTransactions.Count > 0 )
                            archive.Add(b);
                    }
                }

                if (archive.Count == 10000 || (archive.Count == 9999 && mArchiveMarker == 0))
                {
                    WriteArchive(archive);                    
                }

                mBlockLock.ReleaseMutex();
            }
        }

        void FlushBlocks()
        {
            // Write remaining blocks to the disk
            mBlockLock.WaitOne();

            int archiveEnd = mArchiveMarker + 10000;
            List<Block> archive = new List<Block>();
            foreach (Block b in mBlocks)
            {
                if (b.mHeight >= mArchiveMarker && b.mHeight < archiveEnd)
                    archive.Add(b);
            }

            WriteArchive(archive);
            mBlockLock.ReleaseMutex();
        }

        Block FindBlock(byte[] hash)
        {
            Block found = null;
            mBlockLock.WaitOne();
            foreach (Block b in mBlocks)
            {
                if (Utils.HashCompare(b.mHeader.mHash, hash))
                {
                    found = b;
                    break;
                }
            }
            mBlockLock.ReleaseMutex();

            return found;
        }

        void BCUpdateThread()
        {
            LoadBlocks();

            while (true)
            {
                if (mDeadNodes.Count > 0)
                {
                    mNodeLock.WaitOne();
                    foreach (NodeConnection n in mDeadNodes)
                    {
                        n.Destroy();
                    }
                    mDeadNodes.Clear();
                    mNodeLock.ReleaseMutex();
                }

                // Merge pending headers
                if (mPendingHeaders != null)
                {
                    MergePendingHeaders();
                    mPendingHeaders = null;
                    mGettingHeaders = false;
                }

                // Check for larger block chains
                if (!mGettingHeaders)
                {
                    mNodeLock.WaitOne();
                    foreach (NodeConnection n in mNodes)
                    {
                        if (n.mRemoteHeight > mCurrentHeight)
                        {
                            byte[] originHash = null;
                            if (mBlocks.Count > 0)
                            {
                                mBlockLock.WaitOne();
                                originHash = mBlocks[mBlocks.Count - 1].mHeader.mHash;
                                mBlockLock.ReleaseMutex();
                            }
                            n.RequestHeaders(originHash);
                            Console.WriteLine("Requesting Headers " + mCurrentHeight + " / " + n.mRemoteHeight);
                            mGettingHeaders = true;
                            mLastHeaderRequest = DateTime.Now;
                            break;
                        }
                    }
                    mNodeLock.ReleaseMutex();
                }
                else
                {
                    TimeSpan s = DateTime.Now - mLastHeaderRequest;
                    if (s.TotalSeconds > 15)
                    {
                        // Abandon this request and move this connection to the bottom
                        mNodeLock.WaitOne();
                        NodeConnection n = mNodes[0];
                        mNodes.RemoveAt(0);
                        mNodes.Add(n);
                        mNodeLock.ReleaseMutex();
                        n.FetchingHeaders = false;
                        mGettingHeaders = false;
                    }
                }

                RequestBlocks();

                ArchiveBlocks();

                Thread.Sleep(100);
            }
        }

        #region PacketHandlers
        public void VersionRecieved(NodeConnection from)
        {
        }

        public void HandleInvPacket(NodeConnection from, byte[] payload)
        {
        }

        public void HandleBlockPacket(NodeConnection from, byte[] payload)
        {
            MemoryStream stream = new MemoryStream(payload);
            BinaryReader br = new BinaryReader(stream);

            uint version = br.ReadUInt32();
            byte[] prevBlock = br.ReadBytes(32);
            byte[] merkle = br.ReadBytes(32);
            uint time = br.ReadUInt32();
            uint bits = br.ReadUInt32();
            uint nonce = br.ReadUInt32();
            ulong txn = Utils.ReadVarInt(br);

            BlockHeader h = new BlockHeader(version, prevBlock, merkle, time, bits, nonce);
            h.ComputeHash(mUseScryptHash);

            Block block = FindBlock(h.mHash);
            if (block == null)
            {
                // Create a new block
                string hash = Utils.ByteArrayToHexString(h.mHash);
                Console.WriteLine("does a new block ever get introduced this way!?? " + hash);
                return;
            }

            block.mTransactions.Clear();
            for (ulong j = 0; j < txn; j++)
            {
                Transaction tx = new Transaction();
                tx.mVersion = br.ReadUInt32();
                if( mTransactionTimeStamp )
                    tx.mTimestamp = br.ReadUInt32();

                ulong inCount = Utils.ReadVarInt(br);
                for (ulong k = 0; k < inCount; k++)
                {
                    TransactionInput ti = new TransactionInput();
                    ti.mPrevOuptutHash = br.ReadBytes(32);
                    ti.mPrevOutputIndex = br.ReadUInt32();

                    ulong scriptLen = Utils.ReadVarInt(br);
                    ti.mScript = br.ReadBytes((int)scriptLen);

                    ti.mSequence = br.ReadUInt32();

                    tx.mInputs.Add(ti);
                }

                ulong outCount = Utils.ReadVarInt(br);
                for (ulong k = 0; k < outCount; k++)
                {
                    TransactionOutput to = new TransactionOutput();
                    to.mValue = br.ReadUInt64();
                    to.mRealValue = (double)to.mValue / 1000000.0;

                    ulong scriptLen = Utils.ReadVarInt(br);
                    to.mScript = br.ReadBytes((int)scriptLen);

                    tx.mOutputs.Add(to);
                }
                tx.mLockTime = br.ReadUInt32();
                if (tx.mVersion > 1)
                {
                    ulong commentLen = Utils.ReadVarInt(br);
                    tx.mComment = br.ReadBytes((int)commentLen);
                }

                block.mTransactions.Add(tx);
            }            
            br.Close();
        }

        public void HandleHeadersPacket(NodeConnection from, byte[] payload)
        {
            MemoryStream stream = new MemoryStream(payload);
            BinaryReader br = new BinaryReader(stream);

            List<BlockHeader> headers = new List<BlockHeader>();

            ulong count = Utils.ReadVarInt(br);
            Console.WriteLine("Got Headers: " + count);
            for (ulong i = 0; i < count; i++)
            {
                uint version = br.ReadUInt32();
                byte[] prevBlock = br.ReadBytes(32);
                byte[] merkle = br.ReadBytes(32);
                uint time = br.ReadUInt32();
                uint bits = br.ReadUInt32();
                uint nonce = br.ReadUInt32();
                ulong txn = Utils.ReadVarInt(br);

                if (!mIgnoreSigLen)
                {
                    ulong siglen = Utils.ReadVarInt(br);
                    br.ReadBytes((int)siglen);
                }

                BlockHeader header = new BlockHeader(version, prevBlock, merkle, time, bits, nonce);
                if (headers.Count > 0)
                    headers[headers.Count - 1].mHash = prevBlock;
                headers.Add(header);
            }
            br.Close();

            if (!mPickedHashType)
            {
                mUseScryptHash = headers[0].ValidateHash();
                mPickedHashType = true;
            }

            headers[headers.Count - 1].ComputeHash(mUseScryptHash);

            mPendingHeaders = headers;
            mLastNode = from;
        }

        public void BadConnection(NodeConnection n)
        {
            mNodeLock.WaitOne();
            mNodes.Remove(n);
            bool alreadyDead = false;
            for (int i = 0; i < mDeadNodes.Count; i++)
            {
                if (mDeadNodes[i] == n)
                {
                    alreadyDead = true;
                    break;
                }
            }
            if( !alreadyDead )
                mDeadNodes.Add(n);
            mNodeLock.ReleaseMutex();
        }
        #endregion

        public void NodeDiscovered(string node)
        {
            byte[] decoded = Utils.DecodeBase58StringChecked(node.Substring(1));
            if (decoded == null)
                return;

            MemoryStream ms = new MemoryStream(decoded);
            BinaryReader br = new BinaryReader(ms);

            byte[] addrBytes = br.ReadBytes(4);
            string address = "";
            for( int i = 0; i < 4; i++ )
            {
                //byte b = addrBytes[3 - i];
                byte b = addrBytes[i];
                address += b.ToString();
                if( i < 3 )
                    address += ".";
            }
            ushort port = Utils.Byteswap(br.ReadUInt16());

            AddNode(address, port);
            br.Close();
        }


        #region Properties
        public int CurrentHeight
        {
            get { return mCurrentHeight; }
        }

        public string IRCChannelName
        {
            get { return mIRCChannelName; }
        }

        public string Name
        {
            get { return mName; }
        }
        #endregion
    }
}

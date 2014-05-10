using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace HashrateCalculator
{
    class Currency
    {
        Bitcoin mBC = null;

        public string mName;
        public string mIPAddress;
        public ushort mPort;
        public uint mNetVersion = 60001;
        public uint mNetID = 3686187259;
        public uint mBlockTarget;

        public string mSymbol;
        public string mExchangeURL;
        public bool mPOS = false;
        public bool mIgnoreSigLen = true;
        public bool mScryptBlockHash = false;

        public double mCoinsPerBlock = 0;
        public double mBlocksPerDay = 0;
        public double mHashrate;
        public double mBTCPerCoin;
        public double mUSDPerCoin;
        public double mUSDPerDay;

        public string mStatus;

        const int mFileVersion = 1;

        public Currency()
        {
            mBC = new Bitcoin();
            mBC.mHistoryTime = 24 * 60 * 60;        // 1 day
            mStatus = "";
        }

        public void Destroy()
        {
            if( mBC != null )
                mBC.Destroy();
        }

        public bool HasStaticData
        {
            get
            {
                return (mIPAddress != null) && mPort != 0 && mNetVersion != 0 && mNetID != 0 && mBlockTarget != 0;
            }
        }

        public override string ToString()
        {
            return mName;
        }

        public void Load(string path)
        {
            string filename = path + mName;

            FileStream f = File.OpenRead(filename);
            StreamReader sr = new StreamReader(f);
                      
            int version = Convert.ToInt32(sr.ReadLine());

            mIPAddress = sr.ReadLine();
            mPort = Convert.ToUInt16(sr.ReadLine());
            mNetVersion = Convert.ToUInt32(sr.ReadLine());
            mNetID = Convert.ToUInt32(sr.ReadLine());
            mBlockTarget = Convert.ToUInt32(sr.ReadLine());
            mBlocksPerDay = (60.0 * 60.0 * 24.0) / (double)mBlockTarget;
            
            if (version > 0)
            {
                mSymbol = sr.ReadLine();
                mExchangeURL = sr.ReadLine();
                mCoinsPerBlock = Convert.ToDouble(sr.ReadLine());
                mPOS = (sr.ReadLine() != "0");
                mIgnoreSigLen = (sr.ReadLine() != "0");
                mScryptBlockHash = (sr.ReadLine() != "0");
            }

            sr.Close();  

            // Load Block headers
            if (HasStaticData)
            {
                mBC.mWantsTransactionData = mPOS;
                mBC.mIgnoreSigLen = mIgnoreSigLen;
                mBC.mScryptBlockHash = mScryptBlockHash;

                try
                {
                    if (!Directory.Exists(path + "headers/"))
                        Directory.CreateDirectory(path + "headers/");
                    filename = path + "headers/" + mName + ".headers";
                    f = File.OpenRead(filename);
                    BinaryReader br = new BinaryReader(f);

                    mBC.Load(br);

                    br.Close();
                }
                catch (Exception)
                {
                }
                
                mBC.AddNode(mIPAddress, mPort, mNetVersion, mNetID);
            }
        }

        public void Save(string path)
        {
            string filename = path + mName;
            FileStream f = File.Open(filename, FileMode.Create);
            StreamWriter sw = new StreamWriter(f);
            sw.WriteLine(mFileVersion);

            sw.WriteLine(mIPAddress);
            sw.WriteLine(mPort);
            sw.WriteLine(mNetVersion);
            sw.WriteLine(mNetID);
            sw.WriteLine(mBlockTarget);

            sw.WriteLine(mSymbol);
            sw.WriteLine(mExchangeURL);
            sw.WriteLine(mCoinsPerBlock);
            sw.WriteLine(mPOS ? "1" : "0");
            sw.WriteLine(mIgnoreSigLen ? "1" : "0");
            sw.WriteLine(mScryptBlockHash ? "1" : "0");
            sw.Close();   

            // Save block headers
            if (!Directory.Exists(path + "headers/"))
                Directory.CreateDirectory(path + "headers/");
            if (HasStaticData)
            {
                filename = path + "headers/" + mName + ".headers";
                f = File.Open(filename, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(f);
                mBC.Save(bw);
                bw.Close();
            }
        }

        public void StartCalcHashrate()
        {
            if (!HasStaticData)
            {
                mStatus = "No Static Data";
            }
            else
            {
                if( mBC.IsConnected())
                {
                    mStatus = "Connected";
                }
                else
                    mStatus = "No Connection";
            }
        }

        public bool FinishCalcHashrate()
        {
            if (mBC != null && mBC.IsConnected())
            {
                Block[] blocks = mBC.GetBlocks();
                if (mPOS)
                {
                    // Filter pos blocks out
                    List<Block> cleanBlocks = new List<Block>();
                    foreach (Block b in blocks)
                    {
                        if (b.mTransactions.Count > 0)
                        {
                            if (b.mTransactions.Count == 1 || b.mTransactions[0].mOutputs[0].mValue != 0)
                                cleanBlocks.Add(b);
                        }
                    }
                    blocks = cleanBlocks.ToArray();
                }

                if (blocks.Length > 0)
                {
                    UInt64 timeSeconds = Program.UnixTime() - (UInt64)blocks[0].mHeader.mTimestamp;
                    uint expectedBlocks = (uint)(timeSeconds / mBlockTarget);
                    int blockCount = blocks.Length;

                    UInt64 max = 4294901760;
                    uint diffZeros = ((blocks[0].mHeader.mDifficultyBits >> 24) & 0xFF);
                    int shift = 40 - ((int)(32 - diffZeros) * 8);
                    UInt64 diffBits = (UInt64)(blocks[0].mHeader.mDifficultyBits & 0x00FFFFFF);
                    if (shift < 0)
                        max <<= (0 - shift);
                    else
                        diffBits <<= shift;
                    double difficulty = (double)max / (double)diffBits;

                    mHashrate = (((double)blockCount / (double)expectedBlocks) * difficulty * 4294967296) / (double)mBlockTarget;
                    return true;
                }
            }
            return false;
        }

        public void Update(double btcPrice, double hashrate)
        {       
            mUSDPerCoin = mBTCPerCoin * btcPrice;

            double percentOfNetworkHR = hashrate / mHashrate;
            double winBlocksPerDay = percentOfNetworkHR * mBlocksPerDay;            
            double coinsPerDay = winBlocksPerDay * mCoinsPerBlock;
            mUSDPerDay = mUSDPerCoin * coinsPerDay;
        }

        /*
        void HRThreadFunc()
        {
            Bitcoin bc = new Bitcoin();
            bc.AddNode(mIPAddress, mPort, mNetVersion, mNetID);
            while (!bc.IsConnected())
            {
                Thread.Sleep(100);
            }


            bc.RequestHeaders(mLastHeaderHash);
            bc.WaitForHeaders();

            BlockHeader[] blocks = bc.GetRecentBlockHeaders(c.mIgnoreDifficultyDifference);
            c.mLastHeaderHash = blocks[0].mPrevBlock;
            SaveCurrency(c);

            UInt64 timeSeconds = Program.UnixTime() - (UInt64)blocks[0].mTimestamp;
            uint expectedBlocks = (uint)(timeSeconds / c.mBlockTarget);

            UInt64 max = 4294901760;
            uint diffZeros = ((blocks[0].mDifficultyBits >> 24) & 0xFF);
            int shift = 40 - ((int)(32 - diffZeros) * 8);
            UInt64 diffBits = (UInt64)(blocks[0].mDifficultyBits & 0x00FFFFFF);
            if (shift < 0)
                max <<= (0 - shift);
            else
                diffBits <<= shift;
            double difficulty = (double)max / (double)diffBits;
            difficultyBox.Text = difficulty.ToString();

            double hashRate = (((double)blocks.Length / (double)expectedBlocks) * difficulty * 4294967296) / c.mBlockTarget;
            hashrateBox.Text = string.Format("{0:N0}", hashRate);

            blocksPerDayBox.Text = blocks.Length.ToString();
        }
         * */
    }
}

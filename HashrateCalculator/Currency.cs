using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HashrateCalculator
{
    public class Currency
    {
        Bitcoin mBC = null;
        Thread mThread;

        bool mDirty = false;
        DateTime mLastCryptsyTime;
        DateTime mLastBCTime;

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
        public string mCryptsyMarket;

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

            mThread = new Thread(new ThreadStart(ThreadUpdateFunction));
            mThread.Start();
        }

        public void Destroy()
        {
            if( mBC != null )
                mBC.Destroy();
            mThread.Abort();
        }

        public bool IsDirty { get { return mDirty; } }

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
                mCryptsyMarket = sr.ReadLine();
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
            sw.WriteLine(mCryptsyMarket);
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

        public void UpdateBitcoinProtocol()
        {
            if (mBC != null && mBC.IsConnected())
            {
                TimeSpan span = DateTime.Now - mLastBCTime;
                if (span.TotalSeconds > 60)
                {
                    Block[] blocks = mBC.GetBlocks();
                    if (blocks != null)
                    {
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

                            double hashrate = (((double)blockCount / (double)expectedBlocks) * difficulty * 4294967296) / (double)mBlockTarget;
                            if (mHashrate != hashrate)
                            {
                                mHashrate = hashrate;
                                mDirty = true;
                            }
                            mLastBCTime = DateTime.Now;
                        }
                    }
                }
            }
        }

        public void Update(double btcPrice, double hashrate)
        {       
            mUSDPerCoin = mBTCPerCoin * btcPrice;

            double percentOfNetworkHR = hashrate / mHashrate;
            double winBlocksPerDay = percentOfNetworkHR * mBlocksPerDay;            
            double coinsPerDay = winBlocksPerDay * mCoinsPerBlock;
            mUSDPerDay = mUSDPerCoin * coinsPerDay;

            mDirty = false;
        }

        void UpdatePriceData()
        {
            TimeSpan span = DateTime.Now - mLastCryptsyTime;
            if (span.TotalSeconds > 60)
            {
                string url = "http://pubapi.cryptsy.com/api.php?method=singlemarketdata&marketid=" + mCryptsyMarket;
                string priceData = Program.ReadWebString(url);
                if (priceData != null && priceData.Length > 0)
                {
                    int idx = priceData.IndexOf("\"markets\":{");
                    if (idx >= 0)
                    {
                        priceData = priceData.Substring(idx + 11);
                        idx = priceData.IndexOf(':');
                        if (idx >= 0)
                        {
                            priceData = priceData.Substring(idx + 1);
                            priceData = priceData.Substring(0, priceData.Length - 3);
                            Cryptsy.Market mkt = JsonConvert.DeserializeObject<Cryptsy.Market>(priceData);
                            if (mkt != null)
                            {
                                double BTCPerCoin = Convert.ToDouble(mkt.lasttradeprice);
                                if (mBTCPerCoin != BTCPerCoin)
                                {
                                    mBTCPerCoin = BTCPerCoin;
                                    mDirty = true;
                                }
                            }
                        }
                    }
                }

                mLastCryptsyTime = DateTime.Now;
            }
        }

        void ThreadUpdateFunction()
        {
            while (true)
            {
                // Update network conneciton for hashrate
                UpdateBitcoinProtocol();

                // Update cryptsy data for btc value
                UpdatePriceData();

                Thread.Sleep(50);
            }
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

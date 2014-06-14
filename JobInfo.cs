using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCNet;

namespace CentralMine.NET
{
    class JobInfo : WorkBlock
    {
        public string mJobID;

        public string mPrevHash;
        public string mCoinBaseA;
        public string mCoinBaseB;
        public List<String> mMerkleBranch;
        public string mVersion;
        public string mDifficulty;
        public string mTime;

        string mExtraNonce;
        int mExtraNonceSize;
        ulong mExtraNonceB = 2;
        public string mExtraNonce2;

        public string mTimeStr;

        public JobInfo(string extraNonce, int extraNonceSize)
            : base()
        {
            mMerkleBranch = new List<string>();
            mExtraNonce = extraNonce;
            mExtraNonceSize = extraNonceSize;
        }

        public void Initialize(ulong serverDiff)
        {
            if (serverDiff == 0)
            {
                uint diff = Utils.HexStringToUInt(mDifficulty);
                int diffBytes = ((int)diff >> 24) & 0x000000FF;

                strTarget = "";
                for (int i = 0; i < diffBytes - 3; i++)
                    strTarget += "00";
                strTarget += string.Format("{0:X2}", (byte)diff);
                strTarget += string.Format("{0:X2}", (byte)((int)diff >> 8));
                strTarget += string.Format("{0:X2}", (byte)((int)diff >> 16));
                for (int i = diffBytes; i < 32; i++)
                    strTarget += "00";
            }
            else
            {
                strTarget = "000000000000000000000000000000000000000000000000";
                strTarget += string.Format("{0:X2}", (byte)((long)serverDiff));
                strTarget += string.Format("{0:X2}", (byte)((long)serverDiff >> 8));
                strTarget += string.Format("{0:X2}", (byte)((long)serverDiff >> 16));
                strTarget += string.Format("{0:X2}", (byte)((long)serverDiff >> 24));
                strTarget += string.Format("{0:X2}", (byte)((long)serverDiff >> 32));
                strTarget += string.Format("{0:X2}", (byte)((long)serverDiff >> 40));
                strTarget += string.Format("{0:X2}", (byte)((long)serverDiff >> 48));
                strTarget += string.Format("{0:X2}", (byte)((long)serverDiff >> 56));
            }
                      
            target = Utils.HexStringToByteArray(strTarget);
            //Utils.ByteSwapIntegers(target);

            mHashMan = new HashManager();
        }

        public void GenerateWork()
        {
            byte[] en2b = new byte[mExtraNonceSize];
            for( int i = 0; i < mExtraNonceSize; i++ )
            {
                ulong mask = (ulong)0xFF << (i * 8);
                ulong masked = (mExtraNonceB & mask) >> (i * 8);
                en2b[mExtraNonceSize - (1 + i)] = (byte)masked;
            }
            mExtraNonceB++;

            mExtraNonce2 = Utils.ByteArrayToHexString(en2b);
            string coinbase = mCoinBaseA + mExtraNonce + mExtraNonce2 + mCoinBaseB;

            byte[] cb = Utils.HexStringToByteArray(coinbase);
            byte[] cbHash = MinerLib_cs.Scrypt.SHA2562(cb);
            string merkleRoot = Utils.ByteArrayToHexString(cbHash);

            foreach (string merkle in mMerkleBranch)
            {
                byte[] hashData = Utils.HexStringToByteArray(merkleRoot + merkle);
                byte[] merkleHash = MinerLib_cs.Scrypt.SHA2562(hashData);
                merkleRoot = Utils.ByteArrayToHexString(merkleHash);
            }

            // byteswap the merkle root section
            byte[] merkleData = Utils.HexStringToByteArray(merkleRoot);
            Utils.ByteSwapIntegers(merkleData);
            merkleRoot = Utils.ByteArrayToHexString(merkleData);


            uint time = (uint)Utils.UnixTime() + 120;
            mTimeStr = Utils.UIntToHexString(time);

            strData = mVersion + mPrevHash + merkleRoot + mTimeStr + mDifficulty + "00000000" + "000000800000000000000000000000000000000000000000000000000000000000000000000000000000000080020000";            
            data = Utils.HexStringToByteArray(strData);
        }
    }
}

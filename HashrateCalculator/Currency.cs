using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashrateCalculator
{
    class Currency
    {
        public string mName;
        public string mIPAddress;
        public ushort mPort;
        public uint mNetVersion;
        public uint mNetID;
        public uint mBlockTarget;
        public byte[] mLastHeaderHash;
        public bool mIgnoreDifficultyDifference;


        public Currency()
        {
            mLastHeaderHash = new byte[32];
        }

        public override string ToString()
        {
            return mName;
        }
    }
}

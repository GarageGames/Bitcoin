using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentralMine.NET
{
    class MiningTarget
    {
        public string mName;
        public HashAlgorithm mPOWAlgorithm;

        public WalletInfo mWallet;
        public List<PoolInfo> mPools;

        public MiningTarget()
        {
            mPools = new List<PoolInfo>();
        }
    }
}

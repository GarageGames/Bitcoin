using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashrateCalculator
{
    public class Block
    {
        public string mHash;

        public BlockHeader mHeader;
        public List<Transaction> mTransactions;

        public Block()
        {
            mTransactions = new List<Transaction>();
        }

    }
}

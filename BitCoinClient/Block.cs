using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitCoinClient
{
    public class Block : NetworkDataObject
    {
        List<Transaction> mTransactions;

        public Block(NetworkDataObject.DataStatus status = NetworkDataObject.DataStatus.NoData) : base(status)
        {
        }

        public Block(byte[] hash, NetworkDataObject.DataStatus status = NetworkDataObject.DataStatus.NoData)
            : base(hash, status)
        {
        }

        public void AddTransaction(Transaction tx)
        {
            mTransactions.Add(tx);
        }
    }
}

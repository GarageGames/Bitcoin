using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BCNet
{
    class Block
    {
        public int mHeight;
        public BlockHeader mHeader;
        public List<Transaction> mTransactions;

        public DateTime mLastRequestTime;

        public Block()
        {
            mTransactions = new List<Transaction>();
        }

        public void Load(BinaryReader br)
        {
            mHeader = new BlockHeader();
            mHeader.Load(br);

            int transactionCount = br.ReadInt32();
            for (int i = 0; i < transactionCount; i++)
            {
                Transaction t = new Transaction();
                t.Load(br);
                mTransactions.Add(t);
            }
        }

        public void Save(BinaryWriter bw)
        {
            mHeader.Save(bw);

            bw.Write(mTransactions.Count);
            foreach (Transaction t in mTransactions)
            {
                t.Save(bw);
            }
        }

    }
}

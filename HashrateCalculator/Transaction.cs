using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashrateCalculator
{
    public class TransactionInput
    {
        public byte[] mPrevOuptutHash;
        public uint mPrevOutputIndex;

        public byte[] mScript;
        public uint mSequence;
    }

    public class TransactionOutput
    {
        public ulong mValue;
        public double mRealValue;
        public byte[] mScript;
    }

    public class Transaction
    {
        public uint mVersion;
        public uint mTimestamp;
        public List<TransactionInput> mInputs;
        public List<TransactionOutput> mOutputs;
        public uint mLockTime;
        public byte[] mComment;

        public Transaction()
        {
            mInputs = new List<TransactionInput>();
            mOutputs = new List<TransactionOutput>();
        }
    }
}

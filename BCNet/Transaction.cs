using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BCNet
{
    class TransactionInput
    {
        public byte[] mPrevOuptutHash;
        public uint mPrevOutputIndex;

        public byte[] mScript;
        public uint mSequence;
    }

    class TransactionOutput
    {
        public ulong mValue;
        public double mRealValue;
        public byte[] mScript;
    }

    class Transaction
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

        public void Load(BinaryReader br)
        {
            mVersion = br.ReadUInt32();
            mTimestamp = br.ReadUInt32();

            int inputCount = br.ReadInt32();
            for (int i = 0; i < inputCount; i++)
            {
                TransactionInput ti = new TransactionInput();
                ti.mPrevOuptutHash = br.ReadBytes(32);
                ti.mPrevOutputIndex = br.ReadUInt32();

                int scriptLen = br.ReadInt32();
                ti.mScript = br.ReadBytes(scriptLen);
                ti.mSequence = br.ReadUInt32();
                mInputs.Add(ti);
            }

            int outputCount = br.ReadInt32();
            for (int i = 0; i < outputCount; i++)
            {
                TransactionOutput to = new TransactionOutput();
                to.mValue = br.ReadUInt64();
                to.mRealValue = (double)to.mValue / 1000000.0;

                int scriptLen = br.ReadInt32();
                to.mScript = br.ReadBytes(scriptLen);
            }

            mLockTime = br.ReadUInt32();
            int commentLen = br.ReadInt32();
            mComment = br.ReadBytes(commentLen);
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(mVersion);
            bw.Write(mTimestamp);

            bw.Write(mInputs.Count);
            foreach (TransactionInput ti in mInputs)
            {
                bw.Write(ti.mPrevOuptutHash);
                bw.Write(ti.mPrevOutputIndex);

                bw.Write(ti.mScript.Length);
                bw.Write(ti.mScript);
                bw.Write(ti.mSequence);
            }

            bw.Write(mOutputs.Count);
            foreach (TransactionOutput to in mOutputs)
            {
                bw.Write(to.mValue);
                bw.Write(to.mScript.Length);
                bw.Write(to.mScript);
            }

            bw.Write(mLockTime);
            if (mComment != null)
            {
                bw.Write(mComment.Length);
                bw.Write(mComment);
            }
            else
            {
                bw.Write((int)0);
            }
        }
    }
}

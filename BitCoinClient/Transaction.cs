using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BitCoinClient
{
    public class Transaction : NetworkDataObject
    {
        uint mVersion;
        List<TransactionInput> mInputs;
        List<TransactionOutput> mOutputs;
        uint mLockTime;

        public Transaction(NetworkDataObject.DataStatus status = NetworkDataObject.DataStatus.NoData)
            : base(status)
        {
        }

        public Transaction(byte[] hash, NetworkDataObject.DataStatus status = NetworkDataObject.DataStatus.NoData) : base(hash, status)
        {
            
        }

        public void Load(byte[] data)
        {
            mInputs = new List<TransactionInput>();
            mOutputs = new List<TransactionOutput>();

            MemoryStream stream = new MemoryStream(data);
            BinaryReader br = new BinaryReader(stream);

            mVersion = br.ReadUInt32();
            ulong inputs = Program.ReadVarInt(br);
            for (ulong i = 0; i < inputs; i++)
            {
                TransactionInput input = new TransactionInput(br);
                mInputs.Add(input);
            }

            ulong outputs = Program.ReadVarInt(br);
            for (ulong i = 0; i < outputs; i++)
            {
                TransactionOutput output = new TransactionOutput(br);
                mOutputs.Add(output);
            }

            mLockTime = br.ReadUInt32();

            br.Close();
            Status = DataStatus.Loaded;
        }

        public void AddInput(TransactionInput input)
        {
            mInputs.Add(input);
        }

        public void AddOutput(TransactionOutput output)
        {
            mOutputs.Add(output);
        }

        public bool IsCoinBase()
        {
            return (mInputs.Count == 1 && mInputs[0].IsPeviousNull());
        }

        public bool IsFinal()
        {
            if (mLockTime == 0)
                return true;

            foreach (TransactionInput txin in mInputs)
            {
                if (!txin.IsFinal())
                    return false;
            }

            return true;
        }


        public TransactionInput[] Inputs
        {
            get { return mInputs.ToArray(); }
            set { }
        }
    }
}

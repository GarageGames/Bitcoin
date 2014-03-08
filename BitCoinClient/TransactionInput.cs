using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BitCoinClient
{
    public class TransactionInput
    {
        // Outpoint
        byte[] mOPHash;
        uint mOPIndex;

        byte[] mSignatureScript;
        uint mSequence;

        public TransactionInput()
        {
            mOPIndex = 0xFFFFFFFF;
            mSequence = uint.MaxValue;
        }

        public TransactionInput(BinaryReader br)
        {
            mOPHash = br.ReadBytes(32);
            mOPIndex = br.ReadUInt32();

            ulong scriptLen = Program.ReadVarInt(br);
            if (scriptLen > int.MaxValue)
            {
                Console.WriteLine("Really big transaction data! need to handle this");
            }
            else
            {
                mSignatureScript = br.ReadBytes((int)scriptLen);
            }

            mSequence = br.ReadUInt32();
        }

        public bool IsPeviousNull()
        {
            return (mOPIndex == 0xFFFFFFFF);
        }

        public bool IsFinal()
        {
            return (mSequence == uint.MaxValue);
        }
    }
}

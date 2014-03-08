using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenSSL.Crypto.EC;

namespace BitCoinClient
{
    public class TransactionOutput
    {
        ulong mValue;
        byte[] mScript;

        public TransactionOutput(BinaryReader br)
        {
            mValue = br.ReadUInt64();

            ulong scriptLen = Program.ReadVarInt(br);
            if (scriptLen <= int.MaxValue)
            {
                mScript = br.ReadBytes((int)scriptLen);
            }
            else
            {
                Console.WriteLine("Really big output script, need to handle this!");
            }
        }

        public TransactionOutput(Key key)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);

            byte[] pubKey = key.GetPublicBytes();
            bw.Write((byte)pubKey.Length);
            bw.Write(pubKey);
            bw.Write((byte)0xac);

            bw.Close();
        }

        public ulong Value
        {
            get { return mValue; }
            set { mValue = value; }
        }
    }
}

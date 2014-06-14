using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BCNet
{
    public class BlockHeader
    {
        public byte[] mHash;
        public uint mVersion;
        public byte[] mPrevBlock;
        public byte[] mMerkleRoot;
        public uint mTimestamp;
        public uint mDifficultyBits;
        public uint mNOnce;

        public BlockHeader()
        {
            mHash = new byte[32];
            mPrevBlock = new byte[32];
            mMerkleRoot = new byte[32];
        }

        public BlockHeader(uint version, byte[] prevBlock, byte[] merkleRoot, uint timestamp, uint bits, uint nonce)
        {
            mVersion = version;
            mPrevBlock = prevBlock;
            mMerkleRoot = merkleRoot;
            mTimestamp = timestamp;
            mDifficultyBits = bits;
            mNOnce = nonce;
        }

        public void Load(BinaryReader br)
        {
            mHash = br.ReadBytes(32);
            mVersion = br.ReadUInt32();
            mPrevBlock = br.ReadBytes(32);
            mMerkleRoot = br.ReadBytes(32);
            mTimestamp = br.ReadUInt32();
            mDifficultyBits = br.ReadUInt32();
            mNOnce = br.ReadUInt32();
        }

        public void Save(BinaryWriter bw)
        {
            if (mHash != null)
                bw.Write(mHash);
            else
                bw.Write(new byte[32]);
            bw.Write(mVersion);
            bw.Write(mPrevBlock);
            bw.Write(mMerkleRoot);
            bw.Write(mTimestamp);
            bw.Write(mDifficultyBits);
            bw.Write(mNOnce);
        }

        public bool ValidateHash()
        {
            byte[] oldHash = mHash;
            ComputeHash(false);
            byte[] sha256 = mHash;
            ComputeHash(true);
            byte[] scrypt = mHash;
            mHash = oldHash;

            if (Utils.HashCompare(oldHash, sha256))
                return false;
            if (!Utils.HashCompare(oldHash, scrypt))
                Console.WriteLine("Neither sha256 or scrypt generated a matching hash");

            return true;
        }

        public void ComputeHash(bool useScrypt)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(mVersion);
            bw.Write(mPrevBlock);
            bw.Write(mMerkleRoot);
            bw.Write(mTimestamp);
            bw.Write(mDifficultyBits);
            bw.Write(mNOnce);
            byte[] data = ms.ToArray();
            bw.Close();

            if (useScrypt)
            {
                MinerLib_cs.Work work = new MinerLib_cs.Work();
                Buffer.BlockCopy(data, 0, work.data, 0, 80);
                for (int i = 0; i < 20; i++)
                    work.data[i] = Utils.Byteswap(work.data[i]);

                MinerLib_cs.Scrypt scrypt = new MinerLib_cs.Scrypt(work);
                mHash = scrypt.GetHash();
            }
            else
            {
                // SHA 256
                mHash = Utils.GenerateHash(data);
            }
        }
    }
}

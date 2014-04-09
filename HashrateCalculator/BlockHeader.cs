using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashrateCalculator
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

        public BlockHeader(string hash)
        {
            mHash = Program.HexStringToByteArray(hash);
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
    }
}

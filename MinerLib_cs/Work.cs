using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinerLib_cs
{
    public class Work
    {
        public enum Algorithm
        {
            SHA256,
            Scrypt
        };

        public uint hashStart;
        public uint hashCount;
        public Algorithm hashAlgorithm;
        public uint[] target;
        public uint[] data;

        public Work()
        {
            target = new uint[8];
            data = new uint[32];
        }

        public void Copy(Work work)
        {
            hashStart = work.hashStart;
            hashCount = work.hashCount;
            hashAlgorithm = work.hashAlgorithm;
            Array.Copy(work.target, target, 8);
            Array.Copy(work.data, data, 32);
        }
    }
}

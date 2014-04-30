using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinerLib_cs
{
    public class UnitTests
    {
        static uint[] sData = { 0x02000000, 0x18cf342a, 0xa354e9f3, 0xfb4bd876, 0xf9b46596, 0xc07f2e60, 0x4b930aa9, 0x1a1ba39a, 0x639d7bbf, 0x847fe2ad, 0xe767d0f9, 0xe7d22804, 0x5f40582d, 0x6f6242db, 0xd2e066e1, 0xe4aaef45, 0xd9db9063, 0xED272A53, 0x0d1b011c, 0x0005B26B, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000280 };
        static uint[] sTarget = { 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x000d1b01, 0x00000000 };
        const uint sNOnce = 373355;

        public static bool TestAll()
        {
            if (!TestStandard())
                return false;
            if (!TestSIMD())
                return false;
            if (!TestOpenCL())
                return false;
            return true;
        }

        public static bool TestStandard()
        {
            Work work = new Work();
            Array.Copy(sData, work.data, 32);
            Array.Copy(sTarget, work.target, 8);

            Scrypt scrypt = new Scrypt(work);
            bool result = scrypt.Hash(sNOnce);

            return result;
        }

        public static bool TestSIMD()
        {
            return false;
        }

        public static bool TestOpenCL()
        {
            return false;
        }
    }
}

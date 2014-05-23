using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentralMine.NET
{
    static class Utils
    {
        public static string UIntToHexString(uint val)
        {
            byte[] vbytes = new byte[4];
            vbytes[0] = (byte)(val >> 24);
            vbytes[1] = (byte)(val >> 16);
            vbytes[2] = (byte)(val >> 8);
            vbytes[3] = (byte)val;

            return ByteArrayToHexString(vbytes);
        }

        public static uint HexStringToUInt(string hex)
        {
            byte[] h = HexStringToByteArray(hex);
            uint val = (uint)((h[0] << 24) | (h[1] << 16) | (h[2] << 8) | h[3]);
            return val;
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public static string ByteArrayToHexString(byte[] barr)
        {
            string str = "";
            foreach (byte b in barr)
            {
                str += string.Format("{0:X2}", b);
            }
            return str;
        }

        public static void ByteSwapIntegers(byte[] buffer)
        {
            uint[] ints = new uint[16];
            Buffer.BlockCopy(buffer, 0, ints, 0, 32);
            for (int i = 0; i < ints.Length; i++)
                ints[i] = MinerLib_cs.Scrypt.ByteReverse(ints[i]);
            Buffer.BlockCopy(ints, 0, buffer, 0, 32);
        }

        public static UInt64 UnixTime()
        {
            TimeSpan span = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            double unixTime = span.TotalSeconds;
            return (UInt64)unixTime;
        }
    }
}

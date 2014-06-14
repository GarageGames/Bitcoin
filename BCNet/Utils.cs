using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Numerics;

namespace BCNet
{
    public static class Utils
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
                ints[i] = Byteswap(ints[i]);
            Buffer.BlockCopy(ints, 0, buffer, 0, 32);
        }

        public static UInt64 UnixTime()
        {
            TimeSpan span = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            double unixTime = span.TotalSeconds;
            return (UInt64)unixTime;
        }

        public static short Byteswap(short value)
        {
            return (short)Byteswap((ushort)value);
        }

        public static ushort Byteswap(ushort value)
        {
            value = (ushort)(((value & 0xFF00) >> 8) | ((value & 0x00FF) << 8));
            return value;
        }

        public static uint Byteswap(uint value)
        {
            value = ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8);
            return (value << 16) | (value >> 16);
        }

        public static byte[] GenerateHash(byte[] data)
        {
            SHA256 sha = SHA256Managed.Create();
            byte[] hash1 = sha.ComputeHash(data);
            byte[] hash2 = sha.ComputeHash(hash1);

            return hash2;
        }

        public static ulong ReadVarInt(BinaryReader br)
        {
            ulong ret = 0;

            byte size = br.ReadByte();
            if (size < 0xFD)
                ret = (ulong)size;
            else
            {
                switch (size)
                {
                    case 0xFD:
                        ret = (ulong)br.ReadUInt16();
                        break;
                    case 0xFE:
                        ret = (ulong)br.ReadUInt32();
                        break;
                    case 0xFF:
                        ret = br.ReadUInt64();
                        break;
                }
            }

            return ret;
        }

        public static void WriteVarInt(BinaryWriter bw, ulong val)
        {
            if (val < 0xFD)
                bw.Write((byte)val);
            else if (val <= 0xFFFF)
            {
                bw.Write((byte)0xFD);
                bw.Write((ushort)val);
            }
            else if (val <= 0xFFFFFFFF)
            {
                bw.Write((byte)0xFE);
                bw.Write((uint)val);
            }
            else
            {
                bw.Write((byte)0xFF);
                bw.Write(val);
            }
        }

        public static string ReadString(BinaryReader br, int len)
        {
            byte[] bytes = br.ReadBytes(len);

            string str = "";
            for (int i = 0; i < len; i++)
            {
                if (bytes[i] != 0)
                    str += (char)bytes[i];
            }
            return str;
        }

        public static bool HashCompare(byte[] hashA, byte[] hashB)
        {
            if (hashA.Length != 32)
                return false;
            if (hashB.Length != 32)
                return false;

            for (int i = 0; i < 32; i++)
            {
                if (hashA[i] != hashB[i])
                    return false;
            }

            return true;
        }


        public static byte[] DecodeBase58String(string encoded)
        {
            const string staticBase58 = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

            BigInteger bi = 0;
            foreach (char c in encoded)
            {
                int idx = staticBase58.IndexOf(c);
                if (idx < 0)
                {
                    if (c != '\0')
                        return null;
                    break;
                }
                bi *= 58;
                bi += idx;
            }
            byte[] data = bi.ToByteArray();
            byte[] swapped = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                swapped[i] = data[data.Length - (i + 1)];
            return swapped;
        }

        public static byte[] DecodeBase58StringChecked(string encoded)
        {
            byte[] test = DecodeBase58String(encoded);
            if (test == null || test.Length < 4)
                return null;

            byte[] hashData = new byte[test.Length - 4];
            Array.Copy(test, hashData, test.Length - 4);
            byte[] hash = BCNet.Utils.GenerateHash(hashData);

            int testIndex = test.Length - 4;
            for (int i = 0; i < 4; i++)
            {
                if (test[testIndex + i] != hash[i])
                    return null;
            }

            return hashData;
        }
    }
}

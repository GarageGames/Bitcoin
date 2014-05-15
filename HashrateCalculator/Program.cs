using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace HashrateCalculator
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static UInt64 UnixTime()
        {
            TimeSpan span = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            double unixTime = span.TotalSeconds;
            return (UInt64)unixTime;
        }

        public static short Byteswap(short inval)
        {
            return IPAddress.HostToNetworkOrder(inval);
        }

        public static uint Byteswap(uint inval)
        {
            return (uint)IPAddress.HostToNetworkOrder((int)inval);
        }

        public static UInt64 Byteswap(UInt64 inval)
        {
            UInt64 outval = ((inval & (UInt64)0xFF00000000000000) >> 56) |
                            ((inval & (UInt64)0x00FF000000000000) >> 40) |
                            ((inval & (UInt64)0x0000FF0000000000) >> 24) |
                            ((inval & (UInt64)0x000000FF00000000) >> 8) |
                            ((inval & (UInt64)0x00000000FF000000) << 8) |
                            ((inval & (UInt64)0x0000000000FF0000) << 24) |
                            ((inval & (UInt64)0x000000000000FF00) << 40) |
                            ((inval & (UInt64)0x00000000000000FF) << 56);

            return outval;
        }

        public static byte[] Byteswap(byte[] input)
        {
            byte[] output = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[(input.Length - 1) - i] = input[i];
            }
            return output;
        }

        public static string ReadString(BinaryReader br, int len)
        {
            byte[] bytes = br.ReadBytes(len);
            //char[] chars = br.ReadChars(len);

            string str = "";
            for (int i = 0; i < len; i++)
            {
                if (bytes[i] != 0)
                    str += (char)bytes[i];
            }
            return str;
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

        public static string HashToString(byte[] hash)
        {
            string str = "";
            foreach (byte b in hash)
            {
                str += string.Format("{0:X2}", b);
            }
            return str;
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

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public static byte[] GenerateHash(byte[] data)
        {
            SHA256 sha = SHA256Managed.Create();
            byte[] hash1 = sha.ComputeHash(data);
            byte[] hash2 = sha.ComputeHash(hash1);

            return hash2;
        }

        public static string ReadWebString(string url)
        {
            try
            {
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Stream s = response.GetResponseStream();
                StreamReader rs = new StreamReader(s, Encoding.GetEncoding("utf-8"));

                string retVal = rs.ReadToEnd();
                rs.Close();

                int bodyIndex = retVal.IndexOf("<body>", StringComparison.CurrentCultureIgnoreCase);
                if (bodyIndex >= 0)
                {
                    retVal = retVal.Substring(bodyIndex + 6);
                    retVal = retVal.Substring(0, retVal.IndexOf("</body>", StringComparison.CurrentCultureIgnoreCase));
                    retVal = retVal.Trim();
                }

                return retVal;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
    }
}

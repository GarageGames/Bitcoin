using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CentralMine.NET
{
    public class Block
    {
        public byte[] midstate;
        public byte[] data;
        public byte[] data64;
        public byte[] hash1;
        public byte[] target;

        public string strMidstate;
        public string strData;
        public string strHash1;
        public string strTarget;

        public HashManager mHashMan;

        public Block(JObject obj)
        {            
            strMidstate = obj["midstate"].ToString();
            strData = obj["data"].ToString();
            strHash1 = obj["hash1"].ToString();
            strTarget = obj["target"].ToString();

            //strMidstate = "339a90bcf0bf58637daccc90a8ca591ee9d8c8c3c803014f3687b1961bf91947";
            //strData = "000000010000000000000000000000000000000000000000000000000000000000000000fdeda33bb2127b7a3e2cc77a618f7667c31bc87f32518a88aab89f3a4a5e1e4b495fab291d00ffff7c2bac1d000000800000000000000000000000000000000000000000000000000000000000000000000000000000000080020000";
            //strTarget = "0000000000000000000000000000000000000000000000000000ffff00000000";
            // 2,083,236,893

            midstate = HexStringToByteArray(strMidstate);
            data = HexStringToByteArray(strData);
            hash1 = HexStringToByteArray(strHash1);
            target = HexStringToByteArray(strTarget);
                        

            data64 = new byte[64];
            Buffer.BlockCopy(data, 64, data64, 0, 64);

            mHashMan = new HashManager();

            //IntSwapArray(midstate);
            //IntSwapArray(data64);
            //IntSwapArray(target);
        }

        public string GetSolutionString(uint solution)
        {
            data[79] = (byte)(solution & 0xFF);
            data[78] = (byte)((solution & 0xFF00) >> 8);
            data[77] = (byte)((solution & 0xFF0000) >> 16);
            data[76] = (byte)((solution & 0xFF000000) >> 24);

            return ArrayToHexString(data);
        }

        public override string ToString()
        {
            string str = "{\n\tmidstate: " + strMidstate + "\n";
            str += "\tdata: " + strData + "\n";
            str += "\ttarget: " + strTarget + "\n}\n";
            return str;
        }

        void IntSwapArray(byte[] array)
        {
            for (int i = 0; i < array.Length; i += 4)
            {
                byte temp = array[i];
                array[i + 0] = array[i + 3];
                array[i + 3] = temp;

                temp = array[i + 1];
                array[i + 1] = array[i + 2];
                array[i + 2] = temp;
            }
        }

        public static string ArrayToHexString(byte[] hash)
        {
            string str = "";
            foreach (byte b in hash)
            {
                str += string.Format("{0:X2}", b);
            }
            return str;
        }

        byte[] HexStringToByteArray(string hex)
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

        int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}

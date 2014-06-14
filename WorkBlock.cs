using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BCNet;

namespace CentralMine.NET
{
    class WorkBlock
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
        public HashAlgorithm mAlgorithm;

        public WorkBlock()
        {
        }

        public WorkBlock(JObject obj)
        {            
            strMidstate = obj["midstate"].ToString();
            strData = obj["data"].ToString();
            strHash1 = obj["hash1"].ToString();
            strTarget = obj["target"].ToString();

            //strMidstate = "339a90bcf0bf58637daccc90a8ca591ee9d8c8c3c803014f3687b1961bf91947";
            //strData = "000000010000000000000000000000000000000000000000000000000000000000000000fdeda33bb2127b7a3e2cc77a618f7667c31bc87f32518a88aab89f3a4a5e1e4b495fab291d00ffff7c2bac1d000000800000000000000000000000000000000000000000000000000000000000000000000000000000000080020000";
            //strTarget = "0000000000000000000000000000000000000000000000000000ffff00000000";
            // 2,083,236,893
                      
            //strData = "000000022a34cf18f3e954a376d84bfb9665b4f9602e7fc0a90a934b9aa31b1abf7b9d63ade27f84f9d067e70428d2e72d58405fdb42626fe166e0d245efaae46390dbd9532a27ed1c011b0d6bb20500000000800000000000000000000000000000000000000000000000000000000000000000000000000000000080020000";
            //strTarget = "000000000000000000000000000000000000000000000000000d1b0100000000";

            midstate = Utils.HexStringToByteArray(strMidstate);
            data = Utils.HexStringToByteArray(strData);
            hash1 = Utils.HexStringToByteArray(strHash1);
            target = Utils.HexStringToByteArray(strTarget);
                        

            data64 = new byte[64];
            Buffer.BlockCopy(data, 64, data64, 0, 64);
            //Utils.ByteSwapIntegers(target);

            mHashMan = new HashManager();
        }

        public string GetSolutionString(uint solution)
        {
            data[76] = (byte)(solution & 0xFF);
            data[77] = (byte)((solution & 0xFF00) >> 8);
            data[78] = (byte)((solution & 0xFF0000) >> 16);
            data[79] = (byte)((solution & 0xFF000000) >> 24);

            return Utils.ByteArrayToHexString(data);
        }

        public override string ToString()
        {
            string str = "{\n\tmidstate: " + strMidstate + "\n";
            str += "\tdata: " + strData + "\n";
            str += "\ttarget: " + strTarget + "\n}\n";
            return str;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitCoinClient
{
    public class WorkBlock
    {
        public byte[] data;
        public byte[] hash1;
        public byte[] midstate;
        public byte[] target;

        public byte[] hash;

        public WorkBlock(string json)
        {
            hash = new byte[32];

            JObject obj = JObject.Parse(json);
            JObject result = JObject.Parse(obj["result"].ToString());

            midstate = Program.HexStringToByteArray(result["midstate"].ToString());
            data = Program.HexStringToByteArray(result["data"].ToString());
            hash1 = Program.HexStringToByteArray(result["hash1"].ToString());
            target = Program.HexStringToByteArray(result["target"].ToString());
        }

        public WorkBlock(JObject obj)
        {
            midstate = Program.HexStringToByteArray(obj["midstate"].ToString());
            data = Program.HexStringToByteArray(obj["data"].ToString());
            hash1 = Program.HexStringToByteArray(obj["hash1"].ToString());
            target = Program.HexStringToByteArray(obj["target"].ToString());
        }
    }
}

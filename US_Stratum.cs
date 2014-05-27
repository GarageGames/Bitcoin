using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.IO;

namespace CentralMine.NET
{
    class US_Stratum :  Upstream
    {
        enum Status
        {
            Disconnected,
            Connecting,
            Connected,
            Subscribing,
            Authorizing,
            Ready
        };

        Thread mThread;
        Socket mSocket;
        IAsyncResult mSocketAsync;
        Status mStatus;

        int mRPCID;
        Dictionary<int, string> mPendingRPCs;

        byte[] mPendingData;
        int mPendingDataSize;

        string mExtraNOnce;
        int mExtraNOnceSize;
        bool mAuthorized;

        string mSubmitString;
        bool mAwaitingSubmitResult = false;
        bool mSubmitResult = false;
        bool mNewBlockReady = false;
        ulong mServerDiff = 0;

        List<JobInfo> mJobs;

        FileStream mLogFile;
        StreamWriter mLog;

        public US_Stratum(ClientManager cm) : base(cm)
        {
            mPendingRPCs = new Dictionary<int, string>();
            mJobs = new List<JobInfo>();

            mPendingData = new byte[1024 * 16];
            mPendingDataSize = 0;

            mPort = 0;
            mStatus = Status.Disconnected;

            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            mThread = new Thread(new ThreadStart(ThreadUpdate));
            mThread.Start();

            mLogFile = File.Open("stratum.log", FileMode.Create);
            mLog = new StreamWriter(mLogFile);
        }

        public override void Destroy()
        {
            mThread.Abort();
            Disconnect();
            mLog.Close();
        }        

        public override void SetHost(string url, ushort port)
        {
            base.SetHost(url, port);
            Disconnect();
        }

        public override void SetCredentials(string user, string pass)
        {
            base.SetCredentials(user, pass);
            Disconnect();
        }

        public override WorkBlock GetWorkBlock()
        {
            if (mStatus != Status.Ready)
                return null;

            if (mJobs.Count <= 0)
                return null;

            JobInfo ji = mJobs[mJobs.Count - 1];
            ji.GenerateWork();
            mNewBlockReady = false;
            Console.WriteLine("GetWork job: " + ji.mJobID);
            return ji;
        }

        public override bool SubmitWork(WorkBlock work, uint solution)
        {
            JobInfo ji = (JobInfo)work;
            // {"params": ["slush.miner1", "bf", "00000001", "504e86ed", "b2957c02"], "id": 4, "method": "mining.submit"}
            // Values in particular order: worker_name (previously authorized!), job_id, extranonce2, ntime, nonce.
            string[] parms = new string[5];
            parms[0] = mUser;
            parms[1] = ji.mJobID;
            parms[2] = ji.mExtraNonce2;
            parms[3] = ji.mTimeStr;
            parms[4] = Utils.UIntToHexString(solution);

            mSubmitString = String.Format("Submit Job({0}), Time({1}), Solution({2})", parms[1], parms[3], parms[4]) + "\n" + ji.strData + "\n" + ji.strTarget;
            Console.WriteLine(mSubmitString);
            mLog.WriteLine(mSubmitString);


            mAwaitingSubmitResult = true;
            SendRPC("mining.submit", parms);
            while (mAwaitingSubmitResult)
            {
                Thread.Sleep(50);
            }
            return mSubmitResult;
        }

        public override bool NewBlockReady()
        {
            return mNewBlockReady;
        }

        void Disconnect()
        {
            if( mSocket.Connected )
                mSocket.Close();
            mStatus = Status.Disconnected;
            mPendingDataSize = 0;
            mAuthorized = false;
        }

        int SendRPC(string methodName, string[] parameters)
        {
            int rpcID = mRPCID++;
            string rpc = "{\"id\": " + rpcID + ", \"method\": \"" + methodName + "\", \"params\": [";
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    rpc += "\"" + parameters[i] + "\"";
                    if (i < (parameters.Length - 1))
                        rpc += ", ";
                }
            }
            rpc += "]}\n";

            Console.WriteLine("SendRPC: " + rpc);
            mLog.WriteLine("SendRPC: " + rpc);

            byte[] data = System.Text.Encoding.ASCII.GetBytes(rpc);
            int bytesSent = mSocket.Send(data, data.Length, SocketFlags.None);
  
            mPendingRPCs[rpcID] = methodName;
            return rpcID;
        }

        void MiningSubmit(JObject obj, string json)
        {
            bool? result = obj["result"].Value<bool?>();
            if (result == null)
            {
                mSubmitResult = false;
            }
            else
            {
                mSubmitResult = result.Value;
            }
            mAwaitingSubmitResult = false;
        }

        void MiningSubscribe(JObject obj)
        {
            // {"id": 1, "result": [[["mining.set_difficulty", "b4b6693b72a50c7116db18d6497cac52"], ["mining.notify", "ae6812eb4cd7735a302a8a9dd95cf71f"]], "08000002", 4], "error": null}
            //obj = JsonConvert.DeserializeObject<JObject>("{\"error\": null, \"id\": 1, \"result\": [[\"mining.notify\", \"ae6812eb4cd7735a302a8a9dd95cf71f\"], \"f8002c90\", 4]}");
            JArray result = (JArray)obj["result"];
            JArray parms = (JArray)result[0];
            mExtraNOnce = (string)result[1];
            mExtraNOnceSize = (int)result[2];
           
            /*
            string diff = null;
            string notify = null;            
            foreach (JToken t in parms)
            {
                JArray param = (JArray)t;
                string key = (string)param[0];
                string val = (string)param[1];

                if (key == "mining.set_difficulty")
                    diff = val;
                else if (key == "mining.notify")
                    notify = val;
                else
                {
                    Console.WriteLine("MiningSubscribe - Unknown param: " + key);
                }
            }
            */
        }

        void MiningAuthorize(JObject obj)
        {
            mAuthorized = (bool)obj["result"];
            if (!mAuthorized)
                Disconnect();
        }

        void MiningNotify(JObject obj)
        {
            //obj = JsonConvert.DeserializeObject<JObject>("{\"params\": [\"b3ba\", \"7dcf1304b04e79024066cd9481aa464e2fe17966e19edf6f33970e1fe0b60277\", \"01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff270362f401062f503253482f049b8f175308\", \"0d2f7374726174756d506f6f6c2f000000000100868591052100001976a91431482118f1d7504daf1c001cbfaf91ad580d176d88ac00000000\", [\"57351e8569cb9d036187a79fd1844fd930c1309efcd16c46af9bb9713b6ee734\", \"936ab9c33420f187acae660fcdb07ffdffa081273674f0f41e6ecc1347451d23\"], \"00000002\", \"1b44dfdb\", \"53178f9b\", true], \"id\": null, \"method\": \"mining.notify\"}");
            /*
            {
                "params": [
                            "bf", 
                            "4d16b6f85af6e2198f44ae2a6de67f78487ae5611b77c6c0440b921e00000000", 
                            "01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff20020862062f503253482f04b8864e5008", 
                            "072f736c7573682f000000000100f2052a010000001976a914d23fcdf86f7e756a64a7a9688ef9903327048ed988ac00000000", 
                            [], 
                            "00000002", 
                            "1c2ac4af", 
                            "504e86b9", 
                            false
                          ], 
                "id": null, 
                "method": "mining.notify"
            }
            job_id - ID of the job. Use this ID while submitting share generated from this job.
            prevhash - Hash of previous block.
            coinb1 - Initial part of coinbase transaction.
            coinb2 - Final part of coinbase transaction.
            merkle_branch - List of hashes, will be used for calculation of merkle root. This is not a list of all transactions, it only contains prepared hashes of steps of merkle tree algorithm. Please read some materials for understanding how merkle trees calculation works. Unfortunately this example don't have any step hashes included, my bad!
            version - Bitcoin block version.
            nbits - Encoded current network difficulty
            ntime - Current ntime/
            clean_jobs - When true, server indicates that submitting shares from previous jobs don't have a sense and such shares will be rejected. When this flag is set, miner should also drop all previous jobs, so job_ids can be eventually rotated.
            */
            JArray parms = (JArray)obj["params"];
            if (parms.Count == 9)
            {
                JobInfo ji = new JobInfo(mExtraNOnce, mExtraNOnceSize);
                ji.mJobID = (string)parms[0];
                ji.mPrevHash = (string)parms[1];
                ji.mCoinBaseA = (string)parms[2];
                ji.mCoinBaseB = (string)parms[3];
                JArray merkleBranch = (JArray)parms[4];
                foreach (JToken t in merkleBranch)
                    ji.mMerkleBranch.Add((string)t);
                ji.mVersion = (string)parms[5];
                ji.mDifficulty = (string)parms[6];
                ji.mTime = (string)parms[7];
                ji.Initialize(mServerDiff);
                bool cleanJobs = (bool)parms[8];
                if (cleanJobs)
                    mJobs.Clear();

                mJobs.Add(ji);
                mNewBlockReady = true;
            }
            else
            {
                Disconnect();
                Console.WriteLine("MiningNotify - Bad Data");
            }
        }

        void MiningSetDifficulty(JObject obj)
        {
            //{"params": [32], "id": null, "method": "mining.set_difficulty"}
            double difficulty = (double)obj["params"][0];
            //difficulty = 32;
            mLog.WriteLine("Set Difficulty: " + difficulty);

            ulong baseDiff = 0x00000000FFFF0000;
            double diff = (double)baseDiff;

            // if scrypt
            if( mClientManager.mMiningTarget.mPOWAlgorithm == HashAlgorithm.Scrypt )
                diff *= 65536;
            mServerDiff = (ulong)(diff / difficulty);
        }

        void ProcessNetworkLine(string line)
        {
            Console.WriteLine(line);
            mLog.WriteLine(line);
            JObject obj = JsonConvert.DeserializeObject<JObject>(line);

            string pendingMethod = null;
            int? nid = (obj["id"]).Value<int?>();
            if( nid == null )
            {
                pendingMethod = (string)obj["method"];
            }
            else
            {
                int id = (int)obj["id"];
                if (mPendingRPCs.ContainsKey(id))
                {
                    //JObject error = (JObject)obj["error"];
                    //if( !error.HasValues )
                    {
                        pendingMethod = mPendingRPCs[id];
                    }
                        
                    //else
                    //{
                    //    Console.WriteLine("RPC error for method: " + pendingMethod);
                    //    Disconnect();
                    //}
                    mPendingRPCs.Remove(id);
                }
                else
                {
                    Console.WriteLine("Unexpected RPC id: " + id);
                }
            }

            if( pendingMethod != null )
            {
                switch (pendingMethod)
                {
                    case "mining.subscribe":
                        MiningSubscribe(obj);
                        break;
                    case "mining.authorize":
                        MiningAuthorize(obj);
                        break;
                    case "mining.notify":
                        MiningNotify(obj);
                        break;
                    case "mining.submit":
                        MiningSubmit(obj, line);
                        break;
                    case "mining.set_difficulty":
                        MiningSetDifficulty(obj);
                        break;
                    default:
                        Console.WriteLine("Unhandled RPC response: " + pendingMethod);
                        break;
                }
            }
            else
            {
                Console.WriteLine("unknown method: " + line);
            }
        }

        void ReadNetwork()
        {
            if (mSocket.Connected)
            {
                if (mSocket.Poll(1, SelectMode.SelectRead))                
                {
                    // Data to be read
                    byte[] packetData = new byte[1024 * 32];
                    int bytesRead = mSocket.Receive(packetData);
                    if (bytesRead > 0)
                    {
                        Array.Copy(packetData, 0, mPendingData, mPendingDataSize, bytesRead);
                        mPendingDataSize += bytesRead;

                        while (mPendingDataSize > 0)
                        {
                            int i = 0;
                            for (i = 0; i < mPendingDataSize; i++)
                            {
                                if (mPendingData[i] == '\n')
                                    break;
                            }

                            if (i <= mPendingDataSize)
                            {
                                MemoryStream ms = new MemoryStream(mPendingData);
                                StreamReader sr = new StreamReader(ms);

                                string line = sr.ReadLine();
                                ProcessNetworkLine(line);

                                int offset = (int)line.Length + 1;
                                mPendingDataSize -= offset;
                                Array.Copy(mPendingData, offset, mPendingData, 0, mPendingDataSize);

                                sr.Close();
                            }
                        }
                    }
                }
            }
        }

        void ThreadUpdate()
        {
            while (true)
            {
                try
                {
                    ReadNetwork();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Disconnect();
                }

                switch (mStatus)
                {
                    case Status.Disconnected:
                        if (mURL != null && mPort != 0)
                        {
                            // We have something to try to connect to, do it now
                            mSocketAsync = mSocket.BeginConnect(mURL, (int)mPort, null, null);
                            mStatus = Status.Connecting;
                        }
                        break;
                    case Status.Connecting:
                        {
                            if (mSocketAsync.IsCompleted)
                            {
                                mSocket.EndConnect(mSocketAsync);
                                if (mSocket.Connected)
                                {
                                    mStatus = Status.Connected;
                                    mRPCID = 1;
                                }
                                else
                                    Disconnect();
                                mSocketAsync = null;
                            }
                        }
                        break;
                    case Status.Connected:
                        {
                            // Now connected, subscribe for mining
                            SendRPC("mining.subscribe", null);
                            mExtraNOnceSize = -1;
                            mStatus = Status.Subscribing;
                        }
                        break;
                    case Status.Subscribing:
                        if (mExtraNOnceSize >= 0 && mUser != null && mPass != null)
                        {
                            // Done subscribing for mining, now authorize the worker
                            // {"params": ["slush.miner1", "password"], "id": 2, "method": "mining.authorize"}
                            string[] parms = new string[2];
                            parms[0] = mUser;
                            parms[1] = mPass;
                            SendRPC("mining.authorize", parms);
                            mStatus = Status.Authorizing;
                        }
                        break;
                    case Status.Authorizing:
                        if( mAuthorized )
                        {
                            mStatus = Status.Ready;
                        }
                        break;
                    case Status.Ready:
                        break;
                }

                Thread.Sleep(50);
            }
        }
    }
}

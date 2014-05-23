using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Bitnet.Client;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CentralMine.NET
{
    class US_Wallet : Upstream
    {
        Thread mThread;
        int mBlockCount = 0;
        bool mNewBlockReady = false;

        public US_Wallet(ClientManager cm)
            : base(cm)
        {
            mThread = new Thread(new ThreadStart(ThreadFunc));
            mThread.Start();
        }

        public override void Destroy()
        {
            mThread.Abort();
            base.Destroy();
        }

        public override WorkBlock GetWorkBlock()
        {
            WorkBlock work = null;

            JObject obj = null;
            try
            {
                // Get block from bitcoin
                BitnetClient bc = new BitnetClient("http://" + mURL + ":" + mPort);
                bc.Credentials = new NetworkCredential(mUser, mPass);
                mBlockCount = bc.GetBlockCount();
                obj = bc.GetWork();
                mNewBlockReady = false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get work!");
                Console.WriteLine(e.Message);
            }

            if (obj != null)
            {
                work = new WorkBlock(obj);
            }
            return work;
        }

        public override bool SubmitWork(WorkBlock work, uint solution)
        {
            // Submit this solution to bitcoin
            string data = work.GetSolutionString(solution);
            Console.WriteLine("Trying solution: " + data);
            BitnetClient bc = new BitnetClient("http://" + mURL + ":" + mPort);
            bc.Credentials = new NetworkCredential(mUser, mPass);
            bool success = bc.GetWork(data);
            if (!success)
            {
                data = work.GetSolutionString((uint)IPAddress.HostToNetworkOrder((int)solution));
                success = bc.GetWork(data);
            }

            return success;
        }

        public override bool NewBlockReady()
        {
            return mNewBlockReady;
        }

        void ThreadFunc()
        {
            while (true)
            {
                try
                {
                    if (mURL != null && mPort != 0 && mUser != null && mPass != null)
                    {
                        BitnetClient bc = new BitnetClient("http://" + mURL + ":" + mPort);
                        bc.Credentials = new NetworkCredential(mUser, mPass);
                        int blockCount = bc.GetBlockCount();
                        if (blockCount > mBlockCount)
                        {
                            mBlockCount = blockCount;
                            mNewBlockReady = true;
                        }
                    }
                }
                catch (Exception)
                {
                }

                Thread.Sleep(1000 * 10);
            }
        }
    }
}

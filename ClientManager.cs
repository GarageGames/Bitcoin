using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Bitnet.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;


namespace CentralMine.NET
{
    class ClientManager
    {
        public enum Currency
        {
            Bitcoin,
            Xencoin,
            Gamerscoin
        };

        EventLog mEventLog;

        Dictionary<uint, bool> mBlacklist;
        Dictionary<Currency, string> mCurrencyProviders;

        Listener mListener;
        public List<Client> mClients;
        Mutex mClientListMutex;

        Thread mUpdateThread;
        Email mMailer;
        Upstream mUpstream;
        
        public double mHashrate;
        int mPrevBlockIndex;
        public WorkBlock[] mPrevBlocks;
        public WorkBlock mBlock = null;

        public bool mDumpClients = false;
        public MiningTarget mMiningTarget;

        public long mBlocksSubmitted;
        public long mBlocksAccepted;
        
        public ClientManager()
        {
            mEventLog = new EventLog();
            mBlacklist = new Dictionary<uint, bool>();
            mBlacklist[0xC425E50F] = true;

            mBlocksSubmitted = 0;
            mBlocksAccepted = 0;

            #region Bitcoin
            MiningTarget bc = new MiningTarget();
            bc.mName = "Bitcoin";
            bc.mPOWAlgorithm = HashAlgorithm.DoubleSHA256;
            bc.mWallet = new WalletInfo();
            bc.mWallet.mRPCAddress = "127.0.0.1";
            bc.mWallet.mRPCPort = 8332;
            bc.mWallet.mRPCUser = "rpcuser";
            bc.mWallet.mRPCPass = "rpcpass";

            PoolInfo pi = new PoolInfo();
            pi.mName = "Slush";
            pi.mAddress = "stratum.bitcoin.cz";
            pi.mPort = 3333;
            pi.mUser = "f2mserver.worker1";
            pi.mPassword = "yezPpJrt";
            bc.mPools.Add(pi);
            #endregion
            #region Bitgem
            MiningTarget bg = new MiningTarget();
            bg.mName = "Bitgem";
            bg.mPOWAlgorithm = HashAlgorithm.Scrypt;
            bg.mWallet = new WalletInfo();
            bg.mWallet.mRPCAddress = "127.0.0.1";
            bg.mWallet.mRPCPort = 8348;
            bg.mWallet.mRPCUser = "rpcuser";
            bg.mWallet.mRPCPass = "rpcpass";

            pi = new PoolInfo();
            pi.mName = "JTeam";
            pi.mAddress = "us-east.jtcpools.org";
            pi.mPort = 3365;
            pi.mUser = "f2mserver.worker1";
            pi.mPassword = "torque9900";
            bg.mPools.Add(pi);
            #endregion
            #region Gamerscoin
            MiningTarget gc = new MiningTarget();
            gc.mName = "Gamerscoin";
            gc.mPOWAlgorithm = HashAlgorithm.Scrypt;
            gc.mWallet = new WalletInfo();
            gc.mWallet.mRPCAddress = "127.0.0.1";
            gc.mWallet.mRPCPort = 7332;
            gc.mWallet.mRPCUser = "rpcuser";
            gc.mWallet.mRPCPass = "rpcpass";

            pi = new PoolInfo();
            pi.mName = "gamerscoin";
            pi.mAddress = "gamers-coin.org";
            pi.mPort = 3333;
            pi.mUser = "rono.f2mserver";
            pi.mPassword = "torque9900";
            gc.mPools.Add(pi);
            #endregion
            #region AllMulti
            MiningTarget multi = new MiningTarget();
            multi.mName = "Mutli";
            multi.mPOWAlgorithm = HashAlgorithm.Scrypt;

            pi = new PoolInfo();
            pi.mName = "WeMineAll";
            pi.mAddress = "multi2.wemineall.com";
            pi.mPort = 5555;
            pi.mUser = "f2mserver.1";
            pi.mPassword = "x";
            multi.mPools.Add(pi);
            #endregion
            mMiningTarget = gc;
            //mMiningTarget = bg;
            //mMiningTarget = bc;
            //mMiningTarget = multi;

            //mUpstream = new US_Wallet(this);
            //mUpstream.SetHost(mMiningTarget.mWallet.mRPCAddress, mMiningTarget.mWallet.mRPCPort);
            //mUpstream.SetCredentials(mMiningTarget.mWallet.mRPCUser, mMiningTarget.mWallet.mRPCPass);
            mUpstream = new US_Stratum(this);
            mUpstream.SetHost(mMiningTarget.mPools[0].mAddress, mMiningTarget.mPools[0].mPort);
            mUpstream.SetCredentials(mMiningTarget.mPools[0].mUser, mMiningTarget.mPools[0].mPassword);

            //mUpstream = new US_Wallet();
            //mUpstream.SetHost("127.0.0.1", 7332);
            //mUpstream.SetCredentials("rpcuser", "rpcpass");
            //mUpstream = new US_Stratum();
            //mUpstream.SetHost("gamers-coin.org", 3333);
            //mUpstream.SetCredentials("rono.f2mserver", "torque9900");

            mPrevBlocks = new WorkBlock[5];
            mPrevBlockIndex = 0;

            mMailer = new Email();
            mClients = new List<Client>();
            mClientListMutex = new Mutex();
            mListener = new Listener(80, this);

            mUpdateThread = new Thread(new ThreadStart(Update));

            mHashrate = 0;
            BeginBlock();

            mUpdateThread.Start();
        }

        public void Close()
        {
            mUpstream.Destroy();

            // Kill the listener
            mListener.Close();

            // Kill the update thread
            mUpdateThread.Abort();

            // Close all clients
            foreach (Client c in mClients)
            {
                c.Close();
            }

            // Close the event log
            mEventLog.Close();
        }

        public void SetCurrency(Currency c)
        {
            //if (mCurrency != c)
            //{
            //    mEventLog.RecordEvent(EventLog.EventType.Server, string.Format("Currency changed from: {0} to {1}", mCurrency, c));
            //    mCurrency = c;                
            //}
        }

        public void AcceptClient(TcpClient client)
        {
            IPEndPoint ep = client.Client.RemoteEndPoint as IPEndPoint;
            byte[] bytes = ep.Address.GetAddressBytes();
            uint addr = (uint)(bytes[0] << 24) | (uint)(bytes[1] << 16) | (uint)(bytes[2] << 8) | bytes[3];
            if (!mBlacklist.ContainsKey(addr))
            {
                Client c = new Client(client, this);
                mEventLog.RecordEvent(EventLog.EventType.Network, string.Format("New connection from: {0}", ep.Address.ToString()));
                mClientListMutex.WaitOne();
                mClients.Add(c);
                mClientListMutex.ReleaseMutex();
            }
        }

        void BeginBlock()
        {
            mEventLog.RecordEvent(EventLog.EventType.Upstream, string.Format("Getting {0} block", mMiningTarget.mName));
            WorkBlock work = mUpstream.GetWorkBlock();
            if (work != null)
            {
                mEventLog.RecordEvent(EventLog.EventType.Upstream, string.Format("Got {0} block: {1}", mMiningTarget.mName, work.ToString()));

                // Put the current block in the previous list
                if (mBlock != null)
                {
                    mPrevBlocks[mPrevBlockIndex++] = mBlock;
                    if (mPrevBlockIndex >= mPrevBlocks.Length)
                        mPrevBlockIndex = 0;
                }

                mBlock = work;
                mBlock.mAlgorithm = mMiningTarget.mPOWAlgorithm;
            }
            else
            {
                mEventLog.RecordEvent(EventLog.EventType.Upstream, string.Format("GetWork failed"));
            }
        }

        void AssignWork(Client c)
        {
            if (mBlock != null)
            {
                //mEventLog.RecordEvent(EventLog.EventType.HashWork, string.Format("Allocating {0} hashes for client: {1}", c.mDesiredHashes, c.ToLogString()));
                HashManager.HashBlock hashes = mBlock.mHashMan.Allocate(c.mDesiredHashes, c);
                if (hashes != null)
                {
                    //mEventLog.RecordEvent(EventLog.EventType.HashWork, string.Format("Sending hash range ({0} - {1}) to client: {2}", hashes.Start, hashes.Start + hashes.Count, c.ToLogString()));
                    c.SendWork(hashes, mBlock);
                }
            }
        }

        public void WorkComplete(Client solver, bool solutionFound, uint solution)
        {
            mEventLog.RecordClientWork(solver);
            WorkBlock block = solver.mCurrentBlock;
            block.mHashMan.FinishBlock(solver.mHashBlock);
            solver.mHashBlock = null;

            if (solutionFound && mBlock == block)
            {
                mBlocksSubmitted++;
                bool success = mUpstream.SubmitWork(block, solution);
                //if (!success)
                //    success = mUpstream.SubmitWork(block, (uint)IPAddress.HostToNetworkOrder((int)solution));

                // Start a new block
                if (success)
                {
                    // Send email notification about this found solution
                    /*
                    TimeSpan span = DateTime.Now - block.mHashMan.mStartTime;
                    string hashrate = string.Format("{0:N}", block.mHashMan.mHashesDone / span.TotalSeconds);
                    string body = "Found solution for " + block.mCurrency + " block: \n" + block.ToString() + "\n\n";
                    body += "Solution string: " + data + "\n";
                    body += "Block Accepted: " + success.ToString() + "\n";
                    body += "Hashes Done: " + block.mHashMan.mHashesDone + "\n";
                    body += "Time Spent: " + span.ToString() + "\n";
                    body += "Hashrate: " + hashrate + "\n";
                    body += "Clients: " + mClients.Count + "\n";
                    body += "\n\n";
                    //mMailer.SendEmail(body);
                    */
                    //mMailer.SendEmail("Block Accepted");

                    string data = block.GetSolutionString(solution);
                    mEventLog.RecordEvent(EventLog.EventType.Upstream, string.Format("Work accepted! solution: {0}, dataString: {1}", solution, data));

                    BeginBlock();
                    mBlocksAccepted++;
                }
                else
                {
                    string data = block.GetSolutionString(solution);
                    mEventLog.RecordEvent(EventLog.EventType.Upstream, string.Format("Work not accepted. solution: {0}, dataString: {1}", solution, data));
                }
            }
        }

        public uint GetHashesDone()
        {
            uint hashesDone = mBlock.mHashMan.mHashesDone;
            foreach (WorkBlock b in mPrevBlocks)
            {
                if (b != null)
                    hashesDone += b.mHashMan.mHashesDone;
            }
            return hashesDone;
        }

        void Update()
        {
            while (true)
            {
                double oldHashrate = mHashrate;                
                mHashrate = 0;
                if (mBlock == null || mUpstream.NewBlockReady() ||  mBlock.mHashMan.IsComplete() || mBlock.mHashMan.IsExpired() )
                {
                    // Start work on a new block
                    BeginBlock();
                }

                Client clientThatWantsInfo = null;

                if (mClientListMutex.WaitOne(1))
                {
                    int clientsOnline = mClients.Count;
                    foreach (Client c in mClients)
                    {
                        bool stillAlive = c.Update();
                        if (!stillAlive)
                        {
                            if (c.mCurrentBlock != null)
                                c.mCurrentBlock.mHashMan.FreeBlock(c.mHashBlock);
                            mClients.Remove(c);
                            break;
                        }
                        mHashrate += c.mHashrate;

                        if (c.mState == Client.State.Busy && c.mCurrentBlock != mBlock)
                            c.StopWork();

                        if (c.mState == Client.State.Ready)
                            AssignWork(c);

                        if (c.mStatusClient)
                            c.SendStatus(clientsOnline, oldHashrate);

                        if (c.mClientInfoRequested && clientThatWantsInfo == null)
                        {
                            clientThatWantsInfo = c;
                            c.mClientInfoRequested = false;
                        }
                    }

                    if (clientThatWantsInfo != null)
                    {
                        string info = "{\"clients\":[";
                        foreach (Client c in mClients)
                        {
                            info += c.ToJSON() + ",";
                        }
                        info = info.Substring(0, info.Length - 1);
                        info += "]}";
                        clientThatWantsInfo.SendClientInfo(info);
                    }

                    mClientListMutex.ReleaseMutex();
                    Thread.Sleep(50);
                }
                else
                {
                    Thread.Sleep(1);
                }

                if (mDumpClients)
                {
                    FileStream file = new FileStream("clients.txt", FileMode.Create);
                    StreamWriter sw = new StreamWriter(file);

                    mClientListMutex.WaitOne();
                    foreach (Client c in mClients)
                    {
                        string str = c.ToString();
                        sw.WriteLine(str);
                    }
                    mClientListMutex.ReleaseMutex();

                    sw.Close();
                    mDumpClients = false;
                }
            }
        }
    }
}

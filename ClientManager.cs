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


namespace CentralMine.NET
{
    public class ClientManager
    {
        Listener mListener;
        public List<Client> mClients;
        Mutex mClientListMutex;

        Thread mUpdateThread;
        Email mMailer;
        
        public double mHashrate;
        int mPrevBlockIndex;
        public Block[] mPrevBlocks;
        public Block mBlock = null;

        public ClientManager()
        {
            mPrevBlocks = new Block[5];
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

        public void AcceptClient(TcpClient client)
        {
            Client c = new Client(client, this);
            mClientListMutex.WaitOne();
            mClients.Add(c);
            mClientListMutex.ReleaseMutex();
        }

        void BeginBlock()
        {
            // Put the current block in the previous list
            if (mBlock != null)
            {
                mPrevBlocks[mPrevBlockIndex++] = mBlock;
                if (mPrevBlockIndex >= mPrevBlocks.Length)
                    mPrevBlockIndex = 0;
            }

            // Get block from bitcoin
            BitnetClient bc = new BitnetClient("http://127.0.0.1:8332");
            bc.Credentials = new NetworkCredential("rpcuser", "rpcpass");
            JObject obj = bc.GetWork();
            Console.WriteLine("starting block: " + obj.ToString());
            mBlock = new Block(obj);                  
        }

        void AssignWork(Client c)
        {
            if (mBlock != null)
            {
                HashManager.HashBlock hashes = mBlock.mHashMan.Allocate(c.mDesiredHashes, c);
                if (hashes != null)
                    c.SendWork(hashes, mBlock);
            }
        }

        public void WorkComplete(Client solver, bool solutionFound, uint solution)
        {
            Block block = solver.mCurrentBlock;
            block.mHashMan.FinishBlock(solver.mHashBlock);
            solver.mHashBlock = null;

            if (solutionFound)
            {
                // Submit this solution to bitcoin
                string data = block.GetSolutionString(solution);
                Console.WriteLine("Trying solution: " + data);
                BitnetClient bc = new BitnetClient("http://127.0.0.1:8332");
                bc.Credentials = new NetworkCredential("rpcuser", "rpcpass");
                bool success = bc.GetWork(data);

                // Send email notification about this found solution
                TimeSpan span = DateTime.Now - block.mHashMan.mStartTime;
                string hashrate = string.Format("{0:N}", block.mHashMan.mHashesDone / span.TotalSeconds);
                string body = "Found solution for block: \n" + block.ToString() + "\n\n";
                body += "Solution string: " + data + "\n";
                body += "Block Accepted: " + success.ToString() + "\n";
                body += "Hashes Done: " + block.mHashMan.mHashesDone + "\n";
                body += "Time Spent: " + span.ToString() + "\n";
                body += "Hashrate: " + hashrate + "\n";
                body += "Clients: " + mClients.Count + "\n";
                body += "\n\n";
                mMailer.SendEmail(body);

                // Start a new block
                BeginBlock();
            }
        }

        public uint GetHashesDone()
        {
            uint hashesDone = mBlock.mHashMan.mHashesDone;
            foreach (Block b in mPrevBlocks)
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
                mHashrate = 0;
                if (mBlock.mHashMan.IsComplete() || mBlock.mHashMan.IsExpired())
                {
                    // Start work on a new block
                    BeginBlock();
                }

                if (mClientListMutex.WaitOne(1))
                {
                    Console.WriteLine("Starting Client Update");
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

                        if (c.mState == Client.State.Ready)
                            AssignWork(c);
                    }
                    mClientListMutex.ReleaseMutex();
                    Console.WriteLine("Done Client Update");
                    Thread.Sleep(50);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }
    }
}

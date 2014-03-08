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

        public HashManager mHashMan;

        bool mDoingBlock;
        public Block mBlock;
        //uint mHashStart;

        public ClientManager()
        {
            mClients = new List<Client>();
            mClientListMutex = new Mutex();
            mListener = new Listener(555, this);

            mUpdateThread = new Thread(new ThreadStart(Update));

            mDoingBlock = false;
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
            if (!mDoingBlock)
            {
                // Get block from bitcoin
                BitnetClient bc = new BitnetClient("http://127.0.0.1:8332");
                bc.Credentials = new NetworkCredential("rpcuser", "rpcpass");
                JObject obj = bc.GetWork();
                Console.WriteLine("starting block: " + obj.ToString());
                mBlock = new Block(obj);

                // Set the hash start to 0
                mHashMan = new HashManager();

                // Set in block
                mDoingBlock = true;
            }
        }

        void AssignWork(Client c)
        {
            if (mDoingBlock)
            {
                HashManager.HashBlock hashes = mHashMan.Allocate(c.mDesiredHashes, c);
                if( hashes != null )
                    c.SendWork(hashes, mBlock);
            }
        }

        public void WorkComplete(Client solver, bool solutionFound, uint solution)
        {
            mHashMan.FinishBlock(solver.mHashBlock);
            solver.mHashBlock = null;

            if (solutionFound)
            {
                // Submit this solution to bitcoin
                string data = mBlock.GetSolutionString(solution);
                Console.WriteLine("Trying solution: " + data);
                BitnetClient bc = new BitnetClient("http://127.0.0.1:8332");
                bc.Credentials = new NetworkCredential("rpcuser", "rpcpass");
                bool success = bc.GetWork(data);

                // Get a new block
                mDoingBlock = false;
                BeginBlock();
            }
        }

        void Update()
        {
            while (true)
            {
                if (mDoingBlock)
                {
                    if (mHashMan.IsComplete())
                    {
                        mDoingBlock = false;
                        BeginBlock();
                    }
                }

                mClientListMutex.WaitOne();
                foreach (Client c in mClients)
                {
                    bool stillAlive = c.Update();
                    if (!stillAlive)
                    {
                        mHashMan.FreeBlock(c.mHashBlock);
                        mClients.Remove(c);
                        break;
                    }

                    if (c.mState == Client.State.Ready)
                        AssignWork(c);

                    if (c.mState == Client.State.Busy && c.mCurrentBlock != mBlock)
                        c.StopWork(); // Not doing the current block, stop
                }
                mClientListMutex.ReleaseMutex();
                Thread.Sleep(50);
            }
        }
    }
}

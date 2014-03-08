using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using OpenSSL.Crypto.EC;

namespace BitCoinClient
{
    public class Miner
    {
        Thread mThread;
        Bitcoin mBitcoin;

        public Miner(Bitcoin bitcoin)
        {
            mBitcoin = bitcoin;
            Program.RandomSeed();

            mThread = new Thread(new ThreadStart(DoMining));
        }

        void Start()
        {
            mThread.Start();
        }

        void Stop()
        {
            if (mThread.ThreadState == ThreadState.Running)
            {
                mThread.Abort();
                mThread.Join();
            }
        }


        Block CreateBlock(Key key)
        {
            Block block = new Block();

            // Create transaction from nowhere to us!
            Transaction tx = new Transaction();
            TransactionInput input = new TransactionInput();            
            TransactionOutput output = new TransactionOutput(key);
            tx.AddInput(input);
            tx.AddOutput(output);

            block.AddTransaction(tx);

            Transaction[] transactions = mBitcoin.GetTransactions();

            return block;
        }

        public void DoMining()
        {
            // Generate key
            Key key = Program.GenerateKey();

            while (true)
            {
                // Make sure we are connected to the network

                // Create block
                Block block = CreateBlock(key);
            }
        }
    }
}

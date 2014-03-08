using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BitCoinClient
{
    public class Bitcoin
    {
        Dictionary<string, NodeConnection> mNodes;

        Dictionary<string, Transaction> mTransactions;
        Dictionary<string, Block> mBlocks;

        public Bitcoin()
        {
            mNodes = new Dictionary<string, NodeConnection>();
            mTransactions = new Dictionary<string, Transaction>();
            mBlocks = new Dictionary<string, Block>();
        }

        public void AddNode(string nodeAddr, ushort nodePort = 8333)
        {
            string key = nodeAddr + "_" + nodePort.ToString();

            if (!mNodes.ContainsKey(key))
            {
                NodeConnection node = new NodeConnection(this, nodeAddr, nodePort);
                mNodes[key] = node;
            }
        }

        public void HandleInvPacket(NodeConnection node, byte[] payload)
        {
            MemoryStream stream = new MemoryStream(payload);
            BinaryReader br = new BinaryReader(stream);

            List<Transaction> transLoadList = new List<Transaction>();
            List<Block> blockLoadList = new List<Block>();

            ulong count = Program.ReadVarInt(br);
            for (ulong i = 0; i < count; i++)
            {
                uint type = br.ReadUInt32();
                byte[] hash = br.ReadBytes(32);
                if (type != 0)
                {
                    string str = Program.HashToString(hash);
                    if (type == 1)
                    {
                        if (mTransactions.ContainsKey(str))
                        {
                            // We already know about this transaction
                            Transaction t = mTransactions[str];
                            if (t.Status == Transaction.DataStatus.NoData)
                            {
                                // No details about this transaction loaded yet, request it
                                t.Status = Transaction.DataStatus.Requested;
                                transLoadList.Add(t);
                            }
                        }
                        else
                        {
                            // Transaction we dont know about yet
                            Transaction t = new Transaction(hash, Transaction.DataStatus.Requested);

                            // Add it to the transaction dictionary
                            mTransactions[str] = t;

                            // Add it to the list of transactions to requrest data about
                            transLoadList.Add(t);
                        }
                    }
                    else
                    {
                        if (mBlocks.ContainsKey(str))
                        {
                            // We already know about this block
                            Block b = mBlocks[str];
                            if (b.Status == NetworkDataObject.DataStatus.NoData)
                            {
                                // No details about this block loaded yet, request it
                                b.Status = NetworkDataObject.DataStatus.Requested;
                                blockLoadList.Add(b);
                            }
                        }
                        else
                        {
                            // Block we dont know about yet
                            Block b = new Block(hash, NetworkDataObject.DataStatus.Requested);

                            // Add it to the block dictionary
                            mBlocks[str] = b;

                            // Add it to the list of blocks to requrest data about
                            blockLoadList.Add(b);
                        }
                    }
                }
            }

            // Load the transactions and blocks we dont have loaded
            node.RequestData(transLoadList, blockLoadList);

            br.Close();
        }

        public void LoadTransaction(byte[] data)
        {
            byte[] hash = Program.GenerateHash(data);
            string hashStr = Program.HashToString(hash);

            if (mTransactions.ContainsKey(hashStr))
            {
                Transaction t = mTransactions[hashStr];
                t.Load(data);
            }
            else
            {
                Console.WriteLine("Unknown transaction");
            }
        }

        public Transaction[] GetTransactions()
        {
            List<Transaction> otrans = new List<Transaction>();

            List<KeyValuePair<string, Transaction>> transactions = mTransactions.ToList();
            foreach (KeyValuePair<string, Transaction> kv in transactions)
            {
                if (kv.Value.IsCoinBase() || !kv.Value.IsFinal())
                    continue;


                foreach (TransactionInput txin in kv.Value.Inputs)
                {

                }
            }

            return otrans.ToArray();
        }
    }
}

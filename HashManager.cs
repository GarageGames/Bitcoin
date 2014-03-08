using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentralMine.NET
{
    public class HashManager
    {
        public class HashBlock
        {

            public uint Start;
            public uint Count;
            public Client Owner;
        };

        List<HashBlock> mFreeBlocks;
        List<HashBlock> mBusyBlocks;
        List<HashBlock> mDoneBlocks;

        public uint mHashesDone = 0;
        public uint mHashesTotal = 0xFFFFFFFF;
        public uint mHashrate = 0;
        
        public HashManager()
        {
            mFreeBlocks = new List<HashBlock>();
            mBusyBlocks = new List<HashBlock>();
            mDoneBlocks = new List<HashBlock>();

            // Add initial free block
            HashBlock block = new HashBlock();
            block.Owner = null;
            block.Start = 0;
            block.Count = mHashesTotal;
            mFreeBlocks.Add(block);
        }

        HashBlock FindFreeBlock(uint desiredSize)
        {
            HashBlock free = null;

            foreach (HashBlock hb in mFreeBlocks)
            {
                if (hb.Count >= desiredSize)
                {
                    free = hb;
                    break;
                }
            }

            if (free == null && mFreeBlocks.Count > 0)
            {
                // Didnt find a large enough block, use the first block instead
                free = mFreeBlocks[0];
            }

            return free;
        }

        public HashBlock Allocate(uint desired, Client c)
        {
            HashBlock block = null;
            
            HashBlock free = FindFreeBlock(desired);
            if (free != null)
            {
                if (free.Count > desired)
                {
                    // Split this block
                    block = new HashBlock();
                    block.Owner = c;
                    block.Start = free.Start;
                    block.Count = desired;
                    mBusyBlocks.Add(block);

                    free.Start += desired;
                    free.Count -= desired;
                }
                else
                {
                    // Remove the free block from the free list
                    mFreeBlocks.Remove(free);

                    // Assign the free block to the client
                    free.Owner = c;

                    // Add the block to the busy list
                    block = free;
                    mBusyBlocks.Add(block);
                }
            }

            return block;
        }

        public void FinishBlock(HashBlock block)
        {
            mBusyBlocks.Remove(block);
            mDoneBlocks.Add(block);

            // Stat Tracking
            {
                mHashesDone = 0;
                foreach (HashBlock hb in mDoneBlocks)
                    mHashesDone += hb.Count;

                //Console.WriteLine("Block finished: " + mHashesDone + " (%" + ((float)mHashesDone / (float)mHashesTotal) * 100 + ")");
            }
        }

        public void FreeBlock(HashBlock block)
        {
            if (block != null)
            {
                mBusyBlocks.Remove(block);
                mFreeBlocks.Insert(0, block);
                block.Owner = null;
            }
        }

        public bool IsComplete()
        {
            return mFreeBlocks.Count == 0 && mBusyBlocks.Count == 0;
        }
    }
}

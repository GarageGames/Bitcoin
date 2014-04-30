using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinerLib_cs
{
    public class MTM
    {
        int mThreadCount;
        uint mHashRate;
        Timer mTimer;
        WorkThread[] mThreads;
        Work mCurrentWork;

        public MTM(int threadCount, bool useSSE, float gpuPercentage)
        {
            mThreadCount = threadCount;
            mThreads = new WorkThread[mThreadCount];
            for (int i = 0; i < mThreadCount; i++)
                mThreads[i] = new WorkThread();

            mHashRate = 0;
            mCurrentWork = null;
            mTimer = new Timer();
        }

        public void Destroy()
        {
            foreach (WorkThread t in mThreads)
                t.Destroy();
        }

        public void Update(Connection conn)
        {
            if (mCurrentWork != null)
            {
                bool threadsAllDone = true;
                foreach (WorkThread t in mThreads)
                {
                    if (!t.IsWorkDone())
                    {
                        threadsAllDone = false;
                        break;
                    }
                }

                if (threadsAllDone)
                {
                    uint hashes = 0;
                    uint solution = 0;
                    bool solutionFound = false;

                    foreach (WorkThread t in mThreads)
                    {
                        hashes += t.HashesDone;
                        if (t.SolutionFound)
                        {
                            solutionFound = true;
                            solution = t.Solution;
                        }
                    }
                    
                    conn.SendWorkComplete(solutionFound, Scrypt.ByteReverse(solution), hashes);
                    mCurrentWork = null;
                    mTimer.Stop();
                    mHashRate = (uint)((double)hashes / mTimer.GetDuration());
                }
            }
        }

        public void StartWork(Work work)
        {
            mTimer.Start();
            mCurrentWork = work;

            uint hashStart = work.hashStart;
            uint hashCount = work.hashCount; 
    
            uint hashesPerThread = (uint)(hashCount / mThreadCount);
            for( int i = 0; i < mThreadCount; i++ )
            {
                uint hashes = hashesPerThread;
                if( hashes > hashCount )
                    hashes = hashCount;
                hashCount -= hashes;

                mThreads[i].StartWork(hashStart, hashes, work);
                hashStart += hashes;
            }
        }

        public uint HashRate
        {
            get { return mHashRate; }
        }
    }
}

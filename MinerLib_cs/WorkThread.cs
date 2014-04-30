using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MinerLib_cs
{
    public class WorkThread
    {
        Work mWork;
        uint mHashStart;
        uint mHashCount;
        uint mHashesDone;
        uint mSolution;
        bool mSolutionFound;

        Thread mThread;
        bool mWorkToDo;
        bool mWorkDone;

        public WorkThread()
        {
            mWork = new Work();
            mHashCount = 0;

            mThread = new Thread(new ThreadStart(WorkFunction));
            mThread.Start();
        }

        public void Destroy()
        {
            mThread.Abort();
        }

        void WorkFunction()
        {
            while (true)
            {
                if (mWorkToDo)
                {
                    // There is work to be done!
                    switch (mWork.hashAlgorithm)
                    {
                        case Work.Algorithm.SHA256:     // SHA256^2
                            break;
                        case Work.Algorithm.Scrypt:
                            ScryptHashes();
                            break;
                        default:
                            break;
                    }

                    // All done
                    mWorkDone = true;
                }
                                
                Thread.Sleep(1);
            }
        }

        public void StartWork(uint hashStart, uint hashCount, Work work)
        {
            mHashStart = hashStart;
            mHashCount = hashCount;
            mWork.Copy(work);

            mHashesDone = 0;
            mSolutionFound = false;
            
            mWorkDone = false;
            mWorkToDo = true;
        }

        public bool IsWorkDone()
        {
            return mWorkDone;
        }

        public uint HashesDone
        {
            get { return mHashesDone; }
        }

        public uint Solution
        {
            get { return mSolution; }
        }

        public bool SolutionFound
        {
            get { return mSolutionFound; }
        }

        void ScryptHashes()
        {
            ulong end = mHashStart + mHashCount;
            Scrypt scryptData = new Scrypt(mWork);
            for( ulong i = mHashStart; i < end; i++ )
            {
                mHashesDone++;
                bool success = scryptData.Hash((uint)i);
                if( success )
                {
                    mSolution = (uint)i;
                    mSolutionFound = true;
                    break;
                }
            }
        }
    }
}

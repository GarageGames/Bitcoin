using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinerLib_cs
{
    public class Miner
    {
        MTM mMTM;
        Connection mConn;

        const string mHostName = "ronsTestMachine.cloudapp.net";

        public Miner(string memberName, string productName, string platform, int threadCount = 1, bool useSSE = false, float gpuPercentage = 0)
        {
            mMTM = new MTM(threadCount, useSSE, gpuPercentage);
            mConn = new Connection(memberName, productName, platform);
            mConn.ConnectTo(mHostName);
        }

        public void Destroy()
        {
            mConn.Destroy();
            mMTM.Destroy();
        }

        public void Update()
        {
            mConn.Update();
            mMTM.Update(mConn);
            if (mConn.State == Connection.ConnectionState.Connected)
            {
                Work work = mConn.GetWork();
                if (work != null)
                {
                    Console.WriteLine("HashRate: {0}", mMTM.HashRate);
                    mMTM.StartWork(work);
                }
            }
            else if (mConn.State == Connection.ConnectionState.Disconnected)
            {
                mConn.ConnectTo(mHostName);
            }
        }
    }
}

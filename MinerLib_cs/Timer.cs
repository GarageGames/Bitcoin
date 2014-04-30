using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MinerLib_cs
{
    class Timer
    {
        Stopwatch mSW;

        public Timer()
        {
            mSW = new Stopwatch();
        }

        public void Start()
        {
            mSW.Reset();
            mSW.Start();
        }

        public void Stop()
        {
            mSW.Stop();
        }

        public double GetDuration()
        {
            return mSW.Elapsed.TotalSeconds;
        }
    }
}

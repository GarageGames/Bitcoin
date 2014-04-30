using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMiner
{
    class Program
    {
        static void Main(string[] args)
        {
            bool standard = MinerLib_cs.UnitTests.TestStandard();
            bool sse = MinerLib_cs.UnitTests.TestSIMD();
            bool ocl = MinerLib_cs.UnitTests.TestOpenCL();
            Console.WriteLine("UnitTests: standard({0}), sse({1}), ocl({2})", standard, sse, ocl);

            int threadCount = Environment.ProcessorCount - 1;
            if (threadCount < 1)
                threadCount = 1;

            MinerLib_cs.Miner m = new MinerLib_cs.Miner("MiningTest", "CSharpMiner", "Windows", threadCount);

            while (true)
            {
                m.Update();
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}

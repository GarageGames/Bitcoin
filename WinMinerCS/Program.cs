using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using MinerLib;

namespace WinMinerCS
{
    class Program
    {
        static void Main(string[] args)
        {
            int threads = Environment.ProcessorCount - 1;
            if (threads < 1)
                threads = 1;

            F2M.F2M_Initialize();
            IntPtr conn = F2M.F2M_Connection_Create("MiningTest", "C# Dll", "Windows", threads * 10000);
            IntPtr mtm = F2M.F2M_MTM_Create(threads, true, 50);

            ushort HostPort = 80;
            string HostAddress = "ronsTestMachine.cloudapp.net";
            F2M.F2M_Connection_Connect(conn, HostAddress, HostPort);

            while (true)
            {
                F2M.F2M_Connection_Update(conn);
                F2M.F2M_MTM_Update(mtm, conn);

                int state = F2M.F2M_Connection_GetState(conn);
                switch (state)
                {
                    case 1: // Connected
                        {
                            IntPtr work = F2M.F2M_Connection_GetWork(conn);
                            if (work != IntPtr.Zero)
                            {
                                Console.WriteLine("starting work, {0} hashes/sec\n", F2M.F2M_MTM_GetHashRate(mtm));
                                F2M.F2M_MTM_StartWork(mtm, work);
                            }
                        }
                        break;
                    case 2: // Disconnected
                        F2M.F2M_Connection_Connect(conn, HostAddress, HostPort);
                        break;
                }
                Thread.Sleep(10);
            }

            F2M.F2M_Connection_Destroy(conn);
            F2M.F2M_MTM_Destroy(conn);
            F2M.F2M_Shutdown();
        }
    }
}

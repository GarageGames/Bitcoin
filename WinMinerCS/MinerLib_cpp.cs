using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MinerLib
{
    public class F2M
    {
        #region Global
        [DllImport("MinerLib_cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void F2M_Initialize();

        [DllImport("MinerLib_cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void F2M_Shutdown();
        #endregion

        #region MTM
        [DllImport("MinerLib_cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr F2M_MTM_Create(int threadCount, bool useSSE, float gpuPercentage);

        [DllImport("MinerLib_cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void F2M_MTM_Destroy(IntPtr threadManager);

        [DllImport("MinerLib_cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void F2M_MTM_Update(IntPtr threadManager, IntPtr connection);

        [DllImport("MinerLib_cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void F2M_MTM_StartWork(IntPtr threadManager, IntPtr work);

        [DllImport("MinerLib_cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint F2M_MTM_GetHashRate(IntPtr threadManager);
        #endregion

        #region Connection
        [DllImport("MinerLib_cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr F2M_Connection_Create(string memberName, string productName, string platform, int initialHashes = 5000);

        [DllImport("MinerLib_cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void F2M_Connection_Destroy(IntPtr connection);

        [DllImport("MinerLib_cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void F2M_Connection_Connect(IntPtr connection, string hostAddress, ushort port);

        [DllImport("MinerLib_cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void F2M_Connection_Update(IntPtr connection);

        [DllImport("MinerLib_cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int F2M_Connection_GetState(IntPtr connection);

        [DllImport("MinerLib_cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr F2M_Connection_GetWork(IntPtr connection);
        #endregion
    };
}
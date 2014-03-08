using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace BitCoinClient
{
    public class OpenSSL
    {
        [DllImport("libeay32.dll")]
        public static extern void RAND_add(byte[] buff, int num, double entropy);

        [DllImport("libeay32.dll")]
        public static extern void RAND_bytes(byte[] buff, int num);

    }
}

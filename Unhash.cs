using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentralMine.NET
{
    class Unhash
    {
        public Unhash()
        {
            //uint val = UnSigma1(0x6a09e667);
        }

        public uint ROTATE(uint a, int n)
        {
            uint val = (((a) << (n)) | (((a) & 0xffffffff) >> (32 - (n))));
            return val;
        }

        public uint ROTATE_R(uint a, int n)
        {
            uint val = (a >> n) | (a << (32 - n));
            return val;
        }

        public uint Sigma0(uint x)
        {
            uint val = (ROTATE((x), 30) ^ ROTATE((x), 19) ^ ROTATE((x), 10));
            return val;
        }

        public uint Sigma1(uint x)
        {
            uint val = (ROTATE((x), 26) ^ ROTATE((x), 21) ^ ROTATE((x), 7));
            return val;
        }

        public uint UnSigma1(uint x)
        {
            uint a = ROTATE(x, 26);
            uint b = ROTATE(x, 21);
            uint c = ROTATE(x, 7);

            uint val = a ^ b ^ c;

            for (ulong i = 0; i < 0x100000000; i++)
            {
                a = ROTATE_R((uint)i, 26);
                b = ROTATE_R((uint)i, 21);
                c = ROTATE_R((uint)i, 7);

                uint tval = a ^ b ^ c;
                if (tval == val)
                {
                    Console.WriteLine("Potential solution: 0x{0:X2}", tval);
                }
            }

            return val;


        }

        public uint sigma0(uint x)
        {
            uint val = (ROTATE((x), 25) ^ ROTATE((x), 14) ^ ((x) >> 3));
            return val;
        }

        public uint sigma1(uint x)
        {
            uint val = (ROTATE((x), 15) ^ ROTATE((x), 13) ^ ((x) >> 10));
            return val;
        }

        public uint Ch(uint x, uint y, uint z)
        {
            uint val = (((x) & (y)) ^ ((~(x)) & (z)));
            return val;
        }

        public uint Maj(uint x, uint y, uint z)
        {
            uint val = (((x) & (y)) ^ ((x) & (z)) ^ ((y) & (z)));
            return val;
        }

        uint[] K256 = { 0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5, 0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174, 0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da, 0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967, 0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85, 0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070, 0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3, 0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2 };
        /*
        void Undo()
        {
            uint a, b, c, d, e, f, g, h;
            //                              a           b           c           d           e           f           g           h
            //uint[] staticHashInit = { 0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19 };

            d = 0x98C7E2A2 + W[0]; 
            h = 0xFC08884D + W[0];
            c = 0xCD2A11AE + e1(d) + Ch(d, 0x510e527f, 0x9b05688c) + W[1]; 
            g = 0x90BB1E3C + e1(d) + Ch(d, 0x510e527f, 0x9b05688c) + W[1] + e0(h) + Maj(h, 0x6a09e667, 0xbb67ae85);            
            b = 0x0C2E12E0 + e1(c) + Ch(c, d, 0x510e527f) + W[2]; 
            f = 0x50C6645B + e1(c) + Ch(c, d, 0x510e527f) + W[2] + e0(g) + Maj(g, 0x5be0cd19, 0x6a09e667);
            a = 0xA4CE148B + e1(b) + Ch(b, c, d) + W[3]; 
            e = 0x3AC42E24 + e1(b) + Ch(b, c, d) + W[3] + e0(f) + Maj(f, g, h);
            h = d + e1(a) + Ch(a, b, c) + K256[4] + W[4] + h; 
            d = d + e1(a) + Ch(a, b, c) + K256[4] + W[4] + e0(e) + Maj(e, f, g);
            g = c + e1(h) + Ch(h, a, b) + K256[5] + W[5] + g; 
            c = c + e1(h) + Ch(h, a, b) + K256[5] + W[5] + e0(d) + Maj(d, e, f);
            f = b + e1(g) + Ch(g, h, a) + K256[6] + W[6] + f; 
            b = b + e1(g) + Ch(g, h, a) + K256[6] + W[6] + e0(c) + Maj(c, d, e);
            e = a + e1(f) + Ch(f, g, h) + K256[7] + W[7] + e; 
            a = a + e1(f) + Ch(f, g, h) + K256[7] + W[7] + e0(b) + Maj(b, c, d);

            d = h + e1(e) + Ch(e, f, g) + K256[8] + 0x80000000 + d; 
            h = h + e1(e) + Ch(e, f, g) + K256[8] + 0x80000000 + e0(a) + Maj(a, b, c);
            c = g + e1(d) + Ch(d, e, f) + K256[9] + 0x00000000 + c; 
            g = g + e1(d) + Ch(d, e, f) + K256[9] + 0x00000000 + e0(h) + Maj(h, a, b);
            b = f + e1(c) + Ch(c, d, e) + K256[10] + 0x00000000 + b; 
            f = f + e1(c) + Ch(c, d, e) + K256[10] + 0x00000000 + e0(g) + Maj(g, h, a);
            a = e + e1(b) + Ch(b, c, d) + K256[11] + 0x00000000 + a; 
            e = e + e1(b) + Ch(b, c, d) + K256[11] + 0x00000000 + e0(f) + Maj(f, g, h);
            h = d + e1(a) + Ch(a, b, c) + K256[12] + 0x00000000 + h; 
            d = d + e1(a) + Ch(a, b, c) + K256[12] + 0x00000000 + e0(e) + Maj(e, f, g);
            g = c + e1(h) + Ch(h, a, b) + K256[13] + 0x00000000 + g; 
            c = c + e1(h) + Ch(h, a, b) + K256[13] + 0x00000000 + e0(d) + Maj(d, e, f);
            f = b + e1(g) + Ch(g, h, a) + K256[14] + 0x00000000 + f; 
            b = b + e1(g) + Ch(g, h, a) + K256[14] + 0x00000000 + e0(c) + Maj(c, d, e);
            e = a + e1(f) + Ch(f, g, h) + K256[15] + 0x00000100 + e; 
            a = a + e1(f) + Ch(f, g, h) + K256[15] + 0x00000100 + e0(b) + Maj(b, c, d);

            d = h + e1(e) + Ch(e, f, g) + K256[16] + W[16] + d; 
            h = h + e1(e) + Ch(e, f, g) + K256[16] + W[16] + e0(a) + Maj(a, b, c);
            c = g + e1(d) + Ch(d, e, f) + K256[17] + W[17] + c; 
            g = g + e1(d) + Ch(d, e, f) + K256[17] + W[17] + e0(h) + Maj(h, a, b);
            b = f + e1(c) + Ch(c, d, e) + K256[18] + W[18] + b; 
            f = f + e1(c) + Ch(c, d, e) + K256[18] + W[18] + e0(g) + Maj(g, h, a);
            a = e + e1(b) + Ch(b, c, d) + K256[19] + W[19] + a; 
            e = e + e1(b) + Ch(b, c, d) + K256[19] + W[19] + e0(f) + Maj(f, g, h);
            h = d + e1(a) + Ch(a, b, c) + K256[20] + W[20] + h; 
            d = d + e1(a) + Ch(a, b, c) + K256[20] + W[20] + e0(e) + Maj(e, f, g);
            g = c + e1(h) + Ch(h, a, b) + K256[21] + W[21] + g; 
            c = c + e1(h) + Ch(h, a, b) + K256[21] + W[21] + e0(d) + Maj(d, e, f);
            f = b + e1(g) + Ch(g, h, a) + K256[22] + W[22] + f; 
            b = b + e1(g) + Ch(g, h, a) + K256[22] + W[22] + e0(c) + Maj(c, d, e);
            e = a + e1(f) + Ch(f, g, h) + K256[23] + W[23] + e; 
            a = a + e1(f) + Ch(f, g, h) + K256[23] + W[23] + e0(b) + Maj(b, c, d);

            d = h + e1(e) + Ch(e, f, g) + K256[24] + W[24] + d; 
            h = h + e1(e) + Ch(e, f, g) + K256[24] + W[24] + e0(a) + Maj(a, b, c);
            c = g + e1(d) + Ch(d, e, f) + K256[25] + W[25] + c; 
            g = g + e1(d) + Ch(d, e, f) + K256[25] + W[25] + e0(h) + Maj(h, a, b);
            b = f + e1(c) + Ch(c, d, e) + K256[26] + W[26] + b; 
            f = f + e1(c) + Ch(c, d, e) + K256[26] + W[26] + e0(g) + Maj(g, h, a);
            a = e + e1(b) + Ch(b, c, d) + K256[27] + W[27] + a; 
            e = e + e1(b) + Ch(b, c, d) + K256[27] + W[27] + e0(f) + Maj(f, g, h);
            h = d + e1(a) + Ch(a, b, c) + K256[28] + W[28] + h; 
            d = d + e1(a) + Ch(a, b, c) + K256[28] + W[28] + e0(e) + Maj(e, f, g);
            g = c + e1(h) + Ch(h, a, b) + K256[29] + W[29] + g; 
            c = c + e1(h) + Ch(h, a, b) + K256[29] + W[29] + e0(d) + Maj(d, e, f);
            f = b + e1(g) + Ch(g, h, a) + K256[30] + W[30] + f; 
            b = b + e1(g) + Ch(g, h, a) + K256[30] + W[30] + e0(c) + Maj(c, d, e);
            e = a + e1(f) + Ch(f, g, h) + K256[31] + W[31] + e; 
            a = a + e1(f) + Ch(f, g, h) + K256[31] + W[31] + e0(b) + Maj(b, c, d);

            d = h + e1(e) + Ch(e, f, g) + K256[32] + W[32] + d; 
            h = h + e1(e) + Ch(e, f, g) + K256[32] + W[32] + e0(a) + Maj(a, b, c);
            c = g + e1(d) + Ch(d, e, f) + K256[33] + W[33] + c; 
            g = g + e1(d) + Ch(d, e, f) + K256[33] + W[33] + e0(h) + Maj(h, a, b);
            b = f + e1(c) + Ch(c, d, e) + K256[34] + W[34] + b; 
            f = f + e1(c) + Ch(c, d, e) + K256[34] + W[34] + e0(g) + Maj(g, h, a);
            a = e + e1(b) + Ch(b, c, d) + K256[35] + W[35] + a; 
            e = e + e1(b) + Ch(b, c, d) + K256[35] + W[35] + e0(f) + Maj(f, g, h);
            h = d + e1(a) + Ch(a, b, c) + K256[36] + W[36] + h; 
            d = d + e1(a) + Ch(a, b, c) + K256[36] + W[36] + e0(e) + Maj(e, f, g);
            g = c + e1(h) + Ch(h, a, b) + K256[37] + W[37] + g; 
            c = c + e1(h) + Ch(h, a, b) + K256[37] + W[37] + e0(d) + Maj(d, e, f);
            f = b + e1(g) + Ch(g, h, a) + K256[38] + W[38] + f; 
            b = b + e1(g) + Ch(g, h, a) + K256[38] + W[38] + e0(c) + Maj(c, d, e);
            e = a + e1(f) + Ch(f, g, h) + K256[39] + W[39] + e; 
            a = a + e1(f) + Ch(f, g, h) + K256[39] + W[39] + e0(b) + Maj(b, c, d);

            d = h + e1(e) + Ch(e, f, g) + K256[40] + W[40] + d; 
            h = h + e1(e) + Ch(e, f, g) + K256[40] + W[40] + e0(a) + Maj(a, b, c);
            c = g + e1(d) + Ch(d, e, f) + K256[41] + W[41] + c; 
            g = g + e1(d) + Ch(d, e, f) + K256[41] + W[41] + e0(h) + Maj(h, a, b);
            b = f + e1(c) + Ch(c, d, e) + K256[42] + W[42] + b; 
            f = f + e1(c) + Ch(c, d, e) + K256[42] + W[42] + e0(g) + Maj(g, h, a);
            a = e + e1(b) + Ch(b, c, d) + K256[43] + W[43] + a; 
            e = e + e1(b) + Ch(b, c, d) + K256[43] + W[43] + e0(f) + Maj(f, g, h);
            h = d + e1(a) + Ch(a, b, c) + K256[44] + W[44] + h; 
            d = d + e1(a) + Ch(a, b, c) + K256[44] + W[44] + e0(e) + Maj(e, f, g);
            g = c + e1(h) + Ch(h, a, b) + K256[45] + W[45] + g; 
            c = c + e1(h) + Ch(h, a, b) + K256[45] + W[45] + e0(d) + Maj(d, e, f);
            f = b + e1(g) + Ch(g, h, a) + K256[46] + W[46] + f; 
            b = b + e1(g) + Ch(g, h, a) + K256[46] + W[46] + e0(c) + Maj(c, d, e);
            e = a + e1(f) + Ch(f, g, h) + K256[47] + W[47] + e; 
            a = a + e1(f) + Ch(f, g, h) + K256[47] + W[47] + e0(b) + Maj(b, c, d);
        
            d = h + e1(e) + Ch(e, f, g) + K256[48] + W[48] + d; 
            h = h + e1(e) + Ch(e, f, g) + K256[48] + W[48] + e0(a) + Maj(a, b, c);
            c = g + e1(d) + Ch(d, e, f) + K256[49] + W[49] + c; 
            g = g + e1(d) + Ch(d, e, f) + K256[49] + W[49] + e0(h) + Maj(h, a, b);
            b = f + e1(c) + Ch(c, d, e) + K256[50] + W[50] + b; 
            f = f + e1(c) + Ch(c, d, e) + K256[50] + W[50] + e0(g) + Maj(g, h, a);
            a = e + e1(b) + Ch(b, c, d) + K256[51] + W[51] + a; 
            e = e + e1(b) + Ch(b, c, d) + K256[51] + W[51] + e0(f) + Maj(f, g, h);
            h = d + e1(a) + Ch(a, b, c) + K256[52] + W[52] + h; 
            d = d + e1(a) + Ch(a, b, c) + K256[52] + W[52] + e0(e) + Maj(e, f, g);
            g = c + e1(h) + Ch(h, a, b) + K256[53] + W[53] + g; 
            c = c + e1(h) + Ch(h, a, b) + K256[53] + W[53] + e0(d) + Maj(d, e, f);
            f = b + e1(g) + Ch(g, h, a) + K256[54] + W[54] + f; 
            b = b + e1(g) + Ch(g, h, a) + K256[54] + W[54] + e0(c) + Maj(c, d, e);
            e = a + e1(f) + Ch(f, g, h) + K256[55] + W[55] + e; 
            a = a + e1(f) + Ch(f, g, h) + K256[55] + W[55] + e0(b) + Maj(b, c, d);

            d = h + e1(e) + Ch(e, f, g) + K256[56] + W[56] + d; 
            h = h + e1(e) + Ch(e, f, g) + K256[56] + W[56] + e0(a) + Maj(a, b, c);
            c = g + e1(d) + Ch(d, e, f) + K256[57] + W[57] + c; 
            g = g + e1(d) + Ch(d, e, f) + K256[57] + W[57] + e0(h) + Maj(h, a, b);

            b = b + f + e1(c) + Ch(c, d, e) + K256[58] + W[58]; 
            a = a + e + e1(b) + Ch(b, c, d) + K256[59] + W[59]; 
            h = h + d + e1(a) + Ch(a, b, c) + K256[60] + W[60]; 
            g = g + c + e1(h) + Ch(h, a, b) + K256[61] + W[61]; 

                 f       g       h
            00002620 01000000 00000000
            f <= 0x20260000;                
        
            0 = 0x5be0cd19 + h;
            0 = 0x1f83d9ab + g;
        
            0 - 0x5be0cd19 = h;
            0 - 0x1f83d9ab = g;
        
            0 - 0x1f83d9ab - 0xa4506ceb = g + c + e1(h) + Ch(h, a, b) + W[61];

            //W[i] = s1(W[i - 2]) + W[i - 7] + s0(W[i - 15]) + W[i - 16];
            W[0] = ??
            W[1] = ??
            W[2] = ??
            W[3] = ??
            W[4] = ??
            W[5] = ??
            W[6] = ??
            W[7] = ??
            W[8] = 0x80000000;
            W[9] = 0x00000000;
            W[10] = 0x00000000;
            W[11] = 0x00000000;
            W[12] = 0x00000000;
            W[13] = 0x00000000;
            W[14] = 0x00000000;
            W[15] = 0x00000100;
            W[16] = sigma0(W[1]) + W[0];
            W[17] = 0x00a00000 + sigma0(W[2]) + W[1];
            W[18] = sigma1(W[16]) + sigma0(W[3]) + W[2];
            W[19] = sigma1(W[17]) + sigma0(W[4]) + W[3];
            W[20] = sigma1(W[18]) + sigma0(W[5]) + W[4];
            W[21] = sigma1(W[19]) + sigma0(W[6]) + W[5];
            W[22] = sigma1(W[20]) + 0x00000100 + sigma0(W[7]) + W[6];
            W[23] = sigma1(W[21]) + W[16] + 0x11002000 + W[7];
            W[24] = sigma1(W[22]) + W[17] + 0x80000000;
            W[25] = sigma1(W[23]) + W[18];
            W[26] = sigma1(W[24]) + W[19];
            W[27] = sigma1(W[25]) + W[20];
            W[28] = sigma1(W[26]) + W[21];
            W[29] = sigma1(W[27]) + W[22];
            W[30] = sigma1(W[28]) + W[23] + 0x00400022;
            W[31] = sigma1(W[29]) + sigma0(W[16]) + W[24] + 0x00000100;
            W[32] = sigma1(W[30]) + sigma0(W[17]) + W[25] + W[16];
            W[33] = sigma1(W[31]) + sigma0(W[18]) + W[26] + W[17];
            W[34] = sigma1(W[32]) + sigma0(W[19]) + W[27] + W[18];
            W[35] = sigma1(W[33]) + sigma0(W[20]) + W[28] + W[19];
            W[36] = sigma1(W[34]) + sigma0(W[21]) + W[29] + W[20];
            W[37] = sigma1(W[35]) + sigma0(W[22]) + W[30] + W[21];
            W[38] = sigma1(W[36]) + sigma0(W[23]) + W[31] + W[22];
            W[39] = sigma1(W[37]) + sigma0(W[24]) + W[32] + W[23];
            W[40] = sigma1(W[38]) + sigma0(W[25]) + W[33] + W[24];
            W[41] = sigma1(W[39]) + sigma0(W[26]) + W[34] + W[25];
            W[42] = sigma1(W[40]) + sigma0(W[27]) + W[35] + W[26];
            W[43] = sigma1(W[41]) + sigma0(W[28]) + W[36] + W[27];
            W[44] = sigma1(W[42]) + sigma0(W[29]) + W[37] + W[28];
            W[45] = sigma1(W[43]) + sigma0(W[30]) + W[38] + W[29];
            W[46] = sigma1(W[44]) + sigma0(W[31]) + W[39] + W[30];
            W[47] = sigma1(W[45]) + sigma0(W[32]) + W[40] + W[31];
            W[48] = sigma1(W[46]) + sigma0(W[33]) + W[41] + W[32];
            W[49] = sigma1(W[47]) + sigma0(W[34]) + W[42] + W[33];
            W[50] = sigma1(W[48]) + sigma0(W[35]) + W[43] + W[34];
            W[51] = sigma1(W[49]) + sigma0(W[36]) + W[44] + W[35];
            W[52] = sigma1(W[50]) + sigma0(W[37]) + W[45] + W[36];
            W[53] = sigma1(W[51]) + sigma0(W[38]) + W[46] + W[37];
            W[54] = sigma1(W[52]) + sigma0(W[39]) + W[47] + W[38];
            W[55] = sigma1(W[53]) + sigma0(W[40]) + W[48] + W[39];
            W[57] = sigma1(W[55]) + sigma0(W[42]) + W[50] + W[41];
            W[59] = sigma1(W[57]) + sigma0(W[44]) + W[52] + W[43];
            W[61] = sigma1(W[59]) + sigma0(W[46]) + W[54] + W[45];
        
            W[0] = W[16] - sigma0(W[1]);
            W[16] = W[32] - W[25] - sigma0(W[17]) - sigma1(W[30]);
        
            
            

        }
  
        void Unhash(uint[] state, uint[] target, uint[] output)
        {
            uint a = target[0] - state[0];
            uint b = target[1] - state[1];
            uint c = target[2] - state[2];
            uint d = target[3] - state[3];
            uint e = target[4] - state[4];
            uint f = target[5] - state[5];
            uint g = target[6] - state[6];
            uint h = target[7] - state[7];

            uint[] W = new uint[64];
            W[0] = ?
            W[1] = ?
            W[2] = ?
            W[3] = ?
            W[4] = ?
            W[5] = ?
            W[6] = ?
            W[7] = ?
            W[8] = 0x80000000;
            W[9] = 0x00000000;
            W[10] = 0x00000000;
            W[11] = 0x00000000;
            W[12] = 0x00000000;
            W[13] = 0x00000000;
            W[14] = 0x00000000;
            W[15] = 0x00000100;
            W[16] = sigma0(W[1]) + W[0];
            W[17] = 0x00a00000 + sigma0(W[2]) + W[1];
            W[18] = sigma1(W[16]) + sigma0(W[3]) + W[2];
            //W[i] = sigma1(W[i - 2]) + W[i - 7] + sigma0(W[i - 15]) + W[i - 16];


            0 = (d + Sigma1(a) + Ch(a, b, c) + 0x90befffa + W[60]) 
              + (h + Sigma1(e) + Ch(e, f, g) + 0x748f82ee + W[56]) 
              + (Sigma0(a) + Maj(a, b, c))
              + state[7];
                
                uint[] staticHashInit = { 0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19 };
         
    d = 0x98C7E2A2 + W[0]; 
    h = 0xFC08884D + W[0];

    c = 0xCD2A11AE + Sigma1(d) + Ch(d, 0x510e527f, 0x9b05688c) + W[1]; 
    g = 0x90BB1E3C + Sigma1(d) + Ch(d, 0x510e527f, 0x9b05688c) + W[1] + Sigma0(h) + Maj(h, 0x6a09e667, 0xbb67ae85);
    
    b = 0x0C2E12E0 + Sigma1(c) + Ch(c, d, 0x510e527f) + W[2]; 
    f = 0x50C6645B + Sigma1(c) + Ch(c, d, 0x510e527f) + W[2] + Sigma0(g) + Maj(g, 0x5be0cd19, 0x6a09e667);

    a = 0xA4CE148B + Sigma1(b) + Ch(b, c, d) + W[3]; 
    e = 0x3AC42E24 + Sigma1(b) + Ch(b, c, d) + W[3] + Sigma0(f) + Maj(f, g, h);

    h = h + d + Sigma1(a) + Ch(a, b, c) + 0x3956c25b + W[4]; 
    d = d + Sigma1(a) + Ch(a, b, c) + 0x3956c25b + W[4] + Sigma0(e) + Maj(e, f, g);

                
                d = 0x98C7E2A2 + W[0]
                h = 0xFC08884D + W[0]
                c = 0xCD2A11AE + Sigma1(d) + Ch(d, 0x510e527f, 0x9b05688c) + W[1]
                g = 0x90BB1E3C + Sigma1(d) + Ch(d, e, f) + W[1] + Sigma0(h) + Maj(h, a, b)
                b = 0x0C2E12E0 + Sigma1(c) + Ch(c, d, 0x510e527f) + W[2]   
                f = 0x50C6645B + Sigma1(c) + Ch(c, d, e) + W[2] + Sigma0(g) + Maj(g, h, a)
                a = 0xA4CE148B + Sigma1(b) + Ch(b, c, d) + W[3];
                e = 0x3AC42E24 + Sigma1(b) + Ch(b, c, d) + W[3] + Sigma0(f) + Maj(f, g, h)

                d6 = 0x859FF829 + W[0] + W[0] + W[4] + Sigma1(a) + Ch(a, b, c) + Sigma0(e) + Maj(e, f, g) + Sigma1(e) + Ch(e, f, g)

                h5 = 0xFC08884D + W[0] + Sigma1(e) + Ch(e, f, g) + 0xd807aa98 + W[8] + Sigma0(a) + Maj(a, b, c);
                d5 = d6 + Sigma1(a) + Ch(a, b, c) + 0x72be5d74 + W[12]
                    + Sigma0(e) + Maj(e, f, g)
                    + h + Sigma1(e) + Ch(e, f, g) + 0xe49b69c1 + W[16]

                d4 = d5 + Sigma1(a) + Ch(a, b, c) + 0x2de92c6f + W[20]
                    + Sigma0(e) + Maj(e, f, g)
                    + h + Sigma1(e) + Ch(e, f, g) + 0x983e5152 + W[24]

                d3 = d4 + Sigma1(a) + Ch(a, b, c) + 0xc6e00bf3 + W[28]
                    + Sigma0(e) + Maj(e, f, g)
                    + h + Sigma1(e) + Ch(e, f, g) + 0x27b70a85 + W[32]

                d2 = d3 + Sigma1(a) + Ch(a, b, c) + 0x650a7354 + W[36]
                    Sigma0(e) + Maj(e, f, g)
                    + h + Sigma1(e) + Ch(e, f, g) + 0xa2bfe8a1 + W[40]

                d1 = d2 + Sigma1(a) + Ch(a, b, c) + 0xd192e819 + W[44]
                    + Sigma0(e) + Maj(e, f, g)
                    + h + Sigma1(e) + Ch(e, f, g) + 0x19a4c116 + W[48]

                d0 = d1 + Sigma1(a) + Ch(a, b, c) + 0x391c0cb3 + W[52]
                  + Sigma0(e) + Maj(e, f, g)
                  + h + Sigma1(e) + Ch(e, f, g) + 0x748f82ee + W[56]

            0 - state[7] - 0x90befffa - 0x748f82ee = (d0 + Sigma1(a) + Ch(a, b, c) + W[60]) 
                                                    + (h + Sigma1(e) + Ch(e, f, g) + W[56]) 
                                                    + (Sigma0(a) + Maj(a, b, c));

            
            
            //uint targetH = 0xA41F32E7;
            //targetH -= 0x90befffa;      // 136032ED


            
            //X[12] += sigma0(X[13]) + sigma1(X[10]) + X[5];
            //h = d + X[12] + h + Sigma1(e) + Ch(e, f, g) + 0x90befffa;

            //0 = (state[7] + h);
        }
        */
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinerLib_cs
{
    public class Scrypt
    {
        Work work;

        uint[] pad36;
        uint[] pad5c;
        uint[] dataBuffer;
        uint[] dataBuffer2;
        uint[] tempHash;
        uint[] output;
        uint outputMask;

        public Scrypt(Work w)
        {
            work = w;

            pad36 = new uint[16];
            pad5c = new uint[16];
            dataBuffer = new uint[16];
            dataBuffer2 = new uint[16];
            tempHash = new uint[16];
            output = new uint[8];
                        
            for (int i = 8; i < 16; i++)
            {
                pad36[i] = 0x36363636;
                pad5c[i] = 0x5c5c5c5c;
            }
            dataBuffer[5] = 0x80000000;
            dataBuffer[15] = 0x000004A0;
            dataBuffer2[0] = 0x00000001;
            dataBuffer2[1] = 0x80000000;
            dataBuffer2[15] = 0x00000620;
            tempHash[8] = 0x80000000;
            tempHash[15] = 0x00000300;

            dataBuffer[0] = work.data[16];
            dataBuffer[1] = work.data[17];
            dataBuffer[2] = work.data[18];

            uint difficulty = work.data[18];
            uint diffZeros = 32 - (difficulty & 0xFF);
            switch (diffZeros)
            {
                case 0:
                    outputMask = 0;
                    break;
                case 1:
                    outputMask = 0xFF000000;
                    break;
                case 2:
                    outputMask = 0xFFFF0000;
                    break;
                case 3:
                    outputMask = 0x00FFFFFF;
                    break;
                default:
                    outputMask = 0xFFFFFFFF;
                    break;
            }
        }

        public bool Hash(uint nonce)
        {
            dataBuffer[3] = work.data[19] = nonce;

            ScryptHash();
            
            if( (output[7] & outputMask) == 0 )
            {
                for( int i = 7; i > 0; i-- )
                {
                    uint outputVal = ByteReverse(output[i]);
                    if( outputVal > work.target[i] )
                        break;
                    if( outputVal < work.target[i] )
                    {
                        // Found solution
                        return true;
                    }
                }
            }
            return false;
        }

        static uint[] K256 = { 0x428a2f98,0x71374491,0xb5c0fbcf,0xe9b5dba5, 0x3956c25b,0x59f111f1,0x923f82a4,0xab1c5ed5, 0xd807aa98,0x12835b01,0x243185be,0x550c7dc3, 0x72be5d74,0x80deb1fe,0x9bdc06a7,0xc19bf174, 0xe49b69c1,0xefbe4786,0x0fc19dc6,0x240ca1cc, 0x2de92c6f,0x4a7484aa,0x5cb0a9dc,0x76f988da, 0x983e5152,0xa831c66d,0xb00327c8,0xbf597fc7, 0xc6e00bf3,0xd5a79147,0x06ca6351,0x14292967, 0x27b70a85,0x2e1b2138,0x4d2c6dfc,0x53380d13, 0x650a7354,0x766a0abb,0x81c2c92e,0x92722c85, 0xa2bfe8a1,0xa81a664b,0xc24b8b70,0xc76c51a3, 0xd192e819,0xd6990624,0xf40e3585,0x106aa070, 0x19a4c116,0x1e376c08,0x2748774c,0x34b0bcb5, 0x391c0cb3,0x4ed8aa4a,0x5b9cca4f,0x682e6ff3, 0x748f82ee,0x78a5636f,0x84c87814,0x8cc70208, 0x90befffa,0xa4506ceb,0xbef9a3f7,0xc67178f2 };
        static uint[] staticHash = {0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19};


        void ScryptHash()
        {
            uint[] inner = new uint[8];
            uint[] outer = new uint[8];
            sha256_block(inner, staticHash, work.data);
            sha256_block(inner, inner, work.data, 16);
    	
            for( int i = 0; i < 8; i++ )
            {
                pad36[i] = inner[i] ^ 0x36363636;
                pad5c[i] = inner[i] ^ 0x5c5c5c5c;
            }
            sha256_block(inner, staticHash, pad36);
            sha256_block(outer, staticHash, pad5c);

            uint[] salted = new uint[8];
            sha256_block(salted, inner, work.data);

            uint[] temp = new uint[8];
            uint[] bp = new uint[32];
            for( uint i = 0; i < 4; i++ )
            {
                dataBuffer[4] = (i + 1);

                sha256_block(tempHash, salted, dataBuffer);
                sha256_block(temp, outer, tempHash);
                Array.Copy(temp, 0, bp, i * 8, 8);
            }
	
            uint[] X = new uint[32];
            uint[] V = new uint[1024 * 32];
	        for (int i = 0; i < 32; i++)
                X[i] = ByteReverse(bp[i]);

	        for (int i = 0; i < 1024; i++) 
            {
                Array.Copy(X, 0, V, i * 32, 32);
		        xor_salsa(X);
	        }
	        for (int i = 0; i < 1024; i++) 
            {
		        uint j = 32 * (X[16] & 1023);
		        for (uint k = 0; k < 32; k++)
			        X[k] ^= V[j + k];
		        xor_salsa(X);
	        }

            for (int i = 0; i < 32; i++)
                bp[i] = ByteReverse(X[i]);
    
            sha256_block(salted, inner, bp);
            sha256_block(salted, salted, bp, 16);

            sha256_block(tempHash, salted, dataBuffer2);
            sha256_block(output, outer, tempHash);
        }

        void sha256_block(uint[] output, uint[] state, uint[] input, uint inputOffset = 0)
        {
	        uint a,b,c,d,e,f,g,h,s0,s1,T1,T2;
	        uint[] X = new uint[16];
	        int i;
	
	        a = state[0];	b = state[1];	c = state[2];	d = state[3];
	        e = state[4];	f = state[5];	g = state[6];	h = state[7];

	        for (i=0;i<16;i++)
	        {
                T1 = X[i] = input[i + inputOffset];
	            T1 += h;
                T1 += Sigma1(e);
                T1 += Ch(e,f,g);
                T1 += K256[i];
	            T2 = Sigma0(a) + Maj(a,b,c);
	            h = g;	g = f;	f = e;	e = d + T1;
	            d = c;	c = b;	b = a;	a = T1 + T2;
	        }

	        for (;i<64;i++)
	        {
	            s0 = X[(i+1)&0x0f];	s0 = sigma0(s0);
	            s1 = X[(i+14)&0x0f];	s1 = sigma1(s1);

	            T1 = X[i&0xf] += s0 + s1 + X[(i+9)&0xf];
	            T1 += h + Sigma1(e) + Ch(e,f,g) + K256[i];
	            T2 = Sigma0(a) + Maj(a,b,c);
	            h = g;	g = f;	f = e;	e = d + T1;
	            d = c;	c = b;	b = a;	a = T1 + T2;
	        }
    
            output[0] = state[0] + a;
            output[1] = state[1] + b;
            output[2] = state[2] + c;
            output[3] = state[3] + d;
            output[4] = state[4] + e;
            output[5] = state[5] + f;
            output[6] = state[6] + g;
            output[7] = state[7] + h;
        }

        uint ROTATE(uint a, int n)
        {
            uint val = (((a)<<(n))|(((a)&0xffffffff)>>(32-(n))));
            return val;
        }

        uint Sigma0(uint x)
        {
            uint val = (ROTATE((x),30) ^ ROTATE((x),19) ^ ROTATE((x),10));
            return val;
        }

        uint Sigma1(uint x)
        {
            uint val = (ROTATE((x),26) ^ ROTATE((x),21) ^ ROTATE((x),7));
            return val;
        }

        uint sigma0(uint x)
        {
            uint val = (ROTATE((x),25) ^ ROTATE((x),14) ^ ((x)>>3));
            return val;
        }

        uint sigma1(uint x)
        {
            uint val = (ROTATE((x),15) ^ ROTATE((x),13) ^ ((x)>>10));
            return val;
        }

        uint Ch(uint x,uint y,uint z)
        {
            uint val = (((x) & (y)) ^ ((~(x)) & (z)));
            return val;
        }

        uint Maj(uint x, uint y, uint z)
        {
            uint val = (((x) & (y)) ^ ((x) & (z)) ^ ((y) & (z)));
            return val;
        }

        public static uint ByteReverse(uint value)
        {
            value = ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8);
            return (value<<16) | (value>>16);
        }

        void xor_salsa(uint[] B)
        {
	        uint x00,x01,x02,x03,x04,x05,x06,x07,x08,x09,x10,x11,x12,x13,x14,x15;
	        int i;

	        x00 = (B[ 0] ^= B[16]);
	        x01 = (B[ 1] ^= B[17]);
	        x02 = (B[ 2] ^= B[18]);
	        x03 = (B[ 3] ^= B[19]);
	        x04 = (B[ 4] ^= B[20]);
	        x05 = (B[ 5] ^= B[21]);
	        x06 = (B[ 6] ^= B[22]);
	        x07 = (B[ 7] ^= B[23]);
	        x08 = (B[ 8] ^= B[24]);
	        x09 = (B[ 9] ^= B[25]);
	        x10 = (B[10] ^= B[26]);
	        x11 = (B[11] ^= B[27]);
	        x12 = (B[12] ^= B[28]);
	        x13 = (B[13] ^= B[29]);
	        x14 = (B[14] ^= B[30]);
	        x15 = (B[15] ^= B[31]);
	        for (i = 0; i < 8; i += 2) {
		        // Operate on columns
		        x04 ^= ROTATE(x00 + x12,  7);  x09 ^= ROTATE(x05 + x01,  7);
		        x14 ^= ROTATE(x10 + x06,  7);  x03 ^= ROTATE(x15 + x11,  7);

		        x08 ^= ROTATE(x04 + x00,  9);  x13 ^= ROTATE(x09 + x05,  9);
		        x02 ^= ROTATE(x14 + x10,  9);  x07 ^= ROTATE(x03 + x15,  9);

		        x12 ^= ROTATE(x08 + x04, 13);  x01 ^= ROTATE(x13 + x09, 13);
		        x06 ^= ROTATE(x02 + x14, 13);  x11 ^= ROTATE(x07 + x03, 13);

		        x00 ^= ROTATE(x12 + x08, 18);  x05 ^= ROTATE(x01 + x13, 18);
		        x10 ^= ROTATE(x06 + x02, 18);  x15 ^= ROTATE(x11 + x07, 18);

		        // Operate on rows
		        x01 ^= ROTATE(x00 + x03,  7);  x06 ^= ROTATE(x05 + x04,  7);
		        x11 ^= ROTATE(x10 + x09,  7);  x12 ^= ROTATE(x15 + x14,  7);

		        x02 ^= ROTATE(x01 + x00,  9);  x07 ^= ROTATE(x06 + x05,  9);
		        x08 ^= ROTATE(x11 + x10,  9);  x13 ^= ROTATE(x12 + x15,  9);

		        x03 ^= ROTATE(x02 + x01, 13);  x04 ^= ROTATE(x07 + x06, 13);
		        x09 ^= ROTATE(x08 + x11, 13);  x14 ^= ROTATE(x13 + x12, 13);

		        x00 ^= ROTATE(x03 + x02, 18);  x05 ^= ROTATE(x04 + x07, 18);
		        x10 ^= ROTATE(x09 + x08, 18);  x15 ^= ROTATE(x14 + x13, 18);
	        }
	        B[ 0] += x00;
	        B[ 1] += x01;
	        B[ 2] += x02;
	        B[ 3] += x03;
	        B[ 4] += x04;
	        B[ 5] += x05;
	        B[ 6] += x06;
	        B[ 7] += x07;
	        B[ 8] += x08;
	        B[ 9] += x09;
	        B[10] += x10;
	        B[11] += x11;
	        B[12] += x12;
	        B[13] += x13;
	        B[14] += x14;
	        B[15] += x15;


            x00 = (B[16] ^= B[00]);
            x01 = (B[17] ^= B[01]);
            x02 = (B[18] ^= B[02]);
            x03 = (B[19] ^= B[03]);
            x04 = (B[20] ^= B[04]);
            x05 = (B[21] ^= B[05]);
            x06 = (B[22] ^= B[06]);
            x07 = (B[23] ^= B[07]);
            x08 = (B[24] ^= B[08]);
            x09 = (B[25] ^= B[09]);
            x10 = (B[26] ^= B[10]);
            x11 = (B[27] ^= B[11]);
            x12 = (B[28] ^= B[12]);
            x13 = (B[29] ^= B[13]);
            x14 = (B[30] ^= B[14]);
            x15 = (B[31] ^= B[15]);
            for (i = 0; i < 8; i += 2)
            {
                // Operate on columns
                x04 ^= ROTATE(x00 + x12, 7); x09 ^= ROTATE(x05 + x01, 7);
                x14 ^= ROTATE(x10 + x06, 7); x03 ^= ROTATE(x15 + x11, 7);

                x08 ^= ROTATE(x04 + x00, 9); x13 ^= ROTATE(x09 + x05, 9);
                x02 ^= ROTATE(x14 + x10, 9); x07 ^= ROTATE(x03 + x15, 9);

                x12 ^= ROTATE(x08 + x04, 13); x01 ^= ROTATE(x13 + x09, 13);
                x06 ^= ROTATE(x02 + x14, 13); x11 ^= ROTATE(x07 + x03, 13);

                x00 ^= ROTATE(x12 + x08, 18); x05 ^= ROTATE(x01 + x13, 18);
                x10 ^= ROTATE(x06 + x02, 18); x15 ^= ROTATE(x11 + x07, 18);

                // Operate on rows
                x01 ^= ROTATE(x00 + x03, 7); x06 ^= ROTATE(x05 + x04, 7);
                x11 ^= ROTATE(x10 + x09, 7); x12 ^= ROTATE(x15 + x14, 7);

                x02 ^= ROTATE(x01 + x00, 9); x07 ^= ROTATE(x06 + x05, 9);
                x08 ^= ROTATE(x11 + x10, 9); x13 ^= ROTATE(x12 + x15, 9);

                x03 ^= ROTATE(x02 + x01, 13); x04 ^= ROTATE(x07 + x06, 13);
                x09 ^= ROTATE(x08 + x11, 13); x14 ^= ROTATE(x13 + x12, 13);

                x00 ^= ROTATE(x03 + x02, 18); x05 ^= ROTATE(x04 + x07, 18);
                x10 ^= ROTATE(x09 + x08, 18); x15 ^= ROTATE(x14 + x13, 18);
            }
            B[16] += x00;
            B[17] += x01;
            B[18] += x02;
            B[19] += x03;
            B[20] += x04;
            B[21] += x05;
            B[22] += x06;
            B[23] += x07;
            B[24] += x08;
            B[25] += x09;
            B[26] += x10;
            B[27] += x11;
            B[28] += x12;
            B[29] += x13;
            B[30] += x14;
            B[31] += x15;
        }
    }
}

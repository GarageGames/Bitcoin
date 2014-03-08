using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace BitCoinClient
{
    public class Worker
    {
        TcpClient mClient;
        Thread mIOThread;
        Thread mWorkThread;
        
        bool mDoWork;
        uint mHashStart;
        uint mHashCount;
        uint mHashesDone;
        uint[] mMidstate;
        uint[] mData;
        uint[] mTarget;

        public double mHashrate;

        const uint cHashChunk = 200000;
        
        public Worker()
        {
            mDoWork = false;
            mHashCount = 0;
            mHashesDone = 0;
            mMidstate = new uint[8];
            mData = new uint[16];
            mTarget = new uint[8];

            mClient = new TcpClient();
            mClient.Connect("192.168.0.144", 555);
            SendIdentity();

            mIOThread = new Thread(new ThreadStart(IOThread));
            mWorkThread = new Thread(new ThreadStart(WorkThread));
                        
            mIOThread.Start();
            mWorkThread.Start();
        }

        public void Close()
        {
            mIOThread.Abort();
            mWorkThread.Abort();

            mIOThread.Join();
            mWorkThread.Join();

            mClient.Close();
        }

        void SendIdentity()
        {
            BinaryWriter bw = new BinaryWriter(mClient.GetStream());
            bw.Write((byte)1);  // Identity Packet
            bw.Write((byte)1);  // CSharp Client
            bw.Write((uint)1000000); // desired hashes
            //Console.WriteLine("Sent Identity Packet");
        }

        void SendWorkComplete(bool solutionFound, uint solution, uint hashesDone)
        {
            BinaryWriter bw = new BinaryWriter(mClient.GetStream());
            bw.Write((byte)2);  // Work Complete Packet
            bw.Write((byte)(solutionFound ? 1 : 0));
            bw.Write(solution);
            bw.Write(hashesDone);
            mDoWork = false;
            //Console.WriteLine("Sent Work Complete: " + solutionFound + "(" + solution + ") - " + hashesDone);
        }

        void ProcessWorkCommand(NetworkStream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            mHashStart = br.ReadUInt32();
            mHashCount = br.ReadUInt32();
            byte[] midstate = br.ReadBytes(32);
            byte[] data = br.ReadBytes(64);
            byte[] target = br.ReadBytes(32);

            Buffer.BlockCopy(midstate, 0, mMidstate, 0, 32);
            Buffer.BlockCopy(data, 0, mData, 0, 64);
            Buffer.BlockCopy(target, 0, mTarget, 0, 32);

            mHashesDone = 0;
            mDoWork = true;

            //Console.WriteLine("Work Command recieved: " + mHashStart + " - " + (mHashStart + mHashCount));
        }

        void ProcessStopCommand()
        {
            mDoWork = false;
            //Console.WriteLine("Stop Work Command recieved");
        }

        void IOThread()
        {
            while (true)
            {
                NetworkStream stream = mClient.GetStream();

                int command = stream.ReadByte();
                switch (command)
                {
                    case 3:
                        ProcessWorkCommand(stream);
                        break;
                    case 4:
                        ProcessStopCommand();
                        break;
                    case 5:
                        stream.WriteByte((byte)5);  // Ping
                        break;
                    default:
                        byte[] temp = new byte[stream.Length];
                        stream.Read(temp, 0, (int)stream.Length);
                        break;
                }
            }
        }

        void SHATransform(uint[] output, uint[] init, uint[] data)
        {
            OpenSSL.Core.SHA256 sha = new OpenSSL.Core.SHA256();

            byte[] swapped = new byte[data.Length * 4];            
            for (int i = 0; i < data.Length; i++)
            {
                int byidx = i * 4;
                swapped[byidx + 0] = (byte)((data[i] & 0xFF000000) >> 24);
                swapped[byidx + 1] = (byte)((data[i] & 0x00FF0000) >> 16);
                swapped[byidx + 2] = (byte)((data[i] & 0x0000FF00) >> 8);
                swapped[byidx + 3] = (byte)((data[i] & 0x000000FF) );
            }

            Array.Copy(init, sha.ctx.h, 8);
            sha.Update(swapped);
            Buffer.BlockCopy(sha.ctx.h, 0, output, 0, 32);
        }

        #region SHA
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

        uint Maj(uint x,uint y,uint z)
        {
	        uint val = (((x) & (y)) ^ ((x) & (z)) ^ ((y) & (z)));
	        return val;
        }

        uint[] K256 = { 0x428a2f98,0x71374491,0xb5c0fbcf,0xe9b5dba5, 0x3956c25b,0x59f111f1,0x923f82a4,0xab1c5ed5, 0xd807aa98,0x12835b01,0x243185be,0x550c7dc3, 0x72be5d74,0x80deb1fe,0x9bdc06a7,0xc19bf174, 0xe49b69c1,0xefbe4786,0x0fc19dc6,0x240ca1cc, 0x2de92c6f,0x4a7484aa,0x5cb0a9dc,0x76f988da, 0x983e5152,0xa831c66d,0xb00327c8,0xbf597fc7, 0xc6e00bf3,0xd5a79147,0x06ca6351,0x14292967, 0x27b70a85,0x2e1b2138,0x4d2c6dfc,0x53380d13, 0x650a7354,0x766a0abb,0x81c2c92e,0x92722c85, 0xa2bfe8a1,0xa81a664b,0xc24b8b70,0xc76c51a3, 0xd192e819,0xd6990624,0xf40e3585,0x106aa070, 0x19a4c116,0x1e376c08,0x2748774c,0x34b0bcb5, 0x391c0cb3,0x4ed8aa4a,0x5b9cca4f,0x682e6ff3, 0x748f82ee,0x78a5636f,0x84c87814,0x8cc70208, 0x90befffa,0xa4506ceb,0xbef9a3f7,0xc67178f2 };

        void sha256_block_data_order(uint[] output, uint[] state, uint[] input)
        {
	        uint[] X = new uint[16];
	        int i;
		
	        uint a = state[0];
	        uint b = state[1];		
	        uint c = state[2];
	        uint d = state[3];
	        uint e = state[4];
	        uint f = state[5];
	        uint g = state[6];
	        uint h = state[7];

	        for (i=0;i<16;i++)
	        { 
		        uint T1 = X[i] = input[i];
		        T1 += h + Sigma1(e) + Ch(e, f, g) + K256[i];
		        uint T2 = Sigma0(a) + Maj(a, b, c);
		        h = g; g = f; f = e; e = d + T1;
		        d = c; c = b; b = a; a = T1 + T2;
	        }

	        for (;i<64;i++)
	        {
	            uint s0 = X[(i+1)&0x0f];	
		        s0 = sigma0(s0);
	            uint s1 = X[(i+14)&0x0f];	
		        s1 = sigma1(s1);

		        X[i & 0xf] += s0 + s1 + X[(i + 9) & 0xf];
		        uint T1 = X[i & 0xf];
		        T1 += h + Sigma1(e) + Ch(e, f, g) + K256[i];

		        uint T2 = Sigma0(a) + Maj(a, b, c);
		        h = g; g = f; f = e; e = d + T1;
		        d = c; c = b; b = a; a = T1 + T2;
	        }

	        output[0] = (state[0] + a);
	        output[1] = (state[1] + b);
	        output[2] = (state[2] + c);
	        output[3] = (state[3] + d);
	        output[4] = (state[4] + e);
	        output[5] = (state[5] + f);
	        output[6] = (state[6] + g);
	        output[7] = (state[7] + h);
        }
        #endregion

        uint[] staticHashInit = { 0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19 };
        uint[] hash1 = { 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000100 };
        uint DoHashes(uint start, uint count)
        {
            uint[] hashout = new uint[8];
            uint end = start + count;
            uint hashesDone = 0;
            for (uint i = start; i < end; i++, hashesDone++)
            {
                // Write i into data
                //i = 2083236893;
                mData[3] = Program.Byteswap(i);

                //sha256_block_data_order(hash1, mMidstate, mData);
                //sha256_block_data_order(hashout, staticHashInit, hash1);
                SHATransform(hash1, mMidstate, mData);
                SHATransform(hashout, staticHashInit, hash1);
                
                // Check
                if (hashout[7] == 0)
                {
                    for (int k = 6; k >= 0; k--)
                    {
                        if (hashout[k] > mTarget[k])
                            break;
                        if (hashout[k] < mTarget[k])
                        {
                            // Solution found
                            hashesDone++;
                            SendWorkComplete(true, i, mHashesDone + hashesDone);
                            return hashesDone;
                        }
                    }
                }                
            }

            return hashesDone;
        }

        void WorkThread()
        {
            long startTime, endTime, freq;
            Win32Wrapper.QueryPerformanceFrequency(out freq);
            double dfFreq = 1.0 / (double)freq;
            while (true)
            {                
                if (mDoWork)
                {
                    uint hashesRemaining = mHashCount - mHashesDone;
                    uint numHashes = Math.Min(hashesRemaining, cHashChunk);

                    uint start = mHashStart + mHashesDone;

                    Win32Wrapper.QueryPerformanceCounter(out startTime);
                    uint hashesDone = DoHashes(start, numHashes);
                    Win32Wrapper.QueryPerformanceCounter(out endTime);
                    double seconds = (double)(endTime - startTime) * dfFreq;
                    mHashrate = (double)hashesDone / seconds;

                    mHashesDone += hashesDone;
                    if (mHashesDone >= mHashCount)
                    {
                        // Done, didnt find a solution
                        SendWorkComplete(false, 0, mHashesDone);
                    }
                }
            }
        }
    }
}

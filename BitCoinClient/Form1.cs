using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using Bitnet.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace BitCoinClient
{
    public partial class Form1 : Form
    {
        Bitcoin mBC;
        Worker[] mWorkers;
        

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //mBC = new Bitcoin();

            //mBC.AddNode("GGENG021", 8333);
            //mBC.AddNode("bitseed.xf2.org", 8333);
            //mBC.AddNode("dnsseed.bluematt.me", 8333);
            //mBC.AddNode("seed.bitcoin.sipa.be", 8333);
            //mBC.AddNode("dnsseed.bitcoin.dashjr.org", 8333);

            //TryToMine();

            int workerCount = Environment.ProcessorCount - 2;
            if (workerCount <= 0)
                workerCount = 1;
            mWorkers = new Worker[workerCount];
            for (int i = 0; i < workerCount; i++)
            {
                mWorkers[i] = new Worker();
                Thread.Sleep(500);
            }

            //mWorkers = new Worker[1];
            //mWorkers[0] = new Worker();
            timer1.Enabled = true;
            lblThreads.Text = "Threads: " + mWorkers.Length;
        }

        void TryToMine()
        {
            // Get work from bitcoind
            //WorkBlock work = GetWork();

            // Do the work
            WorkBlock work = new WorkBlock("{\"result\":{\"midstate\":\"22d199f12b6740fe0e4eb1275ba82bc7b14986d10e6eb0713169af562fb0ed90\",\"data\":\"00000002b99861098395d933f3a8ec454810dc49c8129f21e94f9ccb0000000000000000d879e26cdf479fbf18aa9c39ac6a87e2b14040093fbbbfde1f779577bed7d7ec530d2a2319015f5300000000000000800000000000000000000000000000000000000000000000000000000000000000000000000000000080020000\",\"hash1\":\"00000000000000000000000000000000000000000000000000000000000000000000008000000000000000000000000000000000000000000000000000010000\",\"target\":\"00000000000000000000000000000000000000000000535f0100000000000000\"},\"error\":null,\"id\":0}");
            DoWork(work);

            // Submit the work
            //SubmitWork(work);
        }

        WorkBlock GetWork()
        {
            /*
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:8332/");
            req.TransferEncoding = "";
            req.ServicePoint.UseNagleAlgorithm = false;
            req.Method = "POST";
            req.ContentType = "application/json";
            req.Expect = "";
            req.Credentials = new NetworkCredential("rpcuser", "rpcpass");

            StreamWriter sw = new StreamWriter(req.GetRequestStream());
            sw.Write("{\"method\": \"getwork\", \"params\": [], \"id\":0}\r\n");
            sw.Close();

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.Default);

            string str = sr.ReadToEnd();
            Console.WriteLine(str);
            WorkBlock work = new WorkBlock(str);
            
            sr.Close();
            */

            BitnetClient bc = new BitnetClient("http://127.0.0.1:8332");
            bc.Credentials = new NetworkCredential("rpcuser", "rpcpass");
            JObject obj = bc.GetWork();
            
            WorkBlock work = new WorkBlock(obj);
            return work;
        }

        void SubmitWork(WorkBlock work)
        {
            //string dataStr = HashToString(work.data)

            BitnetClient bc = new BitnetClient("http://127.0.0.1:8332");
            bc.Credentials = new NetworkCredential("rpcuser", "rpcpass");

            //bc.

        }

        void WriteUIntToByteArray(uint n, byte[] array, int index)
        {
            array[index + 3] = (byte)((n & 0xFF000000) >> 24);
            array[index + 2] = (byte)((n & 0x00FF0000) >> 16);
            array[index + 1] = (byte)((n & 0x0000FF00) >> 8);
            array[index + 0] = (byte)(n & 0x000000FF);
        }

        uint[] ByteArrayToUIntArray(byte[] input, int offset)
        {
            int bytes = input.Length - offset;
            int ints = bytes / 4;
            uint[] output = new uint[ints];
            int byidx = offset;
            for (int i = 0; i < ints; i++)
            {
                uint n = (uint)(((int)input[byidx + 3] << 24) | ((int)input[byidx + 2] << 16) | ((int)input[byidx + 1] << 8) | ((int)input[byidx + 0]));
                output[i] = n;
                byidx += 4;
            }

            return output;
        }

        void SHATransform(uint[] output, byte[] data, uint[] init)
        {
            OpenSSL.Core.SHA256 sha = new OpenSSL.Core.SHA256();

            byte[] swapped = new byte[data.Length];
            int count = data.Length / 4;
            for (int i = 0; i < count; i++)
            {
                int byidx = i * 4;
                swapped[byidx + 0] = data[byidx + 3];
                swapped[byidx + 1] = data[byidx + 2];
                swapped[byidx + 2] = data[byidx + 1];
                swapped[byidx + 3] = data[byidx + 0];
            }

            Array.Copy(init, sha.ctx.h, 8);
            sha.Update(swapped);
            Array.Copy(sha.ctx.h, output, 8);
        }

        uint[] staticData = { 0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19 };
        void DoWork(WorkBlock work)
        {
            byte[] data = new byte[64];
            Array.Copy(work.data, 64, data, 0, 64);
            uint[] midstate = ByteArrayToUIntArray(work.midstate, 0);

            uint[] targetA = ByteArrayToUIntArray(work.target, 0);
            uint[] targetSwapped = new uint[8];
            for( int i = 0; i < 8; i++ )
                targetSwapped[i] = targetA[7 - i];

            uint max = 0xFFFFFFFF;
            long start, end, freq;
            Win32Wrapper.QueryPerformanceFrequency(out freq);
            Win32Wrapper.QueryPerformanceCounter(out start);
            for(uint n = 0; n < max; n++)
            {
                WriteUIntToByteArray(n, data, 12);

                uint[] output = new uint[8];
                SHATransform(output, data, midstate);

                byte[] input = new byte[64];
                Buffer.BlockCopy(output, 0, input, 0, 32);
                Buffer.BlockCopy(work.hash1, 32, input, 32, 32);
                SHATransform(output, input, staticData);

                if (output[7] == 0)
                {
                    uint[] swapped = new uint[8];
                    for (int i = 0; i < 8; i++)
                    {
                        swapped[i] = Program.Byteswap(output[7 - i]);
                    }

                    for (int i = 1; i < 8; i++)
                    {
                        if (swapped[i] < targetSwapped[i])
                        {
                            // Found a solution
                            WriteUIntToByteArray(n, work.data, 76);     // Write the nonce value into the data block
                            return;
                        }

                        if (swapped[i] > targetSwapped[i])
                            break;                        
                    }
                }
            }

            Win32Wrapper.QueryPerformanceCounter(out end);
            long clocks = end - start;
            double seconds = (double)clocks / (double)freq;
            double hashesPerSecond = max / seconds;
            Console.WriteLine(hashesPerSecond.ToString() + " hashes per second");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Worker w in mWorkers)
            {
                w.Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            double hashrate = 0;
            foreach (Worker w in mWorkers)
                hashrate += w.mHashrate;

            lblHashrate.Text = String.Format("Hashrate: {0:N} / second", hashrate);
        }
    }
}

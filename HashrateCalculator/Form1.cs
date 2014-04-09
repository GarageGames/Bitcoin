using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace HashrateCalculator
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

            LoadCurrencies();
            CurrencyBox.SelectedIndex = 0;
        }

        private void LoadCurrencies()
        {
            string[] files = Directory.GetFiles("Currencies/");
            
            foreach( string filename in files)
            {
                FileStream f = File.OpenRead(filename);
                StreamReader sr = new StreamReader(f);
                Currency c = new Currency();
                c.mName = filename.Substring(11);
                c.mIPAddress = sr.ReadLine();
                c.mPort = Convert.ToUInt16(sr.ReadLine());
                c.mNetVersion = Convert.ToUInt32(sr.ReadLine());
                c.mNetID = Convert.ToUInt32(sr.ReadLine());
                c.mBlockTarget = Convert.ToUInt32(sr.ReadLine());
                
                string hash = sr.ReadLine();
                c.mLastHeaderHash = Program.HexStringToByteArray(hash);

                string diff = sr.ReadLine();
                c.mIgnoreDifficultyDifference = (diff == "1");

                sr.Close();

                CurrencyBox.Items.Add(c);
            }
        }

        private void SaveCurrency(Currency c)
        {
            FileStream f = File.Open("Currencies/" + c, FileMode.Create);
            StreamWriter sw = new StreamWriter(f);
            sw.WriteLine(c.mIPAddress);
            sw.WriteLine(c.mPort);
            sw.WriteLine(c.mNetVersion);
            sw.WriteLine(c.mNetID);
            sw.WriteLine(c.mBlockTarget);
            sw.WriteLine(Program.HashToString(c.mLastHeaderHash));
            sw.WriteLine(c.mIgnoreDifficultyDifference ? "1" : "0");
            sw.Close();            
        }

        private void Calculate_Click(object sender, EventArgs e)
        {
            //try
            {
                Currency c = (Currency)CurrencyBox.SelectedItem;
                
                Bitcoin bc = new Bitcoin();
                bc.AddNode(c.mIPAddress, c.mPort, c.mNetVersion, c.mNetID);                

                while (!bc.IsConnected())
                {
                    Thread.Sleep(100);
                }

                bc.RequestHeaders(c.mLastHeaderHash);
                bc.WaitForHeaders();

                BlockHeader[] blocks = bc.GetRecentBlockHeaders(c.mIgnoreDifficultyDifference);
                c.mLastHeaderHash = blocks[0].mPrevBlock;
                SaveCurrency(c);

                UInt64 timeSeconds = Program.UnixTime() - (UInt64)blocks[0].mTimestamp;
                uint expectedBlocks = (uint)(timeSeconds / c.mBlockTarget);

                UInt64 max = 4294901760;
                uint diffZeros = ((blocks[0].mDifficultyBits >> 24) & 0xFF);
                int shift = 40 - ((int)(32 - diffZeros) * 8);                
                UInt64 diffBits = (UInt64)(blocks[0].mDifficultyBits & 0x00FFFFFF);
                if (shift < 0)
                    max <<= (0 - shift);
                else
                    diffBits <<= shift;
                double difficulty = (double)max / (double)diffBits;
                difficultyBox.Text = difficulty.ToString();

                double hashRate = (((double)blocks.Length / (double)expectedBlocks) * difficulty * 4294967296) / c.mBlockTarget;
                hashrateBox.Text = string.Format("{0:N0}", hashRate);

                blocksPerDayBox.Text = blocks.Length.ToString();
            }
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace CentralMine.NET
{
    public partial class Form1 : Form
    {
        ClientManager mTheMan;
        double mHashesDone = 0;
        uint mPrevHashes = 0;
        DateTime mStartTime;

        public Form1()
        {
            InitializeComponent();

            mStartTime = DateTime.Now;
            mTheMan = new ClientManager();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Update block info
            if (mTheMan != null && mTheMan.mBlock != null)
            {
                lblMidstate.Text = "0x" + mTheMan.mBlock.strMidstate;
                lblData.Text = "0x" + mTheMan.mBlock.strData;
                lblHash1.Text = "0x" + mTheMan.mBlock.strHash1;
                lblTarget.Text = "0x" + mTheMan.mBlock.strTarget;
            }

            lblClients.Text = "Clients: " + mTheMan.mClients.Count;


            float percent = ((float)mTheMan.mHashMan.mHashesDone / (float)mTheMan.mHashMan.mHashesTotal) * 100;
            progressBar1.Value = (int)percent;

            lblProgress.Text = String.Format("{0:N0} / {1:N0} ({2}%)", mTheMan.mHashMan.mHashesDone, mTheMan.mHashMan.mHashesTotal, percent);
            

            uint hashesSinceLast = (mTheMan.mHashMan.mHashesDone - mPrevHashes);
            mHashesDone += hashesSinceLast;            
            TimeSpan span = DateTime.Now - mStartTime;
            double hashesPerSecond = mHashesDone / span.TotalSeconds;
  
            mPrevHashes = mTheMan.mHashMan.mHashesDone;

            uint hashesRemaining = mTheMan.mHashMan.mHashesTotal - mTheMan.mHashMan.mHashesDone;
            double secondsRemaining = (double)hashesRemaining / hashesPerSecond;

            int hoursRemaining = (int)(secondsRemaining / 3600);
            secondsRemaining -= (hoursRemaining * 3600);
            int minutesRemaining = (int)(secondsRemaining / 60);
            secondsRemaining -= (minutesRemaining * 60);

            lblHashrate.Text = String.Format("Hashrate: {0:N} / second  Time Remaining: {1}:{2}:{3}", hashesPerSecond, hoursRemaining, minutesRemaining, (int)secondsRemaining);
        }
    }
}

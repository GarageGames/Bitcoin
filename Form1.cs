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

        public Form1()
        {
            InitializeComponent();
            mTheMan = new ClientManager();

            protocol.Items.Add(ClientManager.Currency.Bitcoin);
            protocol.Items.Add(ClientManager.Currency.Feathercoin);
            protocol.Items.Add(ClientManager.Currency.Novacoin);
            protocol.SelectedIndex = 2;           
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

            if (mTheMan.mBlock != null)
            {
                float percent = ((float)mTheMan.mBlock.mHashMan.mHashesDone / (float)mTheMan.mBlock.mHashMan.mHashesTotal) * 100;
                progressBar1.Value = (int)percent;

                lblProgress.Text = String.Format("{0:N0} / {1:N0} ({2}%)", mTheMan.mBlock.mHashMan.mHashesDone, mTheMan.mBlock.mHashMan.mHashesTotal, percent);

                double hashesPerSecond = mTheMan.mHashrate;
                uint hashesRemaining = mTheMan.mBlock.mHashMan.mHashesTotal - mTheMan.mBlock.mHashMan.mHashesDone;
                double secondsRemaining = (double)hashesRemaining / hashesPerSecond;

                int hoursRemaining = (int)(secondsRemaining / 3600);
                secondsRemaining -= (hoursRemaining * 3600);
                int minutesRemaining = (int)(secondsRemaining / 60);
                secondsRemaining -= (minutesRemaining * 60);

                lblHashrate.Text = String.Format("Hashrate: {0:N0} / second.   Hashes per client(avg): {1:N0}", (ulong)hashesPerSecond, (ulong)(hashesPerSecond / mTheMan.mClients.Count));
            }

            
        }

        private void protocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            mTheMan.SetCurrency((ClientManager.Currency)protocol.SelectedItem);
        }
    }
}

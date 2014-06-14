using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace HashrateCalculator
{
    public partial class Form2 : Form
    {
        List<BCNet.Bitcoin> mCurrencies;
        CryptoIRC.CryptoIRC mIRC;

        public Form2()
        {
            mCurrencies = new List<BCNet.Bitcoin>();
            InitializeComponent();

            mIRC = new CryptoIRC.CryptoIRC();

            LoadCurrencies();
        }

        void LoadCurrencies()
        {
            try
            {
                IEnumerable<string> currencies = Directory.EnumerateDirectories("Currencies");
                foreach (string currency in currencies)
                {
                    string currencyName = currency.Substring(currency.LastIndexOf('\\') + 1);
                    BCNet.Bitcoin bc = new BCNet.Bitcoin(currencyName, currency);
                    mCurrencies.Add(bc);
                    mIRC.SubscribeCurrency(bc.IRCChannelName, new CryptoIRC.PeerDiscoveredCB(bc.NodeDiscovered));
                    //bc.AddNode("54.241.118.84", 13580);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

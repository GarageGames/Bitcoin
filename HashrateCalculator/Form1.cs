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
using System.Net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace HashrateCalculator
{
    public partial class Form1 : Form
    {
        Thread mUpdateThread;
        Dictionary<string, Currency> mCurrencies;

        DateTime mLastBTCTime;
        DateTime mLastPriceTime;
        DateTime mLastNetTime;
        double mBTCValue = 0;
        const string currencyDir = "Currencies/";

        public Form1()
        {
            InitializeComponent();            

            mCurrencies = new Dictionary<string, Currency>();

            LoadCurrencies();
            mUpdateThread = new Thread(new ThreadStart(UpdateThreadFunc));
            mUpdateThread.Start();
        }

        string ReadWebString(string url)
        {
            try
            {
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Stream s = response.GetResponseStream();
                StreamReader rs = new StreamReader(s, Encoding.GetEncoding("utf-8"));

                string retVal = rs.ReadToEnd();
                rs.Close();

                int bodyIndex = retVal.IndexOf("<body>", StringComparison.CurrentCultureIgnoreCase);
                if (bodyIndex >= 0)
                {
                    retVal = retVal.Substring(bodyIndex + 6);
                    retVal = retVal.Substring(0, retVal.IndexOf("</body>", StringComparison.CurrentCultureIgnoreCase));
                    retVal = retVal.Trim();
                }

                return retVal;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        void UpdateThreadFunc()
        {
            while (true)
            {
                // Start fetching headers for hashrate
                foreach (KeyValuePair<string, Currency> c in mCurrencies)
                    c.Value.StartCalcHashrate();

                // Get BTC Value in USD
                TimeSpan span = DateTime.Now - mLastBTCTime;
                if (span.TotalSeconds > 30)
                {
                    string btcVal = ReadWebString("http://www.bitcoinexchangerate.org/price");
                    if (btcVal != null && btcVal.Length > 0)
                    {
                        mBTCValue = Convert.ToDouble(btcVal);
                    }
                    mLastBTCTime = DateTime.Now;
                }

                // Get Price data from coinchoose.com
                span = DateTime.Now - mLastPriceTime;
                if (span.TotalSeconds > 240)
                {
                    string priceData = ReadWebString("http://www.coinchoose.com/api.php?base=BTC");
                    if (priceData != null && priceData.Length > 0)
                    {
                        List<PriceData> p = JsonConvert.DeserializeObject<List<PriceData>>(priceData);
                        foreach (PriceData price in p)
                        {
                            if (price.algo == "scrypt")
                            {
                                if (!mCurrencies.ContainsKey(price.name))
                                {
                                    Currency nc = new Currency();
                                    nc.mName = price.name;
                                    nc.mSymbol = price.symbol;
                                    nc.mExchangeURL = price.exchange_url;
                                    nc.mStatus = "New";
                                    mCurrencies[price.name] = nc;
                                }
                                Currency c = mCurrencies[price.name];
                                c.mBTCPerCoin = price.price;
                            }
                        }
                    }
                    mLastPriceTime = DateTime.Now;
                }
                
                // Finish hashrate calculations
                span = DateTime.Now - mLastNetTime;
                if (span.TotalSeconds > 30)
                {
                    foreach (KeyValuePair<string, Currency> c in mCurrencies)
                    {
                        if (!c.Value.FinishCalcHashrate())
                            break;
                        SaveCurrency(c.Value);
                        mLastNetTime = DateTime.Now;
                    }
                }

                // Update currencies display
                UpdatePrices();
                
                // Sleep for a bit
                Thread.Sleep(1000);
            }
        }

        private void UpdatePrices()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate() { UpdatePrices(); }));
            }
            else
            {
                double hr = Convert.ToDouble(hashrate.Text);
                foreach (KeyValuePair<string, Currency> kvp in mCurrencies)
                {
                    Currency c = kvp.Value;
                    c.Update(mBTCValue, hr);

                    // Find it in the listview
                    ListViewItem item = currencyList.FindItemWithText(c.mName);
                    if (item == null )
                    {
                        // Not in the list, add it now
                        item = currencyList.Items.Add(c.mName);
                        item.SubItems.Add(c.mSymbol);
                        item.SubItems.Add("0");     // $/day
                        item.SubItems.Add("0");     // $/coin
                        item.SubItems.Add("0");     // BTC/coin
                        item.SubItems.Add("0");     // hashrate
                        item.SubItems.Add("0");     // block time
                        item.SubItems.Add("0");     // blocks/day
                        item.SubItems.Add("0");     // coins/ block
                        item.SubItems.Add("0");     // status
                    }

                    string tempUSDPerDay = c.mUSDPerDay.ToString();
                    string tempUSDPerCoin = c.mUSDPerCoin.ToString();
                    string tempBTCPerCoin = c.mBTCPerCoin.ToString("F99").TrimEnd('0');
                    string tempHashrate = c.mHashrate.ToString("N0");
                    string tempBlockTarget = c.mBlockTarget.ToString();
                    string tempBlocksPerDay = c.mBlocksPerDay.ToString();
                    string tempCoinsPerBlock = c.mCoinsPerBlock.ToString();
                    string tempStatus = c.mStatus;

                    if (item.SubItems[2].Text != tempUSDPerDay) item.SubItems[2].Text = tempUSDPerDay;
                    if (item.SubItems[3].Text != tempUSDPerCoin) item.SubItems[3].Text = tempUSDPerCoin;
                    if (item.SubItems[4].Text != tempBTCPerCoin) item.SubItems[4].Text = tempBTCPerCoin;
                    if (item.SubItems[5].Text != tempHashrate) item.SubItems[5].Text = tempHashrate;
                    if (item.SubItems[6].Text != tempBlockTarget) item.SubItems[6].Text = tempBlockTarget;
                    if (item.SubItems[7].Text != tempBlocksPerDay) item.SubItems[7].Text = tempBlocksPerDay;
                    if (item.SubItems[8].Text != tempCoinsPerBlock) item.SubItems[8].Text = tempCoinsPerBlock;
                    if (item.SubItems[9].Text != tempStatus) item.SubItems[9].Text = tempStatus;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            mUpdateThread.Abort();
        }

        private void LoadCurrencies()
        {
            if (!Directory.Exists(currencyDir))
                Directory.CreateDirectory(currencyDir);
            string[] files = Directory.GetFiles(currencyDir);
            
            foreach( string filename in files)
            {
                Currency c = new Currency();
                c.mName = filename.Substring(11);
                c.Load(filename.Substring(0, 11));
                mCurrencies[c.mName] = c;
            }
        }

        private void SaveCurrency(Currency c)
        {
            c.Save(currencyDir);         
        }

        private void currencyList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hti = currencyList.HitTest(e.Location);
            if (hti.Item != null)
            {
                Currency c = mCurrencies[hti.Item.Text];                
                CurrencyProperties dlg = new CurrencyProperties();
                dlg.Text = c.mName;
                dlg.IPAddress.Text = c.mIPAddress;
                dlg.Port.Text = c.mPort.ToString();
                dlg.NetVersion.Text = c.mNetVersion.ToString();
                dlg.NetID.Text = c.mNetID.ToString();
                dlg.BlockTarget.Text = c.mBlockTarget.ToString();
                dlg.CoinsPerBlock.Text = c.mCoinsPerBlock.ToString();
                dlg.POS.Checked = c.mPOS;
                dlg.IgnoreSigLen.Checked = c.mIgnoreSigLen;
                dlg.ScryptBlockHash.Checked = c.mScryptBlockHash;
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    c.mIPAddress = dlg.IPAddress.Text;
                    try { c.mPort = Convert.ToUInt16(dlg.Port.Text); }              catch (Exception) { }
                    try { c.mNetVersion = Convert.ToUInt32(dlg.NetVersion.Text); }  catch (Exception) { }
                    try { c.mNetID = Convert.ToUInt32(dlg.NetID.Text); }            catch (Exception) { }
                    try { c.mBlockTarget = Convert.ToUInt32(dlg.BlockTarget.Text); }catch (Exception) { }
                    try { c.mCoinsPerBlock = Convert.ToUInt32(dlg.CoinsPerBlock.Text); } catch (Exception) { }
                    c.mPOS = dlg.POS.Checked;
                    c.mIgnoreSigLen = dlg.IgnoreSigLen.Checked;
                    c.mScryptBlockHash = dlg.ScryptBlockHash.Checked;
                    SaveCurrency(c);
                    c.Load(currencyDir);
                }
            }
        }
    }
}

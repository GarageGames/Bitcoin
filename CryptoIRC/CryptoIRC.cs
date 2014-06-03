using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CryptoIRC
{
    public delegate void PeerDiscoveredCB(string peerName);

    public class CryptoIRC
    {        
        enum ConnectionState
        {
            Disconnected,
            Connected,
        }

        ConnectionState mState;
        Socket mSocket;
        Thread mThread;

        byte[] mPacketBuffer;
        int mPacketBufferCount;

        Dictionary<string, IRCCurrency> mCurrencies;

        DateTime mLastChannelRefresh;
        List<string> mChannels;
        List<string> mPendingChannels;
        bool mChannelsUpdated = false;

        public CryptoIRC()
        {
            mPacketBuffer = new byte[1024 * 32];
            mPacketBufferCount = 0;

            mCurrencies = new Dictionary<string, IRCCurrency>();

            mChannels = new List<string>();

            mState = ConnectionState.Disconnected;
            mThread = new Thread(new ThreadStart(IrcThread));
            mThread.Start();
        }

        public void Destroy()
        {
            mThread.Abort();
            if (mSocket != null && mSocket.Connected)
                mSocket.Close();
        }

        public void SubscribeCurrency(string currency, PeerDiscoveredCB cb)
        {
            IRCCurrency c = new IRCCurrency(cb);
            mCurrencies[currency] = c;
        }

        void IrcThread()
        {
            while (true)
            {
                ReadPackets();

                switch (mState)
                {
                    case ConnectionState.Disconnected:
                        Connect();
                        break;
                    case ConnectionState.Connected:
                        UpdateConnected();
                        break;
                }

                Thread.Sleep(50);
            }
        }

        void ReadPackets()
        {
            try
            {
                if (mSocket != null && mSocket.Connected && mSocket.Available > 0)
                {
                    byte[] data = new byte[1024 * 8];
                    int bytesRead = mSocket.Receive(data);
                    if (bytesRead > 0)
                    {
                        Buffer.BlockCopy(data, 0, mPacketBuffer, mPacketBufferCount, bytesRead);
                        mPacketBufferCount += bytesRead;
                        ProcessPackets();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CryptoIRC::ReadPackets - " + ex.Message);
                mSocket.Close();
                mState = ConnectionState.Disconnected;
            }
        }

        void ProcessPackets()
        {
            while (mPacketBufferCount > 0)
            {
                int packetSize = 0;
                int tailSize = 0;
                for (int i = 0; i < mPacketBufferCount; i++)
                {
                    if (mPacketBuffer[i] == '\r' || mPacketBuffer[i] == '\n')
                    {
                        packetSize = i;
                        if (mPacketBuffer[i] == '\r' && i < (mPacketBufferCount - 1) && mPacketBuffer[i + 1] == '\n')
                            tailSize = 2;
                        else
                            tailSize = 1;
                        break;
                    }
                }

                int dataSize = packetSize + tailSize;
                if (dataSize <= 0)
                    break;

                byte[] packet = new byte[packetSize];
                Buffer.BlockCopy(mPacketBuffer, 0, packet, 0, packetSize);
                mPacketBufferCount -= dataSize;
                Buffer.BlockCopy(mPacketBuffer, dataSize, mPacketBuffer, 0, mPacketBufferCount);

                ProcessPacket(packet);                
            }
        }

        void ProcessPacket(byte[] packet)
        {
            string str = System.Text.Encoding.ASCII.GetString(packet);
            Console.WriteLine(str);

            try
            {
                string[] pieces = str.Split(' ');
                if (pieces[1] == "JOIN")
                {
                    HandleUserJoinedChannel(pieces);
                }
                else if (pieces[1] == "PART")
                {
                }
                else
                {
                    int code = Convert.ToInt32(pieces[1]);
                    switch (code)
                    {
                        case 321:
                            mPendingChannels = new List<string>();
                            break;
                        case 322:
                            mPendingChannels.Add(pieces[3]);
                            break;
                        case 323:
                            mChannels = mPendingChannels;
                            mPendingChannels = null;
                            mChannelsUpdated = true;
                            break;
                        case 353:
                            HandleUserList(pieces);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CryptoIRC::ProcessPacket - " + ex.Message);
            }
            
        }

        void Connect()
        {
            try
            {
                foreach (KeyValuePair<string, IRCCurrency> kvp in mCurrencies)
                {
                    kvp.Value.Clear();
                    mLastChannelRefresh = new DateTime();
                }

                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSocket.Connect("irc.lfnet.org", 6667);
                mState = ConnectionState.Connected;

                SendString("NICK CryptoIRC");
                SendString("USER CryptoIRC 8 * :CryptoIRC");                                
            }
            catch (Exception ex)
            {
                Console.WriteLine("CryptoIRC::Connect - " + ex.Message);
            }
        }

        void SendString(string str)
        {
            string send = str + "\r\n";
            byte[] data = System.Text.Encoding.ASCII.GetBytes(send);
            mSocket.Send(data);
        }

        void UpdateConnected()
        {
            TimeSpan span = DateTime.Now - mLastChannelRefresh;
            if (span.TotalSeconds > 300)
            {
                SendString("LIST");
                mLastChannelRefresh = DateTime.Now;
            }

            if (mChannelsUpdated)
            {
                mChannelsUpdated = false;
                foreach (KeyValuePair<string, IRCCurrency> kvp in mCurrencies)
                {
                    string[] channels = kvp.Value.GetChannels();
                    for (int i = 0; i < 100; i++)
                    {
                        string channelName = "#" + kvp.Key + String.Format("{0:D2}", i);

                        // Check to see if we are already in this channel
                        bool inChannel = false;
                        foreach (string ch in channels)
                        {
                            if (ch == channelName)
                            {
                                inChannel = true;
                                break;
                            }
                        }

                        if (!inChannel)
                        { 
                            // See if the channel exists
                            foreach (string ch in mChannels)
                            {
                                if (ch == channelName)
                                {
                                    // Channel exists, join it now
                                    SendString("JOIN " + channelName);
                                    kvp.Value.JoinChannel(channelName);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        void HandleUserList(string[] users)
        {
            string me = users[2];

            string channelName = users[4].Substring(1);
            string currencyName = channelName.Substring(0, channelName.Length - 2);

            if (mCurrencies.ContainsKey(currencyName))
            {
                IRCCurrency c = mCurrencies[currencyName];

                for (int i = 5; i < users.Length; i++)
                {
                    string name = users[i];

                    if (i == 5)
                        name = name.Substring(1);    // Skip the colon on the first name
                    if (name == me)
                        continue;

                    c.DiscoverPeer(name);
                }
            }
            else
                Console.WriteLine("CryptoIRC::HandleUserList - Unknown currency: " + currencyName);
        }

        void HandleUserJoinedChannel(string[] joinInfo)
        {
            string channelName = joinInfo[2].Substring(2);
            string currencyName = channelName.Substring(0, channelName.Length - 2);

            if (mCurrencies.ContainsKey(currencyName))
            {
                IRCCurrency c = mCurrencies[currencyName];
                string user = joinInfo[0].Substring(1);
                int idx = user.IndexOf('!');
                if (idx > 0)
                {
                    user = user.Substring(0, idx);
                    if( user != "CryptoIRC" )
                        c.DiscoverPeer(user);
                }
            }
            else
                Console.WriteLine("CryptoIRC::HandleUserJoinedChannel - Unknown currency: " + currencyName);
        }
    }
}

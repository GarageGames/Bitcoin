using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoIRC
{
    class IRCCurrency
    {
        PeerDiscoveredCB mCB;
        List<string> mChannels;
        List<string> mPeers;

        public IRCCurrency(PeerDiscoveredCB cb)
        {
            mCB = cb;
            mChannels = new List<string>();
            mPeers = new List<string>();
        }

        public string[] GetChannels()
        {
            return mChannels.ToArray();
        }

        public void JoinChannel(string channelName)
        {
            mChannels.Add(channelName);
        }

        public void Clear()
        {
            mChannels.Clear();
            mPeers.Clear();
        }

        public void DiscoverPeer(string peerName)
        {
            foreach (string p in mPeers)
            {
                if (p == peerName)
                    return;
            }

            mPeers.Add(peerName);
            mCB(peerName);
        }
    }
}

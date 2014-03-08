using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CentralMine.NET
{
    public class Listener
    {
        TcpListener mSocket;
        Thread mThread;
        ClientManager mTheMan;

        public Listener(int port, ClientManager manager)
        {
            mTheMan = manager;
            mSocket = new TcpListener(IPAddress.Any, port);

            mThread = new Thread(new ThreadStart(ListenLoop));
            mThread.Start();
        }

        void ListenLoop()
        {
            mSocket.Start();

            while (true)
            {
                TcpClient client = mSocket.AcceptTcpClient();
                mTheMan.AcceptClient(client);
            }
        }
    }
}

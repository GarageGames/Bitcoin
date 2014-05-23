using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentralMine.NET
{
    abstract class Upstream
    {
        protected string mURL;
        protected ushort mPort;
        protected string mUser;
        protected string mPass;
        protected ClientManager mClientManager;

        public Upstream(ClientManager cm)
        {
            mClientManager = cm;
        }

        virtual public void Destroy()
        {
        }

        virtual public void SetHost(string url, ushort port)
        {
            mURL = url;
            mPort = port;
        }

        virtual public void SetCredentials(string user, string pass)
        {
            mUser = user;
            mPass = pass;
        }

        abstract public WorkBlock GetWorkBlock();
        abstract public bool SubmitWork(WorkBlock work, uint solution);

        abstract public bool NewBlockReady();
    }
}

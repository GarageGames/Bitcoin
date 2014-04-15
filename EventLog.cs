using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;


namespace CentralMine.NET
{
    public class EventLog
    {
        public enum EventType
        {
            Server,
            Network,
            Upstream,
            HashWork,
        };

        struct EventInfo
        {
            public DateTime mTime;
            public EventType mType;
            public string mEventData;
        };

        struct ClientWork
        {
            public DateTime mTime;
            public string mMemberName;
            public string mProductName;
            public uint mHashes;
        };

        struct Product
        {
            public uint mMember;
            public uint mProduct;
            public ulong mHashes;
        };

        List<EventInfo> mEventQueue;
        Mutex mEventQueueLock;

        DateTime mIntervalStart;
        List<ClientWork> mClientWorkQueue;
        Mutex mClientWorkQueueLock;

        Thread mUpdateThread;
        MySqlConnection mSql = null;

        Dictionary<string, uint> mMemberIDs;
        Dictionary<string, uint> mProductIDs;

        public EventLog()
        {
            mMemberIDs = new Dictionary<string, uint>();
            mProductIDs = new Dictionary<string, uint>();

            mEventQueue = new List<EventInfo>();
            mClientWorkQueue = new List<ClientWork>();
            mIntervalStart = DateTime.Now;

            ConnectToDB();

            mEventQueueLock = new Mutex();
            mClientWorkQueueLock = new Mutex();
            mUpdateThread = new Thread(new ThreadStart(Update));
            mUpdateThread.Start();
        }

        public void Close()
        {
            // Kill the update thread
            mUpdateThread.Abort();
        }

        public void RecordEvent(EventType type, string evt)
        {
            EventInfo info = new EventInfo();
            info.mTime = DateTime.Now;
            info.mType = type;

            if (evt.Length < 256)
                info.mEventData = evt;
            else
                info.mEventData = evt.Substring(0, 255);

            mEventQueueLock.WaitOne();
            mEventQueue.Add(info);
            mEventQueueLock.ReleaseMutex();
        }

        public void RecordClientWork(Client c)
        {
            ClientWork w = new ClientWork();
            w.mTime = DateTime.UtcNow;
            w.mMemberName = c.mAgent;
            w.mProductName = c.mLocation;
            w.mHashes = c.mHashesDone;

            if (w.mMemberName == null || w.mMemberName.Length <= 0)
                w.mMemberName = "unknown";
            if (w.mProductName == null || w.mProductName.Length <= 0)
                w.mProductName = "unknown";

            mClientWorkQueueLock.WaitOne();
            mClientWorkQueue.Add(w);
            mClientWorkQueueLock.ReleaseMutex();
        }

        void ConnectToDB()
        {
            if (mSql != null)
                mSql.Close();

            string connStr = "server=localhost;user=root;database=minerdata;port=3306;password=password;";
            mSql = new MySqlConnection(connStr);
            try
            {
                mSql.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT * FROM members", mSql);
                MySqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    string memberName = (string)r[1];
                    uint id = (uint)r[0];
                    mMemberIDs[memberName] = id;
                }
                r.Close();

                cmd = new MySqlCommand("SELECT * FROM products", mSql);
                r = cmd.ExecuteReader();
                while (r.Read())
                {
                    string productName = (string)r[1];
                    uint id = (uint)r[0];
                    mProductIDs[productName] = id;
                }
                r.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                mSql = null;
            }
        }

        uint GetMemberId(string memberName)
        {
            if (memberName.Length > 64)
                memberName = memberName.Substring(0, 64);

            if (!mMemberIDs.ContainsKey(memberName))
            {
                string sql = "INSERT INTO members (member_name) VALUES ('" + memberName + "')";
                try
                {

                    MySqlCommand cmd = new MySqlCommand(sql, mSql);
                    cmd.ExecuteNonQuery();

                    // Now querry the database for the index
                    sql = "SELECT * FROM members WHERE member_name='" + memberName + "'";
                    cmd = new MySqlCommand(sql, mSql);
                    MySqlDataReader r = cmd.ExecuteReader();
                    r.Read();
                    uint ID = (uint)r[0];
                    r.Close();

                    mMemberIDs[memberName] = ID;
                }
                catch (Exception ex)
                {
                    Email.SendErrorEmail(sql + "\n\n" + ex.ToString());
                }
            }

            return mMemberIDs[memberName];
        }

        uint GetProductId(string product, uint memberId)
        {
            if (product.Length > 128)
                product = product.Substring(0, 128);

            if (!mProductIDs.ContainsKey(product))
            {
                string sql = "INSERT INTO products (product_name, member) VALUES ('" + product + "', '" + memberId + "')";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(sql, mSql);
                    cmd.ExecuteNonQuery();

                    // Now querry the database for the index
                    sql = "SELECT * FROM products WHERE product_name='" + product + "'";
                    cmd = new MySqlCommand(sql, mSql);
                    MySqlDataReader r = cmd.ExecuteReader();
                    r.Read();
                    uint ID = (uint)r[0];
                    r.Close();
                    mProductIDs[product] = ID;
                }
                catch (Exception ex)
                {
                    Email.SendErrorEmail(sql + "\n\n" + ex.ToString());
                }
                
            }

            return mProductIDs[product];
        }

        void Update()
        {
            while (true)
            {
                DateTime start = DateTime.Now;

                // Copy the events list locally
                mEventQueueLock.WaitOne();
                List<EventInfo> events = mEventQueue;
                mEventQueue = new List<EventInfo>();
                mEventQueueLock.ReleaseMutex();

                // Send data to database
                string sql = "";
                try
                {
                    foreach (EventInfo e in events)
                    {
                        sql = string.Format("INSERT INTO events (timestamp, type, event) VALUES ('{0}', '{1}', '{2}')", e.mTime.ToString("yyyy-MM-dd HH:mm:ss"), (int)e.mType, e.mEventData);
                        MySqlCommand cmd = new MySqlCommand(sql, mSql);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Email.SendErrorEmail(sql + "\n\n" + ex.ToString());
                }

                // Copy the client work list
                mClientWorkQueueLock.WaitOne();
                List<ClientWork> cw = mClientWorkQueue;
                mClientWorkQueue = new List<ClientWork>();
                TimeSpan interval = DateTime.Now - mIntervalStart;
                mIntervalStart = DateTime.Now;
                mClientWorkQueueLock.ReleaseMutex();

                try
                {
                    // Collect information into products
                    Dictionary<string, Product> products = new Dictionary<string, Product>();
                    foreach (ClientWork w in cw)
                    {
                        if (!products.ContainsKey(w.mProductName))
                        {
                            // Add this product
                            Product p = new Product();
                            p.mMember = GetMemberId(w.mMemberName);
                            p.mProduct = GetProductId(w.mProductName, p.mMember);
                            p.mHashes = 0;
                            products[w.mProductName] = p;
                        }

                        Product prod = products[w.mProductName];
                        prod.mHashes += w.mHashes;
                        products[w.mProductName] = prod;
                    }

                    // Write entries into the database for each prodcut
                    string timeString = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                    foreach (KeyValuePair<string, Product> kv in products)
                    {
                        ulong hashrate = (ulong)(kv.Value.mHashes / interval.TotalSeconds);
                        sql = string.Format("INSERT INTO workdata (member_id, product_id, hashrate, timestamp) VALUES ('{0}', '{1}', '{2}', '{3}')", kv.Value.mMember, kv.Value.mProduct, hashrate, timeString);
                        MySqlCommand cmd = new MySqlCommand(sql, mSql);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Email.SendErrorEmail(sql + "\n\n" + ex.ToString());
                }

                TimeSpan duration = DateTime.Now - start;
                double secondsRemaining = 60 - duration.TotalSeconds;

                if (secondsRemaining < 0)
                {
                    Console.WriteLine("Log thread took {0} seconds, yeilding and going again", (int)duration.TotalSeconds);
                    Thread.Sleep(10);
                }
                else
                {
                    Console.WriteLine("Log Thread sleeping for {0} seconds", (int)secondsRemaining);
                    Thread.Sleep((int)secondsRemaining * 1000);
                }
            }
        }
    }
}

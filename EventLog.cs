using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public EventLog()
        {
        }

        public void RecordEvent(EventType type, string evt)
        {
        }
    }
}

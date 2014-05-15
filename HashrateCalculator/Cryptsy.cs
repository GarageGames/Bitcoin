using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashrateCalculator
{
    namespace Cryptsy
    {
        public class Recenttrade
        {
            public string id { get; set; }
            public string time { get; set; }
            public string price { get; set; }
            public string quantity { get; set; }
            public string total { get; set; }
        }

        public class Sellorder
        {
            public string price { get; set; }
            public string quantity { get; set; }
            public string total { get; set; }
        }

        public class Buyorder
        {
            public string price { get; set; }
            public string quantity { get; set; }
            public string total { get; set; }
        }

        public class Market
        {
            public string marketid { get; set; }
            public string label { get; set; }
            public string lasttradeprice { get; set; }
            public string volume { get; set; }
            public string lasttradetime { get; set; }
            public string primaryname { get; set; }
            public string primarycode { get; set; }
            public string secondaryname { get; set; }
            public string secondarycode { get; set; }
            public List<Recenttrade> recenttrades { get; set; }
            public List<Sellorder> sellorders { get; set; }
            public List<Buyorder> buyorders { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashrateCalculator
{
    public class Block : NetworkDataObject
    {

        public Block(NetworkDataObject.DataStatus status = NetworkDataObject.DataStatus.NoData) : base(status)
        {
        }

        public Block(byte[] hash, NetworkDataObject.DataStatus status = NetworkDataObject.DataStatus.NoData)
            : base(hash, status)
        {
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashrateCalculator
{
    public class NetworkDataObject
    {
        public enum DataStatus
        {
            NoData,
            Requested,
            Loaded
        };

        byte[] mHash;
        DataStatus mDataStatus;


        public NetworkDataObject(DataStatus status = DataStatus.NoData)
        {
            mDataStatus = status;
        }

        public NetworkDataObject(byte[] hash, DataStatus status = DataStatus.NoData)
        {
            mHash = new byte[hash.Length];
            Array.Copy(hash, mHash, hash.Length);

            mDataStatus = status;
        }
        
        public DataStatus Status
        {
            get { return mDataStatus; }
            set { mDataStatus = value; }
        }

        public byte[] Hash
        {
            get { return mHash; }
            set { }
        }
    }
}

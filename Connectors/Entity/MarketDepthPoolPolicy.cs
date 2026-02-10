using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class MarketDepthPoolPolicy : IPooledObjectPolicy<MarketDepth>
    {
        public MarketDepth Create()
        {
            return new MarketDepth();
        }

        public bool Return(MarketDepth md)
        {
            md.Asks.Clear();
            md.Bids.Clear();

            return true;
        }
    }
}

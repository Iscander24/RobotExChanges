using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    internal class MarketDepthSCPoolPolicy : IPooledObjectPolicy<MarketDepthSC>
    {
        public MarketDepthSC Create()
        {
            return new MarketDepthSC();
        }

        public bool Return(MarketDepthSC md)
        {
            //md.Asks.Clear();
            //md.Bids.Clear();

            return true;
        }
    }
}

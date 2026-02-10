using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    internal class NewMarketDepth
    {
        internal NewMarketDepth(MarketDepth marketDepth)
        {
            MarketDepth = marketDepth;

            IsNew = true;
        }

        internal bool IsNew = true;

        internal MarketDepth MarketDepth;
    }
}

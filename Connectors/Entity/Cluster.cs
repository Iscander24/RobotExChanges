using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class Cluster
    {
        public Cluster(Trade trade)
        {
            AddTick(trade);
        }

        #region Properties =========================================================================

        /// <summary>
        /// BidVolume - AskVolume
        /// </summary>
        public decimal Delta => BidVolume - AskVolume;

        /// <summary>
        /// BidVolume + AskVolume
        /// </summary>
        public decimal Sum => BidVolume + AskVolume;

        #endregion

        #region Fields =========================================================================

        public decimal Price = 0;

        public decimal AskVolume = 0;

        public decimal BidVolume = 0;

        #endregion

        #region Methods =========================================================================

        public decimal AddTick(Trade trade)
        {
            if (trade.Operation == Enums.Operation.Buy)
            {
                BidVolume += trade.Volume;
            }
            else
            {
                AskVolume += trade.Volume;
            }

            return Sum;
        }

        #endregion
    }
}

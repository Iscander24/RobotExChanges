using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    internal class MarketDepthSC
    {
        public long? sec;

        /// <summary>
        /// security that owns to glass
        /// бумага, которой принадлежит стакан
        /// </summary>
        public string IsinId { get; set; } = string.Empty;


        /// <summary>
        /// levels of sales. best with index 0
        /// уровни продаж. лучшая с индексом 0
        /// </summary>
        public ConcurrentSortedList Asks { get; set; } = new ConcurrentSortedList();

        /// <summary>
        /// purchase levels. best with index 0
        /// уровни покупок. лучшая с индексом 0
        /// </summary>
        public ConcurrentSortedList Bids { get; set; } = new ConcurrentSortedList();

        public MarketDepth GetMarketDepth(decimal bidPrice, decimal askPrice, ConcurrentDictionary<string, NewMarketDepth> _marketDepthsToSend)
        {
            MarketDepth marketDepth;

            if (!_marketDepthsToSend.TryGetValue(IsinId, out NewMarketDepth? newMarketDepth))
            {
                marketDepth = new MarketDepth();
            }
            else
            {
                marketDepth = newMarketDepth.MarketDepth;
                //marketDepth.Asks.Clear();
                //marketDepth.Bids.Clear();
            }


            marketDepth.IsinId = IsinId;

            marketDepth.DateTime = DateTime.Now;

            var asks = Asks.GetSortedItems();

            List< MarketDepthLevel > levels = new List< MarketDepthLevel >();   

            foreach (KeyValuePair<decimal, decimal> item in asks)
            {
                if (item.Key > bidPrice)
                {
                    MarketDepthLevel level = new MarketDepthLevel()
                    {
                        Price = item.Key,
                        Volume = item.Value
                    };

                    levels.Add(level);

                    //marketDepth.Asks.Add(level);
                }               
                
            }

            marketDepth.Asks = levels;

            List<MarketDepthLevel> bidlevels = new List<MarketDepthLevel>();

            var bids = Bids.GetSortedItems();

            foreach (KeyValuePair<decimal, decimal> item in bids)
            {
                if (item.Key < askPrice)
                {
                    MarketDepthLevel level = new MarketDepthLevel()
                    {
                        Price = item.Key,
                        Volume = item.Value
                    };

                    bidlevels.Insert(0, level);

                    //marketDepth.Bids.Insert(0, level);
                }                
            }

            marketDepth.Bids = bidlevels;

            return marketDepth;
        }
    }
}

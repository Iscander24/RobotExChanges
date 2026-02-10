using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    internal class MarketDepthSorted
    {
        /// <summary>
        /// security that owns to glass
        /// бумага, которой принадлежит стакан
        /// </summary>
        public string IsinId { get; set; } = string.Empty;

        /// <summary>
        /// time to create a glass
        /// время создания стакана
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// levels of sales. best with index 0
        /// уровни продаж. лучшая с индексом 0
        /// </summary>
        public SortedList<decimal, decimal> Asks { get; set; } = new SortedList<decimal, decimal>();

        /// <summary>
        /// purchase levels. best with index 0
        /// уровни покупок. лучшая с индексом 0
        /// </summary>
        public SortedList<decimal, decimal> Bids { get; set; } = new SortedList<decimal, decimal>();


        public MarketDepth GetMarketDepth(decimal bidPrice, decimal askPrice)
        {
            MarketDepth marketDepth = new MarketDepth();
            marketDepth.IsinId = IsinId;

            marketDepth.DateTime = DateTime;

            if (askPrice > 0)
            {
                ClearKeyForBids(askPrice);
            }

            if (bidPrice > 0)
            {
                ClearKeyForAsks(bidPrice);
            }

            foreach (var ask in Asks)
            {
                marketDepth.Asks.Add(new MarketDepthLevel()
                {
                    Price = ask.Key,
                    Volume = ask.Value,
                });
            }

            foreach (var bid in Bids)
            {
                marketDepth.Bids.Insert(0, new MarketDepthLevel()
                {
                    Price = bid.Key,
                    Volume = bid.Value,
                });
            }

            return marketDepth;
        }

        private void ClearKeyForBids(decimal price)
        {
            while (Bids.Count > 0
                    && price < Bids.Keys.Last())
            {
                decimal key = Bids.Last().Key;

                Bids.Remove(key);
            }
        }

        private void ClearKeyForAsks(decimal price)
        {
            while (Asks.Count > 0
                    && price > Asks.Keys.First())
            {
                decimal key = Asks.First().Key;

                Asks.Remove(key);
            }
        }

        public MarketDepth GetMarketDepth(decimal bidPrice, decimal askPrice, ObjectPool<MarketDepth> pool)
        {
            MarketDepth marketDepth = pool.Get();
            marketDepth.IsinId = IsinId;

            marketDepth.DateTime = DateTime.Now;

            //var asks = Asks.GetSortedItems();

            foreach (KeyValuePair<decimal, decimal> item in Asks)
            {
                MarketDepthLevel level = new MarketDepthLevel()
                {
                    Price = item.Key,
                    Volume = item.Value
                };

                marketDepth.Asks.Add(level);
            }

            //var bids = Bids.GetSortedItems();

            foreach (KeyValuePair<decimal, decimal> item in Bids)
            {
                MarketDepthLevel level = new MarketDepthLevel()
                {
                    Price = item.Key,
                    Volume = item.Value
                };

                marketDepth.Bids.Insert(0, level);
            }

            return marketDepth;
        }
    }
}

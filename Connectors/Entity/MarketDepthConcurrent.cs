using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class MarketDepthConcurrent
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
        public ConcurrentDictionary<decimal, decimal> Asks { get; set; } = new ConcurrentDictionary<decimal, decimal>();

        /// <summary>
        /// purchase levels. best with index 0
        /// уровни покупок. лучшая с индексом 0
        /// </summary>
        public ConcurrentDictionary<decimal, decimal> Bids { get; set; } = new ConcurrentDictionary<decimal, decimal>();


        public MarketDepth GetMarketDepth(decimal bidPrice, decimal askPrice)
        {
            MarketDepth marketDepth = new MarketDepth();
            marketDepth.IsinId = IsinId;

            marketDepth.DateTime = DateTime;

            //List<decimal> rem = new List<decimal>();

            foreach (var ask in Asks)
            {
                if (ask.Value > 0
                    && ask.Key > bidPrice)
                {
                    AddAsk(ask, marketDepth.Asks);
                }
                //else if (ask.Key <= bidPrice)
                //{
                //    rem.Add(ask.Key);
                //}
            }

            //foreach (var r in rem)
            //{
            //    Asks.Remove(r, out var val);
            //}

            //rem.Clear();

            foreach (var bid in Bids)
            {
                if (bid.Value > 0
                    && bid.Key < askPrice)
                {
                    AddBid(bid, marketDepth.Bids);  
                }
                //else if (bid.Key >= askPrice)
                //{
                //    rem.Add(bid.Key);
                //}
            }

            //foreach (var r in rem)
            //{
            //    Asks.Remove(r, out var val);
            //}

            return marketDepth;
        }

        private void AddBid(KeyValuePair<decimal, decimal> bid, List<MarketDepthLevel> bids)
        {
            if (bids.Count == 0
                || bids.Last().Price > bid.Key)
            {
                bids.Add(new MarketDepthLevel()
                {
                    Price = bid.Key,
                    Volume = bid.Value,
                });
                return;
            }
            else if (bids.Count > 1)
            {
                for (int i = 0; i < bids.Count - 1; i++)
                {
                    if (bids[i].Price > bid.Key
                        && bids[i+1].Price < bid.Key)
                    {
                        bids.Insert(i+1, new MarketDepthLevel()
                        {
                            Price = bid.Key,
                            Volume = bid.Value,
                        });
                        return;
                    }
                    else if (bids.First().Price < bid.Key)
                    {
                        bids.Insert(0, new MarketDepthLevel()
                        {
                            Price = bid.Key,
                            Volume = bid.Value,
                        });
                        return;
                    }
                }
            }
            else if (bids.Count > 0
                    && bids.First().Price < bid.Key)
            {
                bids.Insert(0, new MarketDepthLevel()
                {
                    Price = bid.Key,
                    Volume = bid.Value,
                });
                return;
            }
        }

        private void AddAsk(KeyValuePair<decimal, decimal> ask, List<MarketDepthLevel> asks)
        {
            if (asks.Count == 0
                || asks.Last().Price < ask.Key)
            {
                asks.Add(new MarketDepthLevel()
                {
                    Price = ask.Key,
                    Volume = ask.Value,
                });
                return;
            }
            else if (asks.Count > 1)
            {
                for (int i = 0; i < asks.Count - 1; i++)
                {
                    if (asks[i].Price < ask.Key
                        && asks[i + 1].Price > ask.Key)
                    {
                        asks.Insert(i+1, new MarketDepthLevel()
                        {
                            Price = ask.Key,
                            Volume = ask.Value,
                        });
                        return;
                    }
                    else if (asks.First().Price > ask.Key)
                    {
                        asks.Insert(0, new MarketDepthLevel()
                        {
                            Price = ask.Key,
                            Volume = ask.Value,
                        });
                        return;
                    }
                }
            }
            else if (asks.Count > 0
                    && asks.First().Price > ask.Key)
            {
                asks.Insert(0, new MarketDepthLevel()
                {
                    Price = ask.Key,
                    Volume = ask.Value,
                });
                return;
            }
        }

        private void ClearKeyForBids(decimal price)
        {
            while (Bids.Count > 0
                    && price < Bids.Keys.Last())
            {
                decimal key = Bids.Last().Key;

                Bids.Remove(key, out var val);
            }
        }

        private void ClearKeyForAsks(decimal price)
        {
            while (Asks.Count > 0
                    && price > Asks.Keys.First())
            {
                decimal key = Asks.First().Key;

                Asks.Remove(key, out var val);
            }
        }
    }
}

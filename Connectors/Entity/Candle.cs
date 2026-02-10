using ControllerExChanges.Enums;
using ControllerExChanges.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class Candle: ICandle
    {
        public Candle(TimeFrame timeFrame,
                        DateTime dateTime,
                        decimal open,
                        decimal high,
                        decimal low,
                        decimal close,
                        decimal volume)
        {
            SetDateTime(timeFrame, dateTime);

            _open = open;
            _high = high;
            _low = low;
            _close = close;
            _volume = volume;
        }
        
        
        public Candle(TimeFrame timeFrame,
                        Trade trade,
                        bool isCreateClusters,
                        decimal ask = decimal.MinValue,
                        decimal bid = decimal.MinValue )
        {
            SetDateTime(timeFrame, trade.DateTime);

            AddTick(trade, ask, bid);

            _open = trade.Price;
            _low = _open;
            _high = _open;
            _close = _open;

            if (ask == decimal.MinValue) _ask = _open;
            if (bid == decimal.MinValue) _bid = _open;

            _isCreateClusters = isCreateClusters;
        }

        #region Properties =====================================================================

        /// <summary>
        /// Все кластера, которые есть в свече
        /// </summary>
        public Dictionary<decimal, Cluster> Clusters
        {
            get => _clusters;
        }

        /// <summary>
        /// Время начала свечи
        /// </summary>
        public DateTime DateTime
        {
            get => _dateTime;

            set => _dateTime = value;
        }
        public decimal High
        {
            get => _high;

            set => _high = value;
        }


        public decimal Low
        {
            get => _low;

            set => _low = value;
        }


        public decimal Open
        {
            get => _open;

            set => _open = value;
        }


        public decimal Close
        {
            get => _close;

            set => _close = value;
        }


        public decimal Volume
        {
            get => _volume;

            set => _volume = value;
        }

        public decimal Ask
        {
            get => _ask;

            set => _ask = value;
        }


        public decimal Bid
        {
            get => _bid;

            set => _bid = value;
        }


        /// <summary>
        /// Кластер с максимальным объёмом в свече
        /// </summary>
        public Cluster? ClusterMaxVolume => _clusterMaxVolume;

        public TimeFrame TimeFrame { get ; set ; }


        public string IsinId { get ; set ; }

        public string NameSecurity { get; set; }

        #endregion


        #region Fields =========================================================================

        DateTime _dateTime = DateTime.MinValue;

        decimal _high = 0;

        decimal _low = 0;

        decimal _open = 0;

        decimal _close = 0;

        decimal _volume = 0;

        decimal _ask = 0;

        decimal _bid = 0;

        bool _isCreateClusters;

        Cluster? _clusterMaxVolume = null;               

        Dictionary<decimal, Cluster> _clusters = new Dictionary<decimal, Cluster>();

        /// <summary>
        /// Временной отрезок свечи
        /// </summary>
        TimeSpan _timeSpan = TimeSpan.FromMinutes(1);

        #endregion

        #region Methods =========================================================================

        /// <summary>
        /// Добавить обезличенную сделку
        /// </summary>
        /// <param name="trade"></param>
        public void AddTick(Trade trade, decimal ask = decimal.MinValue, decimal bid = decimal.MinValue)
        {
            if (_isCreateClusters)
            {
                Cluster? cluster = null;

                if (Clusters.TryGetValue(trade.Price, out cluster))
                {
                    cluster.AddTick(trade);
                }
                else
                {
                    cluster = new Cluster(trade);

                    Clusters.Add(trade.Price, cluster);
                }

                if (_clusterMaxVolume == null
                    || cluster.Sum > _clusterMaxVolume.Sum) _clusterMaxVolume = cluster;
            }
            

            _volume += trade.Volume;

            if (_high < trade.Price) _high = trade.Price;
            if (_low > trade.Price) _low = trade.Price;

            _close = trade.Price;            

            if (ask !=  decimal.MinValue) _ask = ask;
            if (bid  != decimal.MinValue) _bid = bid;
        }

        private void SetDateTime(TimeFrame timeFrame, DateTime dateTime)
        {
            DateTime start = dateTime.Date;

            int dt = (int)((dateTime - start).TotalMinutes/(int)timeFrame);

            //while (dateTime >= start.AddMinutes((int)timeFrame))
            //{
            //    start = start.AddMinutes((int)timeFrame);
            //}

            TimeFrame = timeFrame;

            _dateTime = start.AddMinutes(dt * (int)timeFrame);

            _timeSpan = TimeSpan.FromMinutes((int)timeFrame);
        }


        #endregion
    }
}

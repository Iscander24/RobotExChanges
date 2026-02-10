using ControllerExChanges.Enums;
using ControllerExChanges.Interfaces;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ControllerExChanges.Interfaces.IConnector;

namespace ControllerExChanges.Entity
{
    public class CandleManagerToSecurity:ICandleManagerToSecurity
    {
        /// <summary>
        /// Менеджер свечей для одного инструмента
        /// </summary>
        public CandleManagerToSecurity(ControllerLogger logger,
                                        Security security) 
        {
            _logger = logger.Logger.ForContext("Class", nameof(CandleManagerToSecurity));

            _security = security;

            _logger.Information("{@MethodName}, _security = {@security}", nameof(CandleManagerToSecurity), security);
        }


        #region Properties =======================================================================

        public List<Candle> BaseCandles => _baseCandles;

        public ConcurrentDictionary<TimeFrame, List<Candle>> DictionaryCandles => _dictionaryCandles;

        public Security Security => _security;

        #endregion


        #region Fields =========================================================================

        Security _security;

        List<Candle> _baseCandles = new List<Candle>();

        ConcurrentQueue<(bool, List<Candle>)> _candleQueue = new ConcurrentQueue<(bool, List<Candle>)>();

        /// <summary>
        /// Словарь списков свечей. Ключ => TimeFrame
        /// </summary>
        ConcurrentDictionary<TimeFrame, List<Candle>> _dictionaryCandles = new ConcurrentDictionary<TimeFrame, List<Candle>>();

        bool _isRun = false;

        ILogger _logger;

        #endregion



        #region Methods =========================================================================

        public async Task<bool> SubscribeToCandles(TimeFrame timeFrame = TimeFrame.Min1)
        {
            _logger.Information("{@MethodName}, Security {@Security} TimeFrame {@TimeFrame}", nameof(SubscribeToCandles), Security, timeFrame);

            if (!_dictionaryCandles.TryGetValue(timeFrame, out List<Candle>? candles))
            {
                return _dictionaryCandles.TryAdd(timeFrame, new List<Candle>());
            }           

            await Task.Delay(1);

            return true;
        }

        public async Task<bool> UnSubscribeToCandles(TimeFrame timeFrame)
        {
            if (EventCancelCandle == null
                && EventChangeCandle == null) _isRun = false;

            List<Candle>? candles = null;

            if (_dictionaryCandles.TryGetValue(timeFrame, out candles))
            {
                return _dictionaryCandles.TryRemove(timeFrame, out candles);
            }

            await Task.Delay(1);

            return true;
        }

        internal void SetHistoricalCandles(List<Candle> historicalCandles, TimeFrame timeFrame)
        {
            List<Candle>? candles = null;

            if (!_dictionaryCandles.TryGetValue(timeFrame, out candles))
            {
                candles = new List<Candle>();
            }

            foreach (var candle in historicalCandles)
            {
                if (candles.Count == 0
                     || candle.DateTime > candles.Last().DateTime) candles.Add(candle);
                else if(candle.DateTime < candles.First().DateTime) candles.Insert(0, candle);
                 
                else
                {
                    int ind = -1;
                    
                    for (int i=0; i< candles.Count - 1; i++)
                    {
                        if (candle.DateTime > candles[i].DateTime
                            && candle.DateTime < candles[i+1].DateTime)
                        {
                            ind = i + 1;
                            break;
                        }
                    }

                    if (ind >= 0) candles.Insert(ind, candle);
                }
            }
        }

        public void SetTrade(Trade trade, bool isCreateClusters, decimal ask = decimal.MinValue, decimal bid = decimal.MinValue)
        {
            if (_baseCandles.Count > 0)
            {
                Candle last = _baseCandles.Last();

                if (last.DateTime <= trade.DateTime)
                {
                    if (trade.DateTime < last.DateTime.AddMinutes(1))
                    {
                        last.AddTick(trade, ask, bid);
                    }
                    else
                    {
                        EventCancelCandle?.Invoke(Security, TimeFrame.Min1, _baseCandles);

                        _baseCandles.Add(new Candle(TimeFrame.Min1, trade, isCreateClusters, ask, bid));
                    }
                }                
            }
            else
            {
                _baseCandles.Add(new Candle(TimeFrame.Min1, trade, isCreateClusters));
            }

            EventChangeCandle?.Invoke(Security, TimeFrame.Min1, _baseCandles);

            if (_dictionaryCandles.Count > 0)
            {
                foreach (var val in _dictionaryCandles)
                {
                    if (val.Value.Count > 0)
                    {
                        Candle lastCandle = val.Value.Last();

                        if (lastCandle.DateTime <= trade.DateTime)
                        {
                            if (trade.DateTime < lastCandle.DateTime.AddMinutes((int)val.Key))
                            {
                                lastCandle.AddTick(trade, ask, bid);

                                EventChangeCandle?.Invoke(Security, val.Key, val.Value);
                            }
                            else
                            {
                                EventCancelCandle?.Invoke(Security, val.Key, val.Value);

                                val.Value.Add(new Candle(val.Key, trade, isCreateClusters, ask, bid));
                            }
                        }
                    }                    
                }
            }
        }

        private async void Transporter()
        {
            (bool, List<Candle>) item;

            while (_isRun)
            {
                if (_candleQueue.IsEmpty)
                {
                    await Task.Delay(1);
                }
                else
                {
                    if (_candleQueue.TryDequeue(out item))
                    {
                        if (item.Item1)
                        {
                            //EventCancelCandle?.Invoke(item.Item2);
                        }
                        else
                        {
                            //EventChangeCandle?.Invoke(item.Item2);
                        }
                    }
                }
            }
        }

        

        #endregion

        #region Events =========================================================================

        public event IConnector.eventCancelCandle? EventCancelCandle;
        public event IConnector.eventChangeCandle? EventChangeCandle;

        #endregion
    }
}

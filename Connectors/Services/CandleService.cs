using ControllerExChanges.Entity;
using ControllerExChanges.Enums;
using ControllerExChanges.Interfaces;
using Serilog;
using System.Collections.Concurrent;

namespace ControllerExChanges.Services
{
    public class CandleService : ICandleService
    {
        public CandleService(ControllerLogger logger)
        {
            _controllerLogger = logger;

            _logger = logger.Logger.ForContext("Class", nameof(CandleService));
        }

        #region Properties =======================================================================

        public ConcurrentDictionary<string, CandleManagerToSecurity> Managers => _managers;

        #endregion

        #region Fields =========================================================================

        ILogger _logger;

        ControllerLogger _controllerLogger;

        /// <summary>
        /// Словарь менеджеров управления свечами. Ключ => Security.IsinId
        /// </summary>
        ConcurrentDictionary<string, CandleManagerToSecurity> _managers = new ConcurrentDictionary<string, CandleManagerToSecurity>();

        #endregion

        #region Delegated Methods ===============================================================

        private getCandles? _getCandles;

        private subscribeToCandles? _subscribeToCandles;

        private unSubscribeToCandles? _unSubscribeToCandles;

        #endregion

        #region Public Methods ==================================================================

        public async Task<List<Candle>> GetCandles(Security security,
                                                    TimeFrame timeFrame,
                                                    int countCandles = 1440,
                                                    DateTime? timeStart = null,
                                                    DateTime? timeEnd = null,
                                                    Action<int>? LoadingInfo = null)
        {
            if (timeStart != null
                && timeEnd != null)
            {
                TimeSpan timeSpan = (TimeSpan)(timeEnd - timeStart);

                int count = (int)timeFrame;

                countCandles = (int)(timeSpan.TotalMinutes / count) ;
            }
            
            List<Candle>? candlesHist = new List<Candle>();

            if (_managers.TryGetValue(security.IsinId, out CandleManagerToSecurity? manager))
            {
                if (manager.DictionaryCandles.TryGetValue(timeFrame, out  candlesHist))
                {
                    if (candlesHist.Count == countCandles) return candlesHist;
                    else if (candlesHist.Count > countCandles)
                    {
                        List<Candle> candles = new List<Candle>();

                        for (int i= 0; i < candlesHist.Count; i++)
                        {
                            candles.Add(candlesHist[i]);
                        }

                        return candles;
                    }
                }
            }

            //===============================================================

            if (_getCandles == null)
            {
                _logger.Error("{@MethodName}, _getCandles == null", nameof(GetCandles));

                return new List<Candle>();
            }

            var res = await SubscribeToCandles(security, timeFrame, countCandles, LoadingInfo);            

            if (res)
            {
                if (_managers.TryGetValue(security.IsinId, out CandleManagerToSecurity? manager2))
                {
                    if (manager2.DictionaryCandles.TryGetValue(timeFrame, out  candlesHist))
                    {
                        if (candlesHist.Count >= countCandles) return candlesHist.GetRange(candlesHist.Count- countCandles, countCandles);
                    }
                }
            }

            return candlesHist == null || candlesHist.Count == 0 ?  new List<Candle>() : candlesHist;
        }

        public async Task<bool> SubscribeToCandles(Security security, 
                                                    TimeFrame timeFrame = TimeFrame.Min1,
                                                    int countCandles = 1440,
                                                    Action<int>? LoadingInfo = null)
        {
            CandleManagerToSecurity? manager = null;

            bool res = false;

            _logger.Information("{@MethodName}, Security {@Security}", nameof(SubscribeToCandles), security);

            if (_subscribeToCandles != null) await _subscribeToCandles(security, timeFrame);

            if (_managers.TryGetValue(security.IsinId, out manager))
            {
                res = await manager.SubscribeToCandles(timeFrame);

                if (res)
                {
                    if (_getCandles != null)
                    {
                        List<Candle> candles = await _getCandles(security, timeFrame, countCandles, LoadingInfo);

                        if (candles.Count > 0)
                        {
                            manager.SetHistoricalCandles(candles, timeFrame);
                        }
                    }
                }
            }
            else
            {
                manager = new CandleManagerToSecurity(_controllerLogger, security);
                manager.EventChangeCandle += Manager_EventChangeCandle;
                manager.EventCancelCandle += Manager_EventCancelCandle;
                bool res1 = await manager.SubscribeToCandles(timeFrame);

                if (res1)
                {
                    if (_getCandles != null)
                    {
                        List<Candle> candles = await _getCandles(security, timeFrame, countCandles, LoadingInfo);

                        if (candles.Count > 0)
                        {
                            manager.SetHistoricalCandles(candles, timeFrame);
                        }
                    }                    
                }

                res = _managers.TryAdd(security.IsinId, manager) && res1;
            }

            _logger.Information("{@MethodName}, res {@Res}", nameof(SubscribeToCandles), res);

            return res;
        }

        public async Task<bool> UnSubscribeToCandles(Security security, TimeFrame timeFrame)
        {
            if (_unSubscribeToCandles != null) await _unSubscribeToCandles(security, timeFrame);

            return true;
        }

        public void SetTrade(Trade trade, bool isCreateClusters, decimal ask = decimal.MinValue, decimal bid = decimal.MinValue)
        {
            if (_managers.TryGetValue(trade.IsinId, out CandleManagerToSecurity? manager))
            {
                manager.SetTrade(trade, isCreateClusters, ask, bid);
            }
        }

        #endregion

        #region Private Methods ==================================================================

        internal void DelegatesInit(getCandles getCandles,
                                    subscribeToCandles subscribeToCandles,
                                    unSubscribeToCandles unSubscribeToCandles)
        {
            _getCandles = getCandles;

            _subscribeToCandles = subscribeToCandles;

            _unSubscribeToCandles = unSubscribeToCandles;
        }

        private void Manager_EventCancelCandle(Security security, TimeFrame timeFrame, List<Candle> candles)
        {
            EventCancelCandle?.Invoke(security, timeFrame, candles);
        }

        private void Manager_EventChangeCandle(Security security, TimeFrame timeFrame, List<Candle> candles)
        {
            EventChangeCandle?.Invoke(security, timeFrame, candles);
        }

        

        #endregion

        #region Events =========================================================================

        public event IConnector.eventCancelCandle? EventCancelCandle;
        public event IConnector.eventChangeCandle? EventChangeCandle;

        #endregion

        #region Delegates ======================================================================

        public delegate Task<List<Candle>> getCandles(Security security, TimeFrame timeFrame, int countCandles, Action<int>? LoadingInfo = null);

        public delegate Task<bool> subscribeToCandles(Security security, TimeFrame timeFrame = TimeFrame.Min1);

        public delegate Task<bool> unSubscribeToCandles(Security security, TimeFrame timeFrame);

        #endregion
    }
}

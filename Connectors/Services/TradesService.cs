
using ControllerExChanges.Entity;
using ControllerExChanges.Interfaces;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ControllerExChanges.Interfaces.IConnector;

namespace ControllerExChanges.Services
{
    public class TradesService : ITradesService
    {
        /// <summary>
        /// Сервис для обработки обезличенных сделок
        /// </summary>
        /// <param name="logger"></param>
        public TradesService(ControllerLogger logger)
        {
            _logger = logger.Logger.ForContext<TradesService>();

            Init();
        }

        ~TradesService()
        {
            _isRun = false;
        }

        #region Properties ================================================================================

        public ConcurrentDictionary<string, List<Trade>> KeyTrades => _keyTrades;

        #endregion

        #region Fields ===================================================================================

        ConcurrentDictionary<string, List<Trade>> _keyTrades = new ConcurrentDictionary<string, List<Trade>>();

        /// <summary>
        /// Новые сделки в конвеере, ожидающие отправки потребителям через событие
        /// </summary>
        ConcurrentQueue<Trade> _newTrades = new ConcurrentQueue<Trade>();

        ILogger _logger;

        bool _isRun = true;

        #endregion

        #region Methods ==================================================================================

        public void SetSaveTrades(bool saveTrades)
        {
            _isRun = saveTrades;

            _logger.Information("{@MethodName}, _isRun = {@IsRun}", nameof(SetSaveTrades), _isRun);

            if (_isRun) Init();
        }


        public List<Trade>? GetListTradesFromIsinId(string isinId)
        {
            List<Trade>? tradeList = null;

            _keyTrades.TryGetValue(isinId, out tradeList);

            if (tradeList == null)
            {
                tradeList = new List<Trade>();

                if (_keyTrades.TryAdd(isinId, tradeList))
                {
                    return tradeList;
                }
            }
            else return tradeList;

            return null;            
        }

        public void SetTrade(Trade trade)
        {
            if (_isRun)
            {
                List<Trade>? tradeList = null;

                object o = new();

                if (_keyTrades.TryGetValue(trade.IsinId, out tradeList))
                {
                    lock (o)
                    {
                        if (tradeList.Count > 0)
                        {
                            Trade trd = tradeList.Last();

                            if (trd.DateTime.Ticks > trade.DateTime.Ticks
                                || trd.Number == trade.Number) return;
                        }

                        tradeList.Add(trade);
                    }
                }
                else
                {
                    tradeList = new List<Trade>() { trade };

                    _keyTrades.AddOrUpdate(trade.IsinId, tradeList, (key, value) => value = tradeList);
                }

                _newTrades.Enqueue(trade);
            }
            else
            {
                LastNewTrade?.Invoke(trade);
            }            
        }

        public void SetHistoryTrade(Trade trade)
        {
            List<Trade>? tradeList = null;

            if (_keyTrades.TryGetValue(trade.IsinId, out tradeList))
            {
                if (tradeList.Count > 0
                    && tradeList[0].Number <= trade.Number) return;

                tradeList.Add(trade);
            }
            else
            {
                tradeList = new List<Trade>() { trade };

                _keyTrades.AddOrUpdate(trade.IsinId, tradeList, (key, value) => value = tradeList);
            }

            _newTrades.Enqueue(trade);
        }

        /// <summary>
        /// Конвейер для обезличенных сделок в отдельном потоке
        /// </summary>
        private async void Transporter()
        {
            while (_isRun)
            {
                if (_newTrades.TryDequeue(out Trade trade))
                {
                    try
                    {
                        NewTradeEvent?.Invoke(trade);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("{@MethodName}, Exception = {@Exception}", nameof(Transporter), ex);

                        await Task.Delay(500);

                        _newTrades.Enqueue(trade);
                    }
                }

                await Task.Delay(1);
            }

            if (_isRun)
            {
                Init();
            }
        }

        private void Init()
        {
            Task.Run(() =>
            {
                Transporter();
            });
        }

        #endregion

        #region Events =========================================================================

        public event newTradeEvent? NewTradeEvent;

        public event newTradeEvent? LastNewTrade;

        #endregion
    }
}

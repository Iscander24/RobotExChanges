// Ignore Spelling: Paraments

using ControllerExChanges.Entity;
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


namespace ControllerExChanges.Connectors
{
    internal abstract class BaseConnector
    {
        internal BaseConnector(ICandleService candleService,
                                ISecuritiesService securitiesService,
                                IPortfoliosService portfoliosService,
                                ITradesService tradesService,
                                IOrdersService ordersService,
                                ConnectParaments connectParaments,
                                ControllerLogger logger,
                                string nameClass)
        {
            _nameClass = nameClass;

            _logger = logger.Logger.ForContext("Class", nameClass);

            _securitiesService = securitiesService;
            _securitiesService.SecuritiesChangeEvent += _securitiesService_SecuritiesChangeEvent;

            _portfoliosService = portfoliosService;
            _portfoliosService.PortfoliosChangeEvent += _portfoliosService_PortfoliosChangeEvent;

            _connectParaments = connectParaments;
            _connectParaments.SetParamentsEvent += _connectParaments_SetParamentsEvent;

            _tradesService = tradesService;
            _tradesService.NewTradeEvent += _tradesService_NewTradeEvent;
            _tradesService.LastNewTrade += _tradesService_LastNewTrade;

            _ordersService = ordersService;
            _ordersService.NewOrderEvent += _ordersService_NewOrderEvent;
            _ordersService.NewMyTradeEvent += _ordersService_NewMyTradeEvent;
            _ordersService.EventMessage += OnNewMessage;

            _candleService = candleService;
            _candleService.EventChangeCandle += _candleService_EventChangeCandle;
            _candleService.EventCancelCandle += _candleService_EventCancelCandle;

            DelegatesInit();

            Task.Run(() =>
            {
                Transporter();
            });
        }

        

        ~BaseConnector()
        {
            _isRun = false;
        }

        #region Properties ============================================================================================

        public ExchangeType ExchangeType => _exchangeType;

        public ConnectParaments ConnectParaments => _connectParaments;

        public ConnectStatus ConnectStatus => _connectStatus;

        public DateTime ConnectorTime => _connectorTime;

        public IPortfoliosService PortfoliosService => _portfoliosService;

        public ITradesService TradesService => _tradesService;

        public ISecuritiesService SecuritiesService => _securitiesService;

        public IOrdersService OrdersService => _ordersService;

        public ICandleService CandleService => _candleService;

        public bool IsCreateClusters => _isCreateClusters;

        //public ICommandsService? CommandsService => _commandsService;

        #endregion

        #region Fields =============================================================================================

        protected ILogger _logger;

        //protected readonly ObjectPool<MarketDepth> _poolMarketDepth = new DefaultObjectPool<MarketDepth>(new MarketDepthPoolPolicy());
        //protected readonly ObjectPool<MarketDepthSC> _poolMarketDepthSC = new DefaultObjectPool<MarketDepthSC>(new MarketDepthSCPoolPolicy());

        bool _isRun = true;

        string _nameClass = string.Empty;

        /// <summary>
        /// Тип коннектора
        /// </summary>
        protected ExchangeType _exchangeType = ExchangeType.None;

        /// <summary>
        /// Параметры подключения к бирже
        /// </summary>
        protected ConnectParaments _connectParaments;

        /// <summary>
        /// Статус подключения к бирже
        /// </summary>
        protected ConnectStatus _connectStatus = ConnectStatus.Disconnect;


        /// <summary>
        /// Время на бирже
        /// </summary>
        protected DateTime _connectorTime = DateTime.Now;

        /// <summary>
        /// Портфели с данного подключения
        /// </summary>
        protected IPortfoliosService _portfoliosService;

        /// <summary>
        /// Обезличенные сделки с данного подключения
        /// </summary>
        protected ITradesService _tradesService;

        /// <summary>
        /// Ордера на этой бирже
        /// </summary>
        protected IOrdersService _ordersService;

        /// <summary>
        /// Все инструменты на этой бирже. Ключ - IsinId
        /// </summary>
        protected ISecuritiesService _securitiesService;

        /// <summary>
        /// Сервис управления свечами
        /// </summary>
        protected ICandleService _candleService;

        /// <summary>
        /// Последнее время обновления биржевой информации
        /// Нужно для проверки соединения
        /// </summary>
        protected DateTime LastTimeUpDate = DateTime.MinValue;

        /// <summary>
        /// Сервис, который отправляет запросы пользователя на биржу
        /// </summary>
        //ICommandsService? _commandsService;

        private bool _isNewMarketDepth = false;

        /// <summary>
        /// queue of new depths 
        /// очередь новых стаканов
        /// </summary>
        protected ConcurrentDictionary<string, NewMarketDepth> _marketDepthsToSend = new ConcurrentDictionary<string, NewMarketDepth>();


        /// <summary>
        /// Бумаги, с которыми работает коннектор
        /// </summary>
        private List<string> _workSecurities = new List<string>();

        /// <summary>
        /// Сохранять кластера
        /// </summary>
        private bool _isCreateClusters = false;

        Random _random = new Random();

        

        #endregion

        #region Public Methods =====================================================================================

        public void SetCreateClusters(bool isCreateClusters)
        {
            _isCreateClusters = isCreateClusters;
        }

        public async Task<bool> AddSecurityToSubscription(Security security)
        {
            return await AddWorkSecurity(security);
        }

        public async Task<bool> AddSecurityToSubscriptionMarketDepth(Security security)
        {
            //await AddWorkSecurity(security);

            return await SubscribeToSecurityMarketDepth(security); ;
        }

        public async Task<bool> AddSecurityToSubscriptionCandles(Security security, TimeFrame timeFrame = TimeFrame.Min1)
        {
            await AddWorkSecurity(security);

            return await subscribeToCandles(security, timeFrame);
        }

        public async Task<bool> RemoveSecurityFromSubscription(Security security)
        {
            if (_workSecurities.Find(item => item == security.IsinId) == null) return true;

            _workSecurities.Remove(security.IsinId);

            bool res = await UnsubscribeToSecurity(security);

            return res;
        }        

        public string GetComment(string portfolioNumber)
        {
            DateTime dt = DateTime.Now;

            string comment = _random.Next(1, 100).ToString() + dt.Hour.ToString() + dt.Minute.ToString() + dt.Second.ToString() + dt.Millisecond.ToString();

            return comment;
        }

        #endregion

        #region Private Methods =====================================================

        /// <summary>
        /// Добавить бумагу в список рабочих. Возврат false - бумага уже добавлена ранее. True - бумага добавлена сейчас.
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        private async Task<bool> AddWorkSecurity(Security security)
        {
            //if (_workSecurities.Find(item => item == security.IsinId) != null) return true;

            bool res = await SubscribeToSecurity(security);

            if (res
                && !string.IsNullOrEmpty(security.IsinId))
            {
                _workSecurities.Add(security.IsinId);
                return true;
            }          

            return false;
        }

        private async void Transporter()
        {
            while (_isRun)
            {
                foreach (var depth in _marketDepthsToSend.Values)
                {
                    if (depth.IsNew)
                    {
                        depth.IsNew = false;

                        try
                        {
                            NewMarketDepthEvent?.Invoke(depth.MarketDepth);

                            //depth.MarketDepth.Bids.Clear();
                            //depth.MarketDepth.Asks.Clear();

                            //_poolMarketDepth.Return(depth.MarketDepth);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("{@MethodName}, Exception = {@Exception}", nameof(Transporter), ex);
                        }
                    }
                }

                if (LastTimeUpDate.AddSeconds(15) < DateTime.Now
                    && _workSecurities.Count > 0
                    && _connectStatus == ConnectStatus.Connect)
                {
                    _connectStatus = ConnectStatus.Disconnect;

                    AutoReconnect();

                    ConnectStatusChangeEvent?.Invoke(_connectStatus);

                    _logger.Information("{@MethodName}, ConnectStatusChangeEvent = {@ConnectStatusChangeEvent}", nameof(Transporter), _connectStatus);

                    OnNewMessage(new Message(title: "ConnectStatus",
                                                text: _connectStatus.ToString(),
                                                exchangeType: ExchangeType));
                }

                await Task.Delay(5);
            }
        }

        internal void SetNewMarketDepth(MarketDepth marketDepth)
        {
            if (NewMarketDepthEvent != null
                && marketDepth.Bids.Count > 0
                && marketDepth.Asks.Count > 0)
            {
                NewMarketDepth newMarketDepth = new NewMarketDepth(marketDepth);

                _marketDepthsToSend.AddOrUpdate(marketDepth.IsinId, newMarketDepth, (key, value) => value = newMarketDepth);
            }

            LastTimeUpDate = DateTime.Now;
        }

        protected async Task ReConnect()
        {

            for (int i = 0; i < _workSecurities.Count; i++)
            {
                Security? security = _securitiesService.GetSecurityForIsinId(_workSecurities[i]);

                if (security != null)
                {
                    bool res = await SubscribeToSecurity(security);
                }
            }
        }

        private void AutoReconnect()
        {
           
            Task.Run(async () =>
            {
                int time = 2000;
                
                while (_isRun)
                {
                    await Task.Delay(time);

                    if (ConnectStatus == ConnectStatus.Connect) break;
                    
                    if (time > 10000)
                    {
                        _logger.Information("{@MethodName}, AutoReconnect = false {@AutoReconnect}", nameof(Transporter), time);

                        OnNewMessage(new Message(title: "ConnectStatus",
                                                    text: "Не удалось соедениться с биржей...",
                                                    exchangeType: ExchangeType));
                    }

                    time += 2000;

                    await Connect();
                }
            });
        }

        public abstract Task<ConnectStatus> Connect();

        private void _securitiesService_SecuritiesChangeEvent(ConcurrentDictionary<string, Security> securities)
        {
            SecuritiesChangeEvent?.Invoke(securities);

            _logger.Information("{@MethodName}, _securitiesService.Securities.count {Count}", 
                nameof(_securitiesService_SecuritiesChangeEvent), _securitiesService.Securities.Count);
        }

        private void _portfoliosService_PortfoliosChangeEvent(Dictionary<string, Portfolio> portfolios)
        {
            PortfoliosChangeEvent?.Invoke(portfolios);
        }

        private void _tradesService_NewTradeEvent(Trade trade)
        {
            NewTradeEvent?.Invoke(trade);

            //if (_marketDepthsToSend.TryGetValue(trade.IsinId, out NewMarketDepth? marketDepth))
            //{
            //    _candleService.SetTrade(trade, marketDepth.MarketDepth.Ask.Price, marketDepth.MarketDepth.Bid.Price);
            //}
            //else
            //{
            //    _candleService.SetTrade(trade);
            //}
            //
            _candleService.SetTrade(trade, IsCreateClusters);

            LastTimeUpDate = DateTime.Now;
        }

        private void _tradesService_LastNewTrade(Trade trade)
        {
            LastTradeEvent?.Invoke(trade);

            _candleService.SetTrade(trade, IsCreateClusters);

            LastTimeUpDate = DateTime.Now;
        }

        private void _ordersService_NewMyTradeEvent(MyTrade myTrade)
        {
            NewMyTradeEvent?.Invoke(myTrade);
        }

        private void _ordersService_NewOrderEvent(Order order)
        {
            NewOrderEvent?.Invoke(order);
        }

        private void _candleService_EventCancelCandle(Security security, TimeFrame timeFrame, List<Candle> candles)
        {
            EventCancelCandle?.Invoke(security, timeFrame, candles);
        }

        private void _candleService_EventChangeCandle(Security security, TimeFrame timeFrame, List<Candle> candles)
        {
            EventChangeCandle?.Invoke(security, timeFrame, candles);
        }

        private void _connectParaments_SetParamentsEvent(ConnectParaments paraments)
        {
            SetParamentsEvent?.Invoke(paraments);
        }

        /// <summary>
        /// Инициализация делегатов для сервисов
        /// </summary>
        private void DelegatesInit()
        {
            //((Services.CandleService)CandleService).DelegatesInit(getCandles, subscribeToCandles, unSubscribeToCandles);
        }

        protected virtual void OnNewMessage(Message message)
        {
            EventMessage?.Invoke(message);
        }

        protected void OnConnectStatusChangeEvent(ConnectStatus status)
        {
            ConnectStatusChangeEvent?.Invoke(status);
        }

        #endregion

        #region Abstract Methods =====================================================
        // Abstract Methods переопределяются для каждого коннектора и далее 
        // передаются делегатам в сервисы

        protected abstract Task<List<Candle>> getCandles(Security security, TimeFrame timeFrame, int countCandles, Action<int>? LoadingInfo = null);

        protected abstract Task<bool> subscribeToCandles(Security security, TimeFrame timeFrame = TimeFrame.Min1);

        protected abstract Task<bool> unSubscribeToCandles(Security security, TimeFrame timeFrame);


        protected abstract Task<bool> SubscribeToSecurity(Security security);

        //protected abstract Task<bool> SubscribeToSecurityPrice(Security security);

        protected abstract Task<bool> SubscribeToSecurityMarketDepth(Security security);

        protected abstract Task<bool> UnsubscribeToSecurity(Security security);

        #endregion

        #region Events ===============================================================

        public event IConnector.newTradeEvent? NewTradeEvent;
        public event IConnector.newTradeEvent? LastTradeEvent;
        public event IConnector.newOrderEvent? NewOrderEvent;
        public event IConnector.newMyTradeEvent? NewMyTradeEvent;
        public event IConnector.setParamentsEvent? SetParamentsEvent;
        public event IConnector.securitiesChangeEvent? SecuritiesChangeEvent;
        public event IConnector.portfoliosChangeEvent? PortfoliosChangeEvent;
        public event IConnector.eventCancelCandle? EventCancelCandle;
        public event IConnector.eventChangeCandle? EventChangeCandle;
        public event IConnector.newMarketDepthEvent? NewMarketDepthEvent;
        public event IConnector.connectStatusChangeEvent? ConnectStatusChangeEvent;

        /// <summary>
        /// Сообщение от биржи
        /// </summary>
        public event eventMessage? EventMessage;
        #endregion
    }
}

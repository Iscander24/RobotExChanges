//using Binance.Net;
//using Binance.Net.Clients;
//using Binance.Net.Enums;
//using Binance.Net.Interfaces;
//using Binance.Net.Objects.Models;
//using Binance.Net.Objects.Models.Futures;
//using Binance.Net.Objects.Models.Futures.Socket;
//using Binance.Net.Objects.Models.Spot.Socket;
//using Binance.Net.Objects.Options;
//using ControllerExChanges.Entity;
//using ControllerExChanges.Enums;
//using ControllerExChanges.Interfaces;
//using ControllerExChanges.Services;
//using CryptoExchange.Net.Authentication;
//using CryptoExchange.Net.Objects;
//using CryptoExchange.Net.Objects.Sockets;
//using Microsoft.Extensions.Logging;
//using Serilog;
//using System.Collections.Concurrent;
//using OrderStatus = ControllerExChanges.Enums.OrderStatus;

//namespace ControllerExChanges.Connectors.BinanceFutures
//{
//    internal class BinanceFuturesConnector : BaseConnector, IConnector
//    {
//        public BinanceFuturesConnector(ControllerLogger logger,
//                                ICandleService candleService,
//                                ISecuritiesService securitiesService,
//                                IPortfoliosService portfoliosService,
//                                ConnectParaments connectParaments,
//                                ITradesService tradesService,
//                                IOrdersService ordersService) : base(candleService,
//                                                                     securitiesService,
//                                                                     portfoliosService,
//                                                                     tradesService,
//                                                                     ordersService,
//                                                                     connectParaments,
//                                                                     logger,
//                                                                     nameof(BinanceFuturesConnector))
//        {
//            _exchangeType = Enums.ExchangeType.BinanceFutures;

//            _loggerFactory = new LoggerFactory().AddSerilog(logger.Logger);

//            _connectParaments.ListParaments.Add(new ParameterRow(typeof(string), _apiKeyName));
//            _connectParaments.ListParaments.Add(new ParameterRow(typeof(string), _apiSecretName, true));
//            _connectParaments.BaseCurrency = "USDT";
//            _connectParaments.ExchangeType = _exchangeType;
//        }

//        #region Properties ===========================================================


//        #endregion Properties

//        #region Public Fields ========================================================



//        #endregion Public Fields

//        #region private Fields =======================================================

//        BinanceSocketClient? _socketClient;

//        ILoggerFactory _loggerFactory;

//        /// <summary>
//        /// Словарь подписок на инструменты. Ключ - IsinId
//        /// </summary>
//        ConcurrentDictionary<string, UpdateSubscription?> _updateSubscriptions = new ConcurrentDictionary<string, UpdateSubscription?>();

//        string _referal = "x-4WAWQ7U3";

//        string _accountName = "Futures";

//        const string _apiKeyName = "ApiKey";
//        const string _apiSecretName = "ApiSecret";

//        string _apiKey = string.Empty;
//        string _apiSecret = string.Empty;

//        DateTime _lastMarketDepth = DateTime.MinValue;

//        Random _random = new Random();

//        ConcurrentDictionary<string, MarketDepthSC> _marketDepth = new ConcurrentDictionary<string, MarketDepthSC>();

//        #endregion private Fields

//        #region Commands =============================================================


//        #endregion Commands

//        #region Public Methods =======================================================

//        public override async Task<ConnectStatus> Connect()
//        {
//            if (!_connectParaments.IsReadyParaments())
//            {
//                //TODO Вставить локализацию в сообщения

//                _logger.Warning("MethodName {@MethodName}, ConnectParaments IsEmpty ", nameof(Connect));

//                return ConnectStatus.Disconnect;
//            }

//            LastTimeUpDate = DateTime.Now;

//            _apiKey = _connectParaments.ListParaments.Find(row => row.ParameterName == _apiKeyName)?.Value?.ToString() ?? string.Empty;

//            _apiSecret = _connectParaments.ListParaments.Find(row => row.ParameterName == _apiSecretName)?.Value?.ToString() ?? string.Empty;

//            if (_socketClient != null)
//            {
//                _socketClient.Dispose();
//                _socketClient = null;
//            }


//            try
//            {
//                _socketClient = new BinanceSocketClient(options =>
//                {
//                    options.OutputOriginalData = true;
//                    options.ApiCredentials = new ApiCredentials(_apiKey, _apiSecret);
//                    options.Environment = BinanceEnvironment.Live;
//                });

//                using (var client = new BinanceRestClient(options => GetOptions(options)))
//                {
//                    WebCallResult<string> startOkay = await client.UsdFuturesApi.Account.StartUserStreamAsync();

//                    if (!startOkay.Success)
//                    {
//                        _logger.Error("MethodName {@MethodName}, ! startOkay{@Data}", nameof(Connect), startOkay.Error);

//                        OnNewMessage(new Message(title: "Error",
//                                                text: startOkay.Error?.Message.Substring(0, 100) ?? "",
//                                                exchangeType: ExchangeType));

//                        return ConnectStatus.Disconnect;
//                    }

//                    //CallResult<UpdateSubscription> subOkay = await _socketClient.UsdFuturesApi.SubscribeToUserDataUpdatesAsync(startOkay.Data,
//                    //    onLeverageUpdate: OnLeverageUpdate,
//                    //    onMarginUpdate: OnMarginUpdate,
//                    //    onAccountUpdate:OnAccountUpdate,
//                    //    onOrderUpdate:OnOrderUpdate,
//                    //    onListenKeyExpired: OnListenKeyExpired,
//                    //    onStrategyUpdate: OnStrategyUpdate,
//                    //    onGridUpdate: OnGridUpdate);


//                    CallResult<UpdateSubscription> subOkay = await _socketClient.UsdFuturesApi.SubscribeToUserDataUpdatesAsync(startOkay.Data,
//                        onLeverageUpdate: OnLeverageUpdate,
//                        onMarginUpdate: OnMarginUpdate,
//                        onAccountUpdate: OnAccountUpdate,
//                        onOrderUpdate: OnOrderUpdate,
//                        onListenKeyExpired: OnListenKeyExpired,

//                        null);

//                    if (!subOkay.Success)
//                    {
//                        _logger.Error("MethodName {@MethodName}, Error.Message{@Message} ", nameof(Connect), subOkay.Error?.Message);
//                        OnNewMessage(new Message(title: "Error",
//                                                text: subOkay.Error?.Message ?? "",
//                                                exchangeType: ExchangeType));
//                        return ConnectStatus.Disconnect;
//                    }


//                    _logger.Information("MethodName {@MethodName}, ConnectStatus.Connect ", nameof(Connect));

//                    _connectStatus = ConnectStatus.Connect;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("MethodName {@MethodName}, Exception {@Exception} ", nameof(Connect), ex);

//                OnNewMessage(new Message(title: "Error",
//                                                text: ex.Message,
//                                                exchangeType: ExchangeType));
//                _connectStatus = ConnectStatus.Disconnect;
//            }

//            OnNewMessage(new Message(title: "ConnectStatus",
//                                                text: _connectStatus.ToString(),
//                                                exchangeType: ExchangeType));

//            OnConnectStatusChangeEvent(_connectStatus);
//            //ConnectStatusChangeEvent?.Invoke(_connectStatus);

//            var _ = Task.Run(async () =>
//            {
//                await GetPortfolios();
//                await GetSecurities();
//                await GetOrders();
//                await GetAllPositions();

//                await ReConnect();
//            });

//            return _connectStatus;
//        }

//        public async Task Disconnect()
//        {
//            await Dispose();
//        }

//        public async Task Dispose()
//        {
//            if (_socketClient == null) return;

//            await _socketClient.UnsubscribeAllAsync();

//            _socketClient.Dispose();

//            _connectStatus = ConnectStatus.Disconnect;

//            OnNewMessage(new Message(title: "ConnectStatus",
//                                                text: _connectStatus.ToString(),
//                                                exchangeType: ExchangeType));

//            OnConnectStatusChangeEvent(_connectStatus);
//            //ConnectStatusChangeEvent?.Invoke(_connectStatus);
//        }

//        public List<TimeFrame> GetTimeFrames()
//        {
//            return new List<TimeFrame>()
//            {
//                TimeFrame.Min1,
//                TimeFrame.Min2,
//                TimeFrame.Min3,
//                TimeFrame.Min5,
//                TimeFrame.Min15,
//                TimeFrame.Min30,
//                TimeFrame.Hour1,
//                TimeFrame.Hour2,
//                TimeFrame.Hour4,
//                TimeFrame.Day
//            };
//        }

//        protected override async Task<bool> SubscribeToSecurity(Security security)
//        {
//            if (_socketClient == null) return false;

//            try
//            {
//                await Task.Delay(_random.Next(100));

//                if (!_updateSubscriptions.TryGetValue(security.IsinId, out UpdateSubscription? updateSubscription))
//                {
//                    _updateSubscriptions.TryAdd(security.IsinId, null);

//                    _logger.Information("Method{@Method}, Security{@Security}", nameof(SubscribeToSecurity), security);

//                    var tradesOk = await _socketClient.UsdFuturesApi.SubscribeToTradeUpdatesAsync(security.Name, OnUpdateTrades);

//                    if (tradesOk.Success)
//                    {
//                        _logger.Information("MethodName {@MethodName}, Security {@Security} success!", nameof(SubscribeToSecurity), security);

//                        _updateSubscriptions.AddOrUpdate(security.IsinId, tradesOk.Data, (key, value) => value = tradesOk.Data);

//                        _logger.Information("Method{@Method}, Security{@Security}, Id{@Id}", nameof(SubscribeToSecurity), security.Name, tradesOk.Data.Id);

//                        using (var client = new BinanceRestClient(options => GetOptions(options)))
//                        {
//                            var info = await client.UsdFuturesApi.Account.GetPositionInformationAsync(security.Name);

//                            if (info.Success)
//                            {
//                                List<BinancePositionDetailsUsdt> positions = info.Data.ToList();

//                                _logger.Information("MethodName {@MethodName}, positions{@BinancePositionDetailsUsdt} success!", nameof(SubscribeToSecurity), positions);

//                                foreach (var pos in positions)
//                                {
//                                    if (pos.Symbol == security.Name
//                                        && pos.Quantity != 0)
//                                    {
//                                        await GetOrdersAndTradesForSymbol(client, security.Name, pos.Quantity);
//                                        break;
//                                    }
//                                }
//                            }
//                        }

//                        return true;
//                    }
//                    else
//                    {
//                        _logger.Error("Method{@Method}, Error{@Error}", nameof(SubscribeToSecurity), tradesOk.Error);
//                        OnNewMessage(new Message(title: "Error",
//                                                    text: tradesOk?.Error?.Message ?? "Error",
//                                                    exchangeType: ExchangeType));

//                        _updateSubscriptions.Remove(security.IsinId, out var val);
//                    }
//                }
//                else return true;


//                _logger.Information("MethodName {@MethodName}, Security {@Security} failure///", nameof(SubscribeToSecurity), security);
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("MethodName {@MethodName}, Exception {@Exception}", nameof(SubscribeToSecurity), ex);

//                OnNewMessage(new Message(title: "Error",
//                                            text: ex.Message,
//                                            exchangeType: ExchangeType));
//            }


//            return false;
//        }

//        protected override async Task<bool> UnsubscribeToSecurity(Security security)
//        {
//            if (_updateSubscriptions.TryGetValue(security.IsinId, out UpdateSubscription? updateSubscription))
//            {
//                if (_socketClient != null) await _socketClient.UnsubscribeAsync(updateSubscription);

//                _updateSubscriptions.Remove(security.IsinId, out var val);

//                _logger.Information("MethodName {@MethodName}, Security {@Security} success!", nameof(UnsubscribeToSecurity), security);
//            }

//            if (_updateSubscriptions.TryGetValue(security.IsinId + "MD", out UpdateSubscription? updateSubscriptionMD))
//            {
//                if (_socketClient != null) await _socketClient.UnsubscribeAsync(updateSubscriptionMD);

//                _updateSubscriptions.Remove(security.IsinId + "MD", out var val);

//                _logger.Information("MethodName {@MethodName}, Security {@Security} success!", nameof(UnsubscribeToSecurity), security);
//            }

//            return true;
//        }

//        public async Task<bool> SendOrder(Order order)
//        {
//            OrderSide side = order.Operation == Operation.Buy ? OrderSide.Buy : OrderSide.Sell;

//            FuturesOrderType orderType;
//            TimeInForce? timeInForce;
//            decimal? orderPrice;

//            if (order.OrderType == OrderType.Market)
//            {
//                orderType = FuturesOrderType.Market;
//                timeInForce = null;
//                orderPrice = null;
//            }
//            else
//            {
//                orderType = FuturesOrderType.Limit;
//                timeInForce = TimeInForce.GoodTillCanceled;
//                orderPrice = order.Price;
//            }

//            //FuturesOrderType orderType = order.OrderType == OrderType.Market ? FuturesOrderType.Market : FuturesOrderType.Limit;

//            using (var client = new BinanceRestClient(options => GetOptions(options)))
//            {
//                _logger.Debug("Method{@Method}, Order {@Order}", nameof(SendOrder), order);

//                string newClientOrderId = _referal + order.Comment;

//                _ordersService.SetOrderFromUser(order);

//                try
//                {
//                    var result = await client.UsdFuturesApi.Trading.PlaceOrderAsync(order.SecurityName, side,
//                                orderType, order.Volume,
//                                price: orderPrice, timeInForce: timeInForce,
//                                newClientOrderId: newClientOrderId);
//                    if (result.Success)
//                    {
//                        _logger.Information("Method{@Method}, Order result {@Order}", nameof(SendOrder), result);
//                    }
//                    else
//                    {
//                        order.Status = OrderStatus.Failed;
//                        _logger.Information("Method{@Method}, Order result {@Order}", nameof(SendOrder), result);

//                        OnNewMessage(new Message(title: "Error",
//                                                    text: result.Error?.Message ?? "",
//                                                    exchangeType: ExchangeType,
//                                                    securityName: order.SecurityName));

//                        return false;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    _logger.Error("Method{@Method}, Exception {@Exception}", nameof(SendOrder), ex);

//                }

//                return true;
//            }
//        }

//        public async Task<bool> CancelOrder(Order order)
//        {
//            using (var client = new BinanceRestClient(options => GetOptions(options)))
//            {
//                _logger.Information("Method{@Method}, Order {@Order}", nameof(CancelOrder), order);

//                try
//                {
//                    var result = await client.UsdFuturesApi.Trading.CancelOrderAsync(order.SecurityName, order.NumberMarket);
//                    if (result.Success)
//                    {

//                    }
//                    else
//                    {
//                        OnNewMessage(new Message(title: "Error",
//                                                    text: result.Error?.Message ?? "",
//                                                    exchangeType: ExchangeType,
//                                                    securityName: order.SecurityName));

//                        return false;
//                    }

//                    _logger.Information("Method{@Method}, Order result {@Order}", nameof(CancelOrder), result);
//                }
//                catch (Exception ex)
//                {
//                    _logger.Error("Method{@Method}, Exception {@Exception}", nameof(CancelOrder), ex);

//                }
//            }

//            return true;
//        }

//        #endregion Public Methods

//        #region private Methods ======================================================

//        private async Task GetOrders()
//        {
//            try
//            {
//                using (BinanceRestClient client = new BinanceRestClient(options => GetOptions(options)))
//                {
//                    var info = await client.UsdFuturesApi.Trading.GetOpenOrdersAsync();

//                    if (info.Success)
//                    {
//                        List<BinanceUsdFuturesOrder> list = info.Data.ToList();

//                        foreach (var order in list)
//                        {
//                            Security? security = _securitiesService.GetSecurityFromSecNameAndClass(order.Symbol);

//                            if (security != null)
//                            {
//                                Order newOrder = order.Set(ExchangeType, _accountName, _referal, security);

//                                _ordersService.SetOrderFromExchange(newOrder);
//                            }
//                        }

//                        _logger.Debug("MethodName {@MethodName}, info.Data.Count {@Info} ", nameof(GetOrders), list.Count);
//                    }


//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("MethodName {@MethodName}, Exception {@Exception}", nameof(GetOrders), ex);
//                OnNewMessage(new Message(title: "Error",
//                                                text: ex.Message ?? "",
//                                                exchangeType: ExchangeType));

//            }
//        }

//        private async Task GetSecurities()
//        {
//            using (BinanceRestClient client = new BinanceRestClient(options => GetOptions(options)))
//            {
//                #region Securities ===============================================================================

//                var info = await client.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync();
//                //var tickers = await client.SpotApi.ExchangeData.GetTickersAsync();

//                if (info.Success)
//                {
//                    _logger.Debug("MethodName {@MethodName}, info.ResponseStatusCode {@Info} ", nameof(GetSecurities), info.ResponseStatusCode);

//                    var symbols = info.Data.Symbols;

//                    List<Security> securities = new List<Security>();

//                    foreach (BinanceFuturesUsdtSymbol symbol in symbols)
//                    {
//                        Security security = new Security()
//                        {
//                            Name = symbol.Name,
//                            ClassCode = symbol.QuoteAsset,
//                            IsinId = symbol.Name, // _accountName
//                            FullName = symbol.Name,
//                            BaseContractCode = symbol.QuoteAsset,
//                            ExchangeType = _exchangeType
//                        };

//                        if (symbol.PriceFilter != null)
//                        {
//                            security.PriceStep = symbol.PriceFilter.TickSize;
//                            security.PriceLimitHigh = symbol.PriceFilter.MaxPrice;
//                            security.PriceLimitLow = symbol.PriceFilter.MinPrice;
//                            security.PriceStepCost = symbol.PriceFilter.TickSize;
//                        }

//                        if (symbol.LotSizeFilter != null)
//                        {
//                            security.Lot = symbol.LotSizeFilter.MinQuantity;
//                        }

//                        securities.Add(security);
//                    }

//                    _securitiesService.SetSecurities(securities);

//                    _logger.Information("MethodName {@MethodName}, securities.Count {@Count} ", nameof(GetSecurities), securities.Count);
//                }

//                #endregion
//            }
//        }



//        private void OnListenKeyExpired(DataEvent<BinanceStreamEvent> @event)
//        {
//            //throw new NotImplementedException();
//        }

//        private async void OnLeverageUpdate(DataEvent<BinanceFuturesStreamConfigUpdate> @event)
//        {
//            BinanceFuturesStreamConfigUpdateData data = @event.Data.ConfigUpdateData;

//            await GetPortfolios();
//        }

//        private void OnMarginUpdate(DataEvent<BinanceFuturesStreamMarginUpdate> @event)
//        {
//            BinanceFuturesStreamMarginUpdate upDate = @event.Data;

//        }

//        private async Task GetAllPositions()
//        {
//            try
//            {
//                _logger.Debug("MethodName {@MethodName}, GetAllPositions", nameof(GetAllPositions));

//                List<Portfolio> portfolios = _portfoliosService.GetPortfoliosList();

//                if (portfolios.Count == 0
//                    || string.IsNullOrEmpty(ConnectParaments.BaseCurrency)) return;

//                using (var client = new BinanceRestClient(options => GetOptions(options)))
//                {
//                    var info = await client.UsdFuturesApi.Account.GetPositionInformationAsync();

//                    if (info.Success)
//                    {
//                        List<BinancePositionDetailsUsdt> positions = info.Data.ToList();

//                        foreach (var pos in positions)
//                        {
//                            if (pos.Quantity > 0)
//                            {
//                                _logger.Information("MethodName {@MethodName}, position{@Position}", nameof(GetAllPositions), pos);

//                                await GetOrdersAndTradesForSymbol(client, pos.Symbol, pos.Quantity);
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("MethodName {@MethodName}, Exception {@Exception}", nameof(GetAllPositions), ex);
//                OnNewMessage(new Message(title: "Error",
//                                                text: ex.Message ?? "",
//                                                exchangeType: ExchangeType));
//            }
//        }

//        private async Task GetOrdersAndTradesForSymbol(BinanceRestClient client, string symbol, decimal volume)
//        {
//            List<MyTrade> myTrades = await GetMyTradesFromExchange(client, symbol, volume);

//            _logger.Information("MethodName {@MethodName}, myTrades.Count{@Count}", nameof(GetOrdersAndTradesForSymbol), myTrades.Count);

//            List<Order> orders = await GetOrdersFromExchange(client, myTrades, symbol);

//            _logger.Information("MethodName {@MethodName}, orders.Count{@Count}", nameof(GetOrdersAndTradesForSymbol), orders.Count);

//            foreach (var order in orders)
//            {
//                _ordersService.SetOrderFromExchange(order);
//            }

//            foreach (MyTrade myTrade in myTrades)
//            {
//                _ordersService.SetMyTrade(myTrade);
//            }
//        }

//        private async Task<List<Order>> GetOrdersFromExchange(BinanceRestClient client,
//                                                                List<MyTrade> myTrades,
//                                                                string symbol)
//        {
//            List<Order> orders = new List<Order>();

//            if (myTrades.Count == 0) return orders;

//            try
//            {
//                Security? security = _securitiesService.GetSecurityFromSecNameAndClass(myTrades.Last().SecurityName);

//                if (security == null)
//                {
//                    _logger.Error("Method{@Method}, security == null", nameof(GetOrdersFromExchange));
//                    OnNewMessage(new Message(title: "Error",
//                                                text: "security == null",
//                                                exchangeType: ExchangeType));
//                    return orders;
//                }

//                foreach (MyTrade myTrade in myTrades)
//                {
//                    if (orders.Find(order => order.NumberMarket == myTrade.ParentOrderNumber) == null)
//                    {
//                        var res = await client.UsdFuturesApi.Trading.GetOrderAsync(symbol, myTrade.ParentOrderNumber);

//                        if (res.Success)
//                        {
//                            BinanceFuturesOrder futuresOrder = res.Data;

//                            Order order = futuresOrder.Set(ExchangeType,
//                                                                _accountName,
//                                                                _referal,
//                                                                security);

//                            orders.Add(order);
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("MethodName {@MethodName}, Exception {@Exception}", nameof(GetOrdersFromExchange), ex);
//                OnNewMessage(new Message(title: "Error",
//                                                text: ex.Message ?? "",
//                                                exchangeType: ExchangeType));
//            }

//            return orders;
//        }

//        private async Task<List<MyTrade>> GetMyTradesFromExchange(BinanceRestClient client, string symbol, decimal volume)
//        {
//            List<MyTrade> myTrades = new List<MyTrade>();

//            try
//            {
//                for (int i = 0; i > -365; i -= 7)
//                {
//                    DateTime end = DateTime.Now.AddDays(i);
//                    DateTime start = end.AddDays(-7);

//                    var data = await client.UsdFuturesApi.Trading.GetUserTradesAsync(symbol, start, end);

//                    if (data.Success)
//                    {
//                        List<BinanceFuturesUsdtTrade> trades = data.Data.ToList();

//                        if (trades != null
//                            && trades.Count > 0)
//                        {
//                            Security? security = _securitiesService.GetSecurityFromSecNameAndClass(trades.Last().Symbol);

//                            if (security == null) return myTrades;

//                            for (int y = trades.Count - 1; y >= 0; y--)
//                            {
//                                MyTrade myTrade = trades[y].Set(ExchangeType,
//                                                                _accountName,
//                                                                _referal,
//                                                                security);

//                                myTrades.Insert(0, myTrade);

//                                if (myTrade.Operation == Operation.Buy) volume -= myTrade.Volume;
//                                else volume += myTrade.Volume;

//                                if (volume == 0) return myTrades;
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("MethodName {@MethodName}, Exception {@Exception}", nameof(GetMyTradesFromExchange), ex);
//                OnNewMessage(new Message(title: "Error",
//                                                text: ex.Message ?? "",
//                                                exchangeType: ExchangeType));
//            }

//            return myTrades;
//        }


//        private async Task<bool> GetPortfolios()
//        {
//            try
//            {
//                _logger.Information("MethodName {@MethodName}, GetPortfolios", nameof(GetPortfolios));

//                using (var client = new BinanceRestClient(options => GetOptions(options)))
//                {
//                    List<Portfolio> portfolios = new List<Portfolio>();

//                    var accountSpot = await client.UsdFuturesApi.Account.GetAccountInfoAsync();

//                    if (accountSpot.Success)
//                    {
//                        _logger.Information("MethodName {@MethodName}, accountSpot {@AccountSpot} ", nameof(GetPortfolios), accountSpot);

//                        Portfolio fut = new Portfolio()
//                        {
//                            Name = _accountName
//                        };

//                        foreach (var balance in accountSpot.Data.Assets)
//                        {
//                            if (balance.Asset == "USDT")
//                            {

//                            }

//                            Deposit deposit = new Deposit()
//                            {
//                                Asset = balance.Asset,
//                                Many = balance.WalletBalance,
//                                CurrentMany = balance.AvailableBalance,
//                                BlockedMoney = balance.WalletBalance - balance.AvailableBalance
//                            };

//                            fut.Deposits.Add(deposit);
//                        }

//                        portfolios.Add(fut);

//                        _portfoliosService.SetPortfolios(portfolios);

//                        _logger.Information("MethodName {@MethodName}, portfolios {@Portfolios} ", nameof(GetPortfolios), portfolios);
//                    }
//                    else
//                    {
//                        _logger.Error("MethodName {@MethodName}, accountSpot {@AccountSpot} ", nameof(GetPortfolios), accountSpot);

//                        return false;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("MethodName {@MethodName}, Exception {@Exception}", nameof(GetPortfolios), ex);
//                OnNewMessage(new Message(title: "Error",
//                                                text: ex.Message ?? "",
//                                                exchangeType: ExchangeType));

//                return false;
//            }
//            return true;
//        }

//        private BinanceRestOptions GetOptions(BinanceRestOptions options)
//        {

//            options.OutputOriginalData = true;
//            options.ApiCredentials = new ApiCredentials(_apiKey, _apiSecret);
//            options.Environment = BinanceEnvironment.Live;

//            return options;
//        }

//        private async void OnOrderUpdate(DataEvent<BinanceFuturesStreamOrderUpdate> @event)
//        {
//            var or = @event.Data.UpdateData;

//            _logger.Debug("Method{@Method}, Order data {@Order}", nameof(OnOrderUpdate), or);

//            Security? security = _securitiesService.GetSecurityFromSecNameAndClass(or.Symbol);

//            if (security == null)
//            {
//                _logger.Error("Method{@Method}, security == null", nameof(OnOrderUpdate));
//                return;
//            }

//            Order order = or.Set(ExchangeType, _accountName, _referal);

//            _ordersService.SetOrderFromExchange(order);

//            GetMyTrades(order, security);

//            await GetPortfolios();
//        }

//        private async void GetMyTrades(Order order, Security security)
//        {
//            try
//            {
//                using (var client = new BinanceRestClient(options => GetOptions(options)))
//                {
//                    var res = await client.UsdFuturesApi.Trading.GetUserTradesAsync(order.SecurityName, orderId: order.NumberMarket);

//                    if (res.Success)
//                    {
//                        List<BinanceFuturesUsdtTrade> binanceTrades = res.Data.ToList();

//                        foreach (var trade in binanceTrades)
//                        {
//                            _ordersService.SetMyTrade(trade.Set(ExchangeType,
//                                                                _accountName,
//                                                                _referal,
//                                                                security));
//                        }

//                        _logger.Information("Method{@Method}, binanceTrades {@BinanceTrades}", nameof(GetMyTrades), binanceTrades);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("MethodName {@MethodName}, Exception {@Exception}", nameof(GetMyTrades), ex);
//                OnNewMessage(new Message(title: "Error",
//                                                text: ex.Message ?? "",
//                                                exchangeType: ExchangeType));
//            }
//        }

//        private void OnAccountUpdate(DataEvent<BinanceFuturesStreamAccountUpdate> data)
//        {
//            BinanceFuturesStreamAccountUpdate streamPosition = data.Data;

//            _logger.Information("Method{@Method}, streamPosition{@StreamPosition}", nameof(OnAccountUpdate), streamPosition);

//            List<Portfolio> portfolios = new List<Portfolio>();

//            Portfolio spot = new Portfolio()
//            {
//                Name = _accountName
//            };

//            foreach (BinanceFuturesStreamBalance balance in streamPosition.UpdateData.Balances)
//            {
//                Deposit deposit = new Deposit()
//                {
//                    Asset = balance.Asset,
//                    Many = balance.WalletBalance,
//                    CurrentMany = balance.CrossWalletBalance,
//                    BlockedMoney = balance.BalanceChange
//                };

//                spot.Deposits.Add(deposit);
//            }

//            portfolios.Add(spot);

//            _portfoliosService.SetPortfolios(portfolios);
//        }

//        private async void OnUpdateEventMarketDepth(DataEvent<IBinanceFuturesEventOrderBook> dataEvent)
//        {
//            IBinanceOrderBook md = dataEvent.Data;

//            Security? security = _securitiesService.GetSecurityForIsinId(dataEvent.Topic);

//            if (security == null) return;

//            try
//            {
//                if (_lastMarketDepth.AddSeconds(10) < DateTime.Now)
//                {
//                    _lastMarketDepth = DateTime.Now;

//                    BinanceFuturesOrderBook? book = await GetMarketDepth(dataEvent.Topic);

//                    if (book != null) md = book;
//                }

//                if (_marketDepth.TryGetValue(dataEvent.Topic, out MarketDepthSC? depthOkx))
//                {
//                    decimal bidPrice = md.Bids.Count() > 0 ? md.Bids.Last().Price : 0;
//                    decimal askPrice = md.Asks.Count() > 0 ? md.Asks.First().Price : 0;

//                    if (bidPrice == 0
//                        || askPrice == 0) return;

//                    decimal bidPrice100 = bidPrice - 500 * security.PriceStep;
//                    decimal askPrice100 = askPrice + 500 * security.PriceStep;

//                    foreach (var ask in md.Asks)
//                    {
//                        if (ask.Price > askPrice100) continue;

//                        depthOkx.Bids.TryRemove(ask.Price, out var val);

//                        if (ask.Quantity == 0)
//                        {
//                            depthOkx.Asks.TryRemove(ask.Price, out var val2);
//                            continue;
//                        }

//                        depthOkx.Asks.SetValue(ask.Price, ask.Quantity);
//                    }

//                    foreach (var bid in md.Bids)
//                    {
//                        if (bid.Price < bidPrice100) continue;

//                        depthOkx.Asks.TryRemove(bid.Price, out var val);

//                        if (bid.Quantity == 0)
//                        {
//                            depthOkx.Bids.TryRemove(bid.Price, out var val2);
//                            continue;
//                        }
//                        depthOkx.Bids.SetValue(bid.Price, bid.Quantity);
//                    }

//                    MarketDepth marketDepth = depthOkx.GetMarketDepth(bidPrice, askPrice, _marketDepthsToSend);

//                    //if (marketDepth.Asks.Count() < 200
//                    //    || marketDepth.Bids.Count() < 200)
//                    //{

//                    //}

//                    //if ((askPrice - bidPrice)/security.PriceStep > 20)
//                    //{

//                    //}

//                    SetNewMarketDepth(marketDepth);


//                }
//                else
//                {
//                    depthOkx = new MarketDepthSC()
//                    {
//                        IsinId = dataEvent.Topic
//                    };

//                    _marketDepth.AddOrUpdate(dataEvent.Topic, depthOkx, (key, value) => value = depthOkx);
//                }
//            }
//            catch (Exception ex)
//            {

//            }
//        }

//        private void OnUpdateMarketDepth(DataEvent<IBinanceFuturesEventOrderBook> @event)
//        {
//            IBinanceOrderBook md = @event.Data;
//            //Security? security = _securitiesService.GetSecurityFromSecNameAndClass(md.Symbol);

//            //if (security == null) return;

//            MarketDepth marketDepth;

//            if (!_marketDepthsToSend.TryGetValue(md.Symbol.ToUpper(), out NewMarketDepth? newMarketDepth))
//            {
//                marketDepth = new MarketDepth();
//            }
//            else
//            {
//                marketDepth = newMarketDepth.MarketDepth;
//                marketDepth.Asks.Clear();
//                marketDepth.Bids.Clear();
//            }

//            marketDepth.IsinId = md.Symbol.ToUpper();

//            foreach (var ask in md.Asks)
//            {
//                marketDepth.Asks.Add(new MarketDepthLevel()
//                {
//                    Price = ask.Price,
//                    Volume = ask.Quantity
//                });
//            }

//            foreach (var bid in md.Bids)
//            {
//                marketDepth.Bids.Add(new MarketDepthLevel()
//                {
//                    Price = bid.Price,
//                    Volume = bid.Quantity
//                });
//            }

//            SetNewMarketDepth(marketDepth);

//            if (_lastMarketDepth.AddSeconds(10) < DateTime.Now)
//            {
//                _lastMarketDepth = DateTime.Now;

//                GetMarketDepth(marketDepth.IsinId);

//                //SetNewMarketDepth(marketDepth);
//            }


//        }

//        private async Task<BinanceFuturesOrderBook?> GetMarketDepth(string instrument)
//        {
//            using (BinanceRestClient client = new BinanceRestClient(options => GetOptions(options)))
//            {
//                var resMd = await client.UsdFuturesApi.ExchangeData.GetOrderBookAsync(instrument, 500);

//                if (resMd.Success)
//                {
//                    BinanceFuturesOrderBook md = resMd.Data;

//                    return md;
//                }
//            }

//            return null;
//        }

//        private void OnUpdateTrades(DataEvent<BinanceStreamTrade> @event)
//        {
//            Security? security = _securitiesService.GetSecurityFromSecNameAndClass(@event.Data.Symbol);

//            if (security == null) return;

//            Trade trade = new Trade()
//            {
//                SecurityName = @event.Data.Symbol,
//                SecurityClassCode = security.ClassCode,
//                IsinId = security.IsinId,
//                Number = @event.Data.Id,
//                Price = @event.Data.Price,
//                Volume = @event.Data.Quantity,
//                DateTime = @event.Data.TradeTime,
//                Operation = @event.Data.BuyerIsMaker ? Operation.Sell : Operation.Buy,
//            };

//            _tradesService.SetTrade(trade);
//        }

//        private void OnCandles(DataEvent<IBinanceStreamKlineData> kline)
//        {
//            IBinanceStreamKlineData data = kline.Data;

//            data.Data.Set(IsCreateClusters);
//        }

//        protected async override Task<List<Candle>> getCandles(Security security, TimeFrame timeFrame, int countCandles, Action<int>? LoadingInfo = null)
//        {
//            List<Candle> candles = new List<Candle>();

//            int percent = 0;
//            int all = countCandles;

//            try
//            {
//                using (BinanceRestClient client = new BinanceRestClient(options => GetOptions(options)))
//                {
//                    DateTime? dateEnd = null;
//                    DateTime? dateStart = null;

//                    while (countCandles > 1)
//                    {
//                        var res = await client.UsdFuturesApi.ExchangeData.GetKlinesAsync(security.Name, timeFrame.Set(),
//                                                                                    startTime: dateStart, endTime: dateEnd, limit: 1000);

//                        if (res.Success)
//                        {
//                            List<Candle> newCandles = new List<Candle>();

//                            foreach (IBinanceKline kline in res.Data)
//                            {
//                                newCandles.Add(kline.Set(timeFrame, IsCreateClusters));
//                            }

//                            candles.InsertRange(0, newCandles);

//                            countCandles -= 1000;

//                            percent = (int)((all - countCandles) * 100 / all);

//                            LoadingInfo?.Invoke(percent);
//                        }
//                        else break;

//                        dateEnd = candles.First().DateTime;

//                        dateStart = dateEnd?.AddMinutes(-1000 * (int)timeFrame);

//                        await Task.Delay(20);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("MethodName {@MethodName}, Exception {@Exception}", nameof(getCandles), ex);
//                OnNewMessage(new Message(title: "Error",
//                                                text: ex.Message ?? "",
//                                                exchangeType: ExchangeType));
//            }

//            return candles;
//        }

//        protected override async Task<bool> subscribeToCandles(Security security, TimeFrame timeFrame = TimeFrame.Min1)
//        {
//            return true;
//            try
//            {
//                _logger.Information("MethodName {@MethodName}, subscribeToCandles Name{@Name}, TimeFrame{@TimeFrame}", nameof(subscribeToCandles), security.Name, timeFrame);

//                if (_socketClient == null) return false;

//                var resSubscribe = await _socketClient.UsdFuturesApi.SubscribeToKlineUpdatesAsync(security.Name, timeFrame.Set(), OnCandles);

//                if (resSubscribe.Success) return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("MethodName {@MethodName}, Exception{@Exception}", nameof(subscribeToCandles), ex);
//            }

//            return false;
//        }

//        protected override async Task<bool> unSubscribeToCandles(Security security, TimeFrame timeFrame)
//        {
//            return true;
//        }

//        protected override async Task<bool> SubscribeToSecurityMarketDepth(Security security)
//        {
//            if (_socketClient == null) return false;

//            await Task.Delay(_random.Next(100));

//            try
//            {
//                if (!_updateSubscriptions.TryGetValue(security.IsinId + "MD", out UpdateSubscription? updateSubscription))
//                {
//                    _updateSubscriptions.TryAdd(security.IsinId + "MD", null);
//                    GetMarketDepth(security.Name);



//                    //var subOkay = await _socketClient.UsdFuturesApi.SubscribeToPartialOrderBookUpdatesAsync(security.Name, 20, 100, OnUpdateMarketDepth);
//                    var subOkay = await _socketClient.UsdFuturesApi.SubscribeToOrderBookUpdatesAsync(security.Name, 100, OnUpdateEventMarketDepth);
//                    if (subOkay.Success)
//                    {
//                        _updateSubscriptions.AddOrUpdate(security.IsinId + "MD", subOkay.Data, (key, value) => value = subOkay.Data);

//                        _logger.Information("MethodName {@MethodName}, Security {@Security} success!", nameof(SubscribeToSecurityMarketDepth), security);

//                        return true;
//                    }
//                    else
//                    {
//                        _logger.Error("Method{@Method}, Error{@Error}", nameof(SubscribeToSecurityMarketDepth), subOkay.Error);
//                        OnNewMessage(new Message(title: "Error",
//                                                    text: subOkay?.Error?.Message ?? "Error",
//                                                    exchangeType: ExchangeType));

//                        _updateSubscriptions.Remove(security.IsinId + "MD", out var val);
//                    }

//                    //_socketClient.UsdFuturesApi.order
//                }
//                else return true;

//            }
//            catch (Exception ex)
//            {
//                _logger.Error("MethodName {@MethodName}, Exception {@Exception}", nameof(SubscribeToSecurityMarketDepth), ex);

//                OnNewMessage(new Message(title: "Error",
//                                                text: ex.Message ?? "",
//                                                exchangeType: ExchangeType));
//            }

//            return false;
//        }

//        #endregion private Methods

//        #region Events ===============================================================================================

//        public event Action? DisposeEvent;

//        #endregion
//    }
//}

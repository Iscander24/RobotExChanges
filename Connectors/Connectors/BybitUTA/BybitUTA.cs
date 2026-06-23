using Bybit.Net;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using Bybit.Net.Objects.Models.V5;
using ControllerExChanges.Connectors.Bybit;
using ControllerExChanges.Entity;
using ControllerExChanges.Enums;
using ControllerExChanges.Interfaces;
using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using RestSharp;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Connectors.BybitUTA
{
    internal class BybitUTA : BaseConnector, IConnector
    {
        public BybitUTA(ControllerLogger logger,
                                ICandleService candleService,
                                ISecuritiesService securitiesService,
                                IPortfoliosService portfoliosService,
                                ConnectParaments connectParaments,
                                ITradesService tradesService,
                                IOrdersService ordersService) : base(candleService,
                                                                     securitiesService,
                                                                     portfoliosService,
                                                                     tradesService,
                                                                     ordersService,
                                                                     connectParaments,
                                                                     logger,
                                                                     nameof(BybitUTA))
        {
            _exchangeType = ExchangeType.ByBit;

            _loggerFactory = new LoggerFactory().AddSerilog(logger.Logger);

            _connectParaments.ListParaments.Add(new ParameterRow(typeof(string), _apiKeyName));
            _connectParaments.ListParaments.Add(new ParameterRow(typeof(string), _apiSecretName, true));
            _connectParaments.BaseCurrency = "USDT";
            _connectParaments.ExchangeType = _exchangeType;
        }

        #region Properties ===========================================================


        #endregion Properties

        #region Public Fields ========================================================



        #endregion Public Fields

        #region private Fields =======================================================

        BybitSocketClient? _socketClient;

        ILoggerFactory _loggerFactory;

        /// <summary>
        /// Словарь подписок на инструменты. Ключ - IsinId
        /// </summary>
        ConcurrentDictionary<string, UpdateSubscription?> _updateSubscriptions = new ConcurrentDictionary<string, UpdateSubscription?>();

        //private List<UpdateSubscription> _updateSubscriptions = new();

        string _referal = "NONDLW";

        string _accountName = "UniTrade";

        const string _apiKeyName = "ApiKey";
        const string _apiSecretName = "ApiSecret";

        string _apiKey = string.Empty;
        string _apiSecret = string.Empty;

        DateTime _lastMarketDepth = DateTime.MinValue;

        Random _random = new Random();

        ConcurrentDictionary<string, MarketDepthSC> _marketDepth = new ConcurrentDictionary<string, MarketDepthSC>();

        public event Action? DisposeEvent;


        #endregion

        #region Public Methods =======================================================
        public override async Task<ConnectStatus> Connect()
        {
            if (!_connectParaments.IsReadyParaments())
            {
                return ConnectStatus.Disconnect;
            }

            LastTimeUpDate = DateTime.Now;

            _apiKey = _connectParaments.ListParaments.Find(row => row.ParameterName == _apiKeyName)?.Value?.ToString() ?? string.Empty;
            _apiSecret = _connectParaments.ListParaments.Find(row => row.ParameterName == _apiSecretName)?.Value?.ToString() ?? string.Empty;

            if (_socketClient != null)
            {
                _socketClient.Dispose();
                _socketClient = null;
            }

            try
            {
                _socketClient = new BybitSocketClient(options =>
                {
                    options.ApiCredentials = new BybitCredentials(_apiKey, _apiSecret);
                    options.Environment = BybitEnvironment.Live;
                });

                var ordersSub = await _socketClient.V5PrivateApi.SubscribeToOrderUpdatesAsync(handler: OnOrderUpdate);

                var positionsSub = await _socketClient.V5PrivateApi.SubscribeToPositionUpdatesAsync(handler: OnPositionUpdate);

                var walletSub = await _socketClient.V5PrivateApi.SubscribeToWalletUpdatesAsync(handler: OnWalletUpdates);

                if (!ordersSub.Success || !positionsSub.Success || !walletSub.Success)
                {
                    _logger.Warning("MethodName {@MethodName}, Initial subscription failed ", nameof(Connect));

                    return ConnectStatus.Disconnect;
                }

                _connectStatus = ConnectStatus.Connect;
            }

            catch (Exception ex)
            {
                _logger.Error("MethodName {@MethodName}, Exception {@Exception} ", nameof(Connect), ex);

                OnNewMessage(new Message(title: "Error",
                                                text: ex.Message,
                                                exchangeType: ExchangeType));
                _connectStatus = ConnectStatus.Disconnect;
            }

            OnNewMessage(new Message(title: "ConnectStatus",
                                                text: _connectStatus.ToString(),
                                                exchangeType: ExchangeType));

            OnConnectStatusChangeEvent(_connectStatus);

            GetSecurities();

            return _connectStatus;
        }
        public async Task Disconnect()
        {
            await Dispose();
        }

        public async Task Dispose()
        {
            if (_socketClient == null) return;

            await _socketClient.UnsubscribeAllAsync();

            _socketClient.Dispose();

            _connectStatus = ConnectStatus.Disconnect;

            OnNewMessage(new Message(title: "ConnectStatus",
                                                text: _connectStatus.ToString(),
                                                exchangeType: ExchangeType));

            OnConnectStatusChangeEvent(_connectStatus);
        }


        protected override Task<List<Candle>> getCandles(Security security, TimeFrame timeFrame, int countCandles, Action<int>? LoadingInfo = null)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> subscribeToCandles(Security security, TimeFrame timeFrame = TimeFrame.Min1)
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> SubscribeToSecurity(Security security)
        {
            if (_socketClient == null) return false;

            try
            {
                await Task.Delay(_random.Next(100));

                if (!_updateSubscriptions.TryGetValue(security.IsinId, out UpdateSubscription? updateSubscription))
                //if (_updateSubscriptions != null)
                {
                    _updateSubscriptions.TryAdd(security.IsinId, null);

                    _logger.Information("Method{@Method}, Security{@Security}", nameof(SubscribeToSecurity), security);

                    var tradesOk = await _socketClient.V5SpotApi.SubscribeToTradeUpdatesAsync(symbol: security.Name, handler: OnUpdateTrades);

                    var spreadTradesOk = await _socketClient.V5SpreadApi.SubscribeToTradeUpdatesAsync(symbol: security.Name, handler: OnUpdateTrades);  // сокет для спредов

                    if (tradesOk.Success)
                    {
                        _logger.Information("MethodName {@MethodName}, Security {@Security} success!", nameof(SubscribeToSecurity), security);

                        _updateSubscriptions.AddOrUpdate(security.IsinId, tradesOk.Data, (key, value) => value = tradesOk.Data);

                        //_updateSubscriptions.Add(tradesOk.Data);

                        _logger.Information("Method{@Method}, Security{@Security}, Id{@Id}", nameof(SubscribeToSecurity), security.Name, tradesOk.Data.Id);

                        tradesOk.Data.ConnectionLost += () => { Debug.WriteLine($"!!! Потеряно соединение для {security.Name} !!!"); };

                        tradesOk.Data.ConnectionClosed += () => { Debug.WriteLine($"!!! Соединение закрыто {security.Name} !!!"); };

                        tradesOk.Data.ConnectionRestored += (TimeSpan duration) => { Debug.WriteLine($"!!! Соединение для {security.Name} восстановлено через {duration} !!!"); };

                        //using (var client = new BybitRestClient(options => GetOptions(options)))
                        //{
                        //    var info = await client.V5Api.Account.GetBalancesAsync(AccountType.Unified, security.Name);

                        //    if (info.Success)
                        //    {
                        //        //List<BybitBalance> positions = info.Data.List.ToList();

                        //        var balance = info.Data.List.FirstOrDefault()?.Assets.FirstOrDefault(a => a.Asset == security.Name);

                        //        _logger.Information("MethodName {@MethodName}, balance{@BybitBalance} success!", nameof(SubscribeToSecurity), balance);

                        //        if (balance != null && (balance.WalletBalance != 0 || balance.Free != 0))
                        //        {
                        //            await GetOrdersAndTradesForSymbol(client, security.Name, balance.WalletBalance ?? 0m);
                        //        }
                        //    }
                        //}

                        return true;
                    }
                    else if (spreadTradesOk.Success)
                    {
                        _logger.Information("MethodName {@MethodName}, Security {@Security} success!", nameof(SubscribeToSecurity), security);

                        _updateSubscriptions.AddOrUpdate(security.IsinId, spreadTradesOk.Data, (key, value) => value = spreadTradesOk.Data); // subs for spreads

                        spreadTradesOk.Data.ConnectionLost += () => { Debug.WriteLine($"!!! Потеряно соединение для {security.Name} !!!"); };

                        spreadTradesOk.Data.ConnectionClosed += () => { Debug.WriteLine($"!!! Соединение закрыто {security.Name} !!!"); };

                        spreadTradesOk.Data.ConnectionRestored += (TimeSpan duration) => { Debug.WriteLine($"!!! Соединение для {security.Name} восстановлено через {duration} !!!"); };

                        if (security.ClassCode == "FutureSpread")
                        {

                        }
                    }

                    else            // добавить логгирование ошибок всех сокетов (т.е. если попали сюда, то бумага вообще не нашлась нигде)
                    {
                        _logger.Error("Method{@Method}, Error{@Error}", nameof(SubscribeToSecurity), tradesOk.Error);
                        OnNewMessage(new Message(title: "Error",
                                                    text: tradesOk?.Error?.Message ?? "Error",
                                                    exchangeType: ExchangeType));

                        _updateSubscriptions.Remove(security.IsinId, out var val);
                    }
                }
                else return true;


                _logger.Information("MethodName {@MethodName}, Security {@Security} failure///", nameof(SubscribeToSecurity), security);
            }
            catch (Exception ex)
            {
                _logger.Error("MethodName {@MethodName}, Exception {@Exception}", nameof(SubscribeToSecurity), ex);

                OnNewMessage(new Message(title: "Error",
                                            text: ex.Message,
                                            exchangeType: ExchangeType));
            }

            return false;
        }

        protected override Task<bool> SubscribeToSecurityMarketDepth(Security security)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> unSubscribeToCandles(Security security, TimeFrame timeFrame)
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> UnsubscribeToSecurity(Security security)
        {
            if (_updateSubscriptions.TryGetValue(security.IsinId, out UpdateSubscription? updateSubscription))
            {
                if (_socketClient != null) await _socketClient.UnsubscribeAsync(updateSubscription);

                _updateSubscriptions.Remove(security.IsinId, out var val);

                _logger.Information("MethodName {@MethodName}, Security {@Security} success!", nameof(UnsubscribeToSecurity), security);
            }

            return true;        // если такой бумаги не будет в подписках, то все равно вернет true ! посмотреть
        }        

        public async Task<bool> SendOrder(Order order)
        {
            if (order == null || ConnectStatus != ConnectStatus.Connect) return false;

            OrderSide orderSide = order.Operation == Operation.Buy ? OrderSide.Buy : OrderSide.Sell;

            NewOrderType orderType = order.OrderType == Enums.OrderType.Limit ? NewOrderType.Limit : NewOrderType.Market;

            TimeInForce? timeInForce;

            decimal? orderPrice;

            if (orderType == NewOrderType.Limit)
            {
                timeInForce = TimeInForce.GoodTillCanceled;
                orderPrice = order.Price;
            }
            else
            {
                timeInForce = null;
                orderPrice = null;
            }

            //if (order.ClassCode =! )

            //    var clientRest = new BybitRestClient(options =>
            //    {
            //        options.ApiCredentials = new BybitCredentials(_apiKey, _apiSecret);
            //    });
            //var orderAPI = await clientRest.V5Api.Trading.Plac;


            return true;
        }

        public Task<bool> CancelOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public List<TimeFrame> GetTimeFrames()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods =======================================================
        private void OnOrderUpdate(DataEvent<BybitOrderUpdate[]> @event)
        {
            var ordersList = @event.Data.ToList();

            _logger.Debug("Method{@Method}, OrderList {@orderList}", nameof(OnOrderUpdate), ordersList);

            foreach (var exchangeOrder in ordersList)
            {
                Security security = _securitiesService.GetSecurityFromSecNameAndClass(exchangeOrder.Symbol);

                Order order = exchangeOrder.Set(ExchangeType, _accountName, _referal, security);

                _logger.Debug("Method{@Method}, Order data {@Order}", nameof(OnOrderUpdate), order);

                _ordersService.SetOrderFromExchange(order);
            }
        }

        private void OnWalletUpdates(DataEvent<BybitBalance[]> @event)
        {
            var walletData = @event.Data.ToList();

            _logger.Debug("Method{@Method}, OrderList {@orderList}", nameof(OnWalletUpdates), walletData);

            if (walletData != null)
            {
                foreach (var wallet in walletData)
                {

                }
            }


        }

        private void OnPositionUpdate(DataEvent<BybitPositionUpdate[]> @event)
        {
            var positionData = @event.Data.ToList();

            _logger.Debug("Method{@Method}, OrderList {@orderList}", nameof(OnPositionUpdate), positionData);




        }
        private void OnUpdateTrades(DataEvent<BybitTrade[]> @event)
        {
            try
            {
                Security? security = _securitiesService.GetSecurityFromSecNameAndClass(@event.Symbol);

                if (security == null) return;

                foreach (var incomeTrade in @event.Data)
                {
                    Trade trade = new()
                    {
                        SecurityName = incomeTrade.Symbol,
                        SecurityClassCode = security.ClassCode,
                        IsinId = security.IsinId,
                        Number = long.TryParse(incomeTrade.TradeId, out var id) ? id : 0,
                        Price = incomeTrade.Price,
                        Volume = incomeTrade.Quantity,
                        DateTime = incomeTrade.Timestamp,
                        Operation = incomeTrade.Side == OrderSide.Buy ? Operation.Buy : Operation.Sell,
                    };
                    _tradesService.SetTrade(trade);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("MethodName {@MethodName}, Exception {@Exception}", nameof(OnUpdateTrades), ex);
                OnNewMessage(new Message(title: "Error",
                                                text: ex.Message ?? "",
                                                exchangeType: ExchangeType));
            }
        }

        private async Task GetSecurities()
        {
            using (BybitRestClient client = new BybitRestClient())
            {
                var spotSymbols = await client.V5Api.ExchangeData.GetSpotSymbolsAsync();

                var spreadSymbols = await client.V5Api.ExchangeData.GetSpreadSymbolsAsync();

                if (spotSymbols.Success || spreadSymbols.Success)
                {
                    List<Security> securities = new List<Security>();

                    _logger.Debug("MethodName {@MethodName}, spotInfo.ResponseStatusCode {@spotInfo} ", nameof(GetSecurities), spotSymbols.ResponseStatusCode);

                    var symbolsSpot = spotSymbols.Data.List;

                    foreach (BybitSpotSymbol symbol in symbolsSpot)
                    {
                        Security security = new Security()
                        {
                            Name = symbol.Name,
                            ClassCode = Category.Spot.ToString(),  // нужно переделать на категормю
                            IsinId = symbol.Name, // _accountName
                            FullName = symbol.Name,
                            BaseContractCode = symbol.QuoteAsset,
                            ExchangeType = _exchangeType
                        };

                        if (symbol.PriceFilter != null)
                        {
                            security.PriceStep = symbol.PriceFilter.TickSize;
                            security.PriceStepCost = symbol.PriceFilter.TickSize;
                        }

                        if (symbol.LotSizeFilter != null)
                        {
                            security.Lot = symbol.LotSizeFilter.MinOrderValue;
                        }

                        securities.Add(security);
                    }

                    _logger.Debug("MethodName {@MethodName}, spotInfo.ResponseStatusCode {@spotInfo} ", nameof(GetSecurities), spreadSymbols.ResponseStatusCode);

                    var symbolsSpread = spreadSymbols.Data.ToList();

                    foreach (BybitSpreadSymbol symbol in symbolsSpread)
                    {
                        Security security = new Security()
                        {
                            Name = symbol.Symbol,
                            ClassCode = symbol.ContractType.ToString(),  // берем из ContractType, так как в SpreadSymbol все спреды вместе
                            IsinId = symbol.Symbol, 
                            FullName = symbol.Symbol,
                            BaseContractCode = symbol.QuoteAsset,
                            ExchangeType = _exchangeType
                        };

                        if (symbol.DeliveryTime != null)
                        {
                            security.ExpirationDate = (DateTime)symbol.DeliveryTime;
                        }

                        if (symbol.TickQuantity != null)
                        {
                            security.PriceStep = symbol.TickQuantity;
                            security.PriceStepCost = symbol.TickQuantity;
                        }

                        if (symbol.MinQuantity != null)
                        {
                            security.Lot = symbol.MinQuantity;
                        }

                        securities.Add(security);
                    }

                    _securitiesService.SetSecurities(securities);

                    _logger.Information("MethodName {@MethodName}, securities.Count {@Count} ", nameof(GetSecurities), securities.Count);
                }
            }
        }

        #endregion
    }
}

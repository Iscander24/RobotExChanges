using ControllerExChanges.Entity;
using ControllerExChanges.Enums;
using ControllerExChanges.Interfaces;
using QuikSharp;
using QuikSharp.DataStructures;
using System;
using System.Globalization;
using System.Text;
using Candle = ControllerExChanges.Entity.Candle;

namespace ControllerExChanges.Connectors
{
    internal class QuikConnector : BaseConnector, IConnector
    {
        public QuikConnector(ICandleService candleService,
                            ISecuritiesService securitiesService,
                            IPortfoliosService portfoliosService,
                            ITradesService tradesService,
                            IOrdersService ordersService,
                            ConnectParaments connectParaments,
                            ControllerLogger logger) : base (candleService,
                                                            securitiesService,
                                                            portfoliosService,
                                                            tradesService, ordersService,
                                                            connectParaments,
                                                            logger,
                                                            nameof(QuikConnector))
        {
            _exchangeType = ExchangeType.QuikConnector;
        }

        #region Fields ===============================================================

        Quik? _quik;




        #endregion

        #region Methods ===============================================================

        public async override Task<ConnectStatus> Connect()
        {
            if (_quik == null)
            {
                try
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    _quik = new Quik(Quik.DefaultPort, new InMemoryStorage(), Quik.DefaultHost);

                    SubscribeToQuikEvents(_quik);

                    await GetConnectStatusAsync();

                }
                catch (Exception ex)
                {
                    _logger.Error("Method{@Method}, Exception{@Exception}", nameof(Connect), ex);
                }
            }

            return _connectStatus;
        }

        public Task<bool> CancelOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public async Task Disconnect()
        {
            if (_quik != null)
            {
                UnSubscribeToQuikEvents(_quik);

                _quik.StopService();

                await Task.Delay(1000);

                _quik = null;
            }

            _connectStatus = ConnectStatus.Disconnect;

            OnConnectStatusChangeEvent(_connectStatus);
        }

        public async Task Dispose()
        {
            await Disconnect();

            DisposeEvent?.Invoke();
        }

        public List<TimeFrame> GetTimeFrames()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendOrder(Order order)
        {
            throw new NotImplementedException();
        }
                
        protected override Task<List<Candle>> getCandles(Security security, TimeFrame timeFrame, int countCandles, Action<int>? LoadingInfo = null)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> subscribeToCandles(Security security, TimeFrame timeFrame = TimeFrame.Min1)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> unSubscribeToCandles(Security security, TimeFrame timeFrame)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> SubscribeToSecurity(Security security)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> SubscribeToSecurityMarketDepth(Security security)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> UnsubscribeToSecurity(Security security)
        {
            throw new NotImplementedException();
        }

        #region ================================================== private Methods =====================================================

        private void SubscribeToQuikEvents (Quik quik)
        {
            quik.Events.OnAccountBalance += Events_OnAccountBalance;
            quik.Events.OnAccountPosition += Events_OnAccountPosition;
            quik.Events.OnConnected += Events_OnConnected;
            quik.Events.OnConnectedToQuik += Events_OnConnectedToQuik;
            quik.Events.OnDisconnected += Events_OnDisconnected;
            quik.Events.OnDisconnectedFromQuik += Events_OnDisconnectedFromQuik;
            quik.Events.OnAllTrade += Events_OnAllTrade;
            quik.Events.OnQuote += Events_OnQuote;
            quik.Events.OnOrder += Events_OnOrder;
            quik.Events.OnTrade += Events_OnTrade;
            quik.Events.OnParam += Events_OnParam;
            quik.Events.OnTransReply += Events_OnTransReply;
        }

        private void UnSubscribeToQuikEvents(Quik quik)
        {
            quik.Events.OnAccountBalance -= Events_OnAccountBalance;
            quik.Events.OnAccountPosition -= Events_OnAccountPosition;
            quik.Events.OnConnected -= Events_OnConnected;
            quik.Events.OnConnectedToQuik -= Events_OnConnectedToQuik;
            quik.Events.OnDisconnected -= Events_OnDisconnected;
            quik.Events.OnDisconnectedFromQuik -= Events_OnDisconnectedFromQuik;
            quik.Events.OnAllTrade -= Events_OnAllTrade;
            quik.Events.OnQuote -= Events_OnQuote;
            quik.Events.OnOrder -= Events_OnOrder;
            quik.Events.OnTrade -= Events_OnTrade;
            quik.Events.OnParam -= Events_OnParam;
            quik.Events.OnTransReply -= Events_OnTransReply;
        }

        private void Events_OnTransReply(QuikSharp.DataStructures.Transaction.TransactionReply transReply)
        {
            
        }

        private void Events_OnParam(QuikSharp.DataStructures.Param par)
        {
            
        }

        private void Events_OnTrade(QuikSharp.DataStructures.Transaction.Trade trade)
        {
            
        }

        private void Events_OnOrder(QuikSharp.DataStructures.Transaction.Order order)
        {
            
        }

        private void Events_OnQuote(QuikSharp.DataStructures.OrderBook orderbook)
        {
            
        }

        private void Events_OnAllTrade(QuikSharp.DataStructures.AllTrade allTrade)
        {
            
        }

        private async void Events_OnDisconnectedFromQuik()
        {
            await GetConnectStatusAsync();
        }

        private async void Events_OnDisconnected()
        {
            await GetConnectStatusAsync();
        }

        private async void Events_OnConnectedToQuik(int port)
        {
            await GetConnectStatusAsync();
        }

        private async void Events_OnConnected()
        {
            await GetConnectStatusAsync();
        }

        private async Task GetConnectStatusAsync()
        {
            if (_quik != null)
            {
                bool state = await _quik.Service.IsConnected();

                if (state && _connectStatus != ConnectStatus.Connect)
                {
                    GetSecurities(_quik);
                }

                _connectStatus = state ? ConnectStatus.Connect : ConnectStatus.Disconnect;
            }

            else _connectStatus = ConnectStatus.Disconnect;

            OnConnectStatusChangeEvent(_connectStatus);
        }

        /// <summary>
        /// Получение бумаг с QUIK
        /// </summary>
        /// <param name="quik"></param>
        private void GetSecurities(Quik quik)
        {
            Task.Run(async () =>
            {
                string[] classes = await quik.Class.GetClassesList();

                foreach (string className in classes)
                {
                    string[] securitiesNames = await quik.Class.GetClassSecurities(className);

                    List<Security> securities = new List<Security>();

                    foreach (string name in securitiesNames)
                    {
                        SecurityInfo? securityInfo = await quik.Class.GetSecurityInfo(className, name);

                        if (securityInfo != null)
                        {
                            Security security = new Security();

                            security.Name = securityInfo.Name;
                            security.ClassCode = securityInfo.ClassCode;
                            security.ExchangeType = ExchangeType.QuikConnector;
                            security.Lot = securityInfo.LotSize;
                            security.PriceStep = Convert.ToDecimal(securityInfo.MinPriceStep);
                            security.IsinId = securityInfo.IsinCode;

                            if (security.IsinId == "")
                            {
                                security.IsinId = securityInfo.Name + "_" + securityInfo.ClassCode;
                            }

                            if (DateTime.TryParseExact(securityInfo.MatDate, "yyyyMMdd",
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out DateTime dt))
                            {
                                security.ExpirationDate = dt;
                            }

                            decimal cost = 1;
                            
                            ParamTable? paramTable = await quik.Trading.GetParamEx(className, name, ParamNames.STEPPRICE);

                            if (paramTable != null)
                            {
                                if (decimal.TryParse(paramTable.ParamValue.Replace(".", ","), out cost))
                                {
                                    if (cost == 0)
                                    {
                                        ParamTable? paramTableT = await quik.Trading.GetParamEx(className, name, ParamNames.STEPPRICET);

                                        if (paramTableT != null)
                                        {
                                            if (decimal.TryParse(paramTableT.ParamValue.Replace(".", ","), out cost))
                                            {
                                                if (cost == 0)
                                                {
                                                    cost = (decimal)security.PriceStep;
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                            security.PriceStepCost = cost;

                            ParamTable? paramSellWarranty = await quik.Trading.GetParamEx(className, name, ParamNames.SELLDEPO);

                            if (paramSellWarranty != null)
                            {
                                if (decimal.TryParse(paramSellWarranty.ParamValue.Replace('.', ','), out decimal sellWarranty))
                                {
                                    security.SellersWarranty = sellWarranty;
                                }
                            }

                            ParamTable? paramBuyWarranty = await quik.Trading.GetParamEx(className, name, ParamNames.BUYDEPO);

                            if (paramBuyWarranty != null)
                            {
                                if (decimal.TryParse(paramBuyWarranty.ParamValue.Replace('.', ','), out decimal buyWarranty))
                                {
                                    security.BuyersWarranty= buyWarranty;
                                }
                            }

                            ParamTable? paramVolumeToday = await quik.Trading.GetParamEx(className, name, ParamNames.VOLTODAY);

                            if (paramVolumeToday != null)
                            {
                                if (decimal.TryParse(paramVolumeToday.ParamValue.Replace('.', ','), out decimal volumeToday))
                                {
                                    security.VolumeToDay = volumeToday;
                                }
                            }

                            securities.Add(security);
                        }
                    }
                    _securitiesService.SetSecurities(securities);
                }
            });
        }

        private void Events_OnAccountPosition(QuikSharp.DataStructures.AccountPosition accPos)
        {
            
        }

        private void Events_OnAccountBalance(QuikSharp.DataStructures.AccountBalance accBal)
        {
            
        }

        #endregion

        #endregion

        #region Events =====================================================

        public event Action? DisposeEvent;

        #endregion
    }
}

    


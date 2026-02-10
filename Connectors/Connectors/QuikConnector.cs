using ControllerExChanges.Entity;
using ControllerExChanges.Enums;
using ControllerExChanges.Interfaces;
using QuikSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


                }
                catch (Exception ex)
                {
                    _logger.Error("Method{@Method}, Exception{@Exception}", nameof(Connect), ex);
                }
            }

        }

        public Task<bool> CancelOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public Task Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task Dispose()
        {
            throw new NotImplementedException();
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

        #endregion

        #region Events =====================================================

        public event Action? DisposeEvent;

        #endregion
    }
}

    


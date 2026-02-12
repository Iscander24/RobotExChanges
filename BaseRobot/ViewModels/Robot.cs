using BaseRobot.Commands;
using BaseRobot.RobotEntity;
using BaseRobot.RobotEnums;
using BaseRobot.Views;
using ControllerExChanges.Controller;
using ControllerExChanges.Entity;
using ControllerExChanges.Enums;
using ControllerExChanges.Interfaces;
using ControllerExChanges.Services;
using ControlzEx.Theming;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BaseRobot.ViewModels
{
    public class Robot : BaseVM
    {
        public Robot(ILogger logger, Controller controller)
        {
            controller.EventCreateNewConnector += Controller_EventCreateNewConnector;

            _messenger = Messenger.Instance;

            _messenger.Message += _messenger_Message;

            _dispatcher = Dispatcher.CurrentDispatcher;

            _logger = logger.ForContext<Robot>();

            _controller = controller;
        }

        

        #region =========================== Fields =========================================

        private IConnector? _connector;

        private Security? _security;

        private Messenger _messenger;

        private List<MyCandle> _candles = new List<MyCandle>();

        private TimeFrame _timeFrame = TimeFrame.Min1;

        private Dispatcher _dispatcher;

        public MyPosition? position;

        private ILogger _logger;

        public Theme? theme;

        private Controller _controller;

        #endregion

        #region =========================== Properties =========================================

        public string Header
        {
            get => _header;

            set
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
            }
        }
        private string _header = string.Empty;

        public Security? Security => _security;

        public IConnector? Server => _connector;

        public ConfigRobot? ConfigRobot
        {
            get => _configRobot;

            set
            {
                _configRobot = value;

                if (ConfigRobot != null) InitServer(ConfigRobot);
            }
        }
        ConfigRobot? _configRobot;

        public decimal Price
        {
            get => _price;

            set
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
            }
        }
        decimal _price;

        public decimal VolumeOrder
        {
            get => _volumeOrder;

            set
            {
                _volumeOrder = value;
                OnPropertyChanged(nameof(VolumeOrder));
            }
        }
        private decimal _volumeOrder;

        /// <summary>
        /// Средняя цена открытия позиции
        /// </summary>
        public decimal OpenPrice
        {
            get
            {
                if (position == null) return 0;

                return position.OpenPrice;
            }
        }

        /// <summary>
        /// Объем позиции
        /// </summary>
        public decimal PositionVolume
        {
            get
            {
                if (position == null) return 0;

                return position.Volume; ///
            }
        }

        /// <summary>
        /// Аккумулированный PnL по роботу
        /// </summary>
        public decimal Accum
        {
            get
            {
                if (position == null) return 0;

                return position.Accum;
            }
        }
        public decimal PriceOrder
        {
            get => _priceOrder;

            set
            {
                _priceOrder = value;
                OnPropertyChanged(nameof(PriceOrder));
            }
        }
        private decimal _priceOrder;

        public ObservableCollection<LimitOrder> LimitOrders
        {
            get => _limitOrders;

            set
            {
                _limitOrders = value;
                OnPropertyChanged(nameof(LimitOrders));
            }
        }
        private ObservableCollection<LimitOrder> _limitOrders = new ObservableCollection<LimitOrder>();

        //public ObservableCollection<MyTrade> CompletedTrades
        //{
        //    get => _completedTrades;

        //    set
        //    {
        //        _completedTrades = value;
        //        OnPropertyChanged(nameof(CompletedTrades));
        //    }
        //}
        //private ObservableCollection<MyTrade> _completedTrades = new ObservableCollection<MyTrade>();

        /// <summary>
        /// Список счетов
        /// </summary>
        public ObservableCollection<Portfolio> Portfolios
        {
            get => _portfolios;

            set
            {
                _portfolios = value;
                OnPropertyChanged(nameof(Portfolios));
            }
        }
        private ObservableCollection<Portfolio> _portfolios = new ObservableCollection<Portfolio>();

        /// <summary>
        /// Выбранный счет
        /// </summary>
        public Portfolio? SelectedPortfolio
        {
            get => _selectedPortfolio;

            set
            {
                _selectedPortfolio = value;
                OnPropertyChanged(nameof(SelectedPortfolio));
            }
        }
        private Portfolio? _selectedPortfolio;

        #endregion

        #region =========================== Commands =========================================

        private DelegateCommand? _commandChangeSecurity;
        public DelegateCommand? CommandChangeSecurity
        {
            get
            {
                if (_commandChangeSecurity == null)
                {
                    _commandChangeSecurity = new DelegateCommand(ChangeSecurity);
                }
                return _commandChangeSecurity;
            }
        }

        private DelegateCommand? _commandBuy;

        public DelegateCommand CommandBuy
        {
            get
            {
                if (_commandBuy == null)
                {
                    _commandBuy = new DelegateCommand((object? o) => SendOrder(Operation.Buy));
                }
                return _commandBuy;
            }
        }

        private DelegateCommand? _commandSell;

        public DelegateCommand CommandSell
        {
            get
            {
                if (_commandSell == null)
                {
                    _commandSell = new DelegateCommand((object ? o) => SendOrder(Operation.Sell));
                }
                return _commandSell;
            }
        }

        private DelegateCommand? _commandChart;
        
        public DelegateCommand CommandChart
        {
            get
            {
                if (_commandChart == null)
                {
                    _commandChart = new DelegateCommand(Chart);
                }
                return _commandChart;
            }
        }

        #endregion

        #region =========================== Methods =========================================

        private void Chart(object? obj)
        {
            if (_connector == null || _security == null) return;

            Thread thread = new Thread(() =>
            {
                try
                {
                    ChartWindowVM chartWindowVM = new ChartWindowVM(_logger, _connector, _security);

                    ChartWindow chartWindow = new ChartWindow();
                    chartWindow.DataContext = chartWindowVM;

                    ThemeManager.Current.ChangeTheme(chartWindow, theme ?? ThemeManager.Current.Themes[0]);

                    chartWindow.Title = "Chart " + _security.Name;

                    chartWindow.Show();

                    chartWindow.Closing += (s, e) => chartWindowVM.Unsubscribe();

                    Dispatcher.Run();

                }
                catch(Exception ex)
                {
                    _logger.Error("Method{@Method}, Exception{@Exception}", nameof(Chart), ex);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private void SendOrder(Operation side)
        {
            if (_connector != null
                && _connector.ConnectStatus == ConnectStatus.Connect
                && PriceOrder != 0
                && VolumeOrder > 0
                && Security != null
                && SelectedPortfolio != null
                && position.GetPermission())
            {
                Order order = new Order()
                {
                    ClassCode = Security.ClassCode,
                    SecurityName = Security.Name,
                    Account = SelectedPortfolio.Name,
                    Operation = side,
                    Price = PriceOrder,
                    Volume = VolumeOrder,
                    OrderType = OrderType.Limit,
                    Comment = _connector.GetComment(SelectedPortfolio.Name)
                };

                //Log.Logger.Information("Method{@Method}, New Order{@Order}", nameof(SendOrder), order);

                _logger.Information("Method{@Method}, New Order {@Order}", nameof(SendOrder), order);

                position.AddNewOrder(order);

                _connector.SendOrder(order);
            }
        }


        private void _messenger_Message(MessageType type, object message)
        {
            
        }

        private void Controller_EventCreateNewConnector(IConnector connector)
        {
            if (_configRobot != null && _configRobot.ExchangeType == connector.ExchangeType)
            {
                ServerMaster_ServerCreateEvent(connector);
            }
        }

        private void InitServer(ConfigRobot configRobot)
        {
            if (configRobot.ExchangeType == ExchangeType.None) return;

            List<IConnector> connectors = _controller.Connectors;

            if (connectors == null) return;

            foreach (IConnector server in connectors)
            {
                if (server.ExchangeType == configRobot.ExchangeType)  /////////////
                {
                    ServerMaster_ServerCreateEvent(server);
                }
            }
        }

        /// <summary>
        /// метод запускающий мессенджер с сообщением о смене бумаги и сохранением параметров в конфиг 
        /// которые исполняются в MyBot'е одноименным методом через принятие сообщения (события)
        /// также запускает бумагу
        /// </summary>
        /// <param name="obj"></param>
        private void ChangeSecurity(object? obj)
        {
            _messenger.SendMessage(MessageType.ChangeSecurity, this);
            _messenger.SendMessage(MessageType.SaveParaments);

            if (_security != null) StartSecurity(_security);
        }

        public void SetSecurity(Security security)
        {
            //if (Server != null && _security != null && _security.Name != Security)
            //{
            //    Server.StopThisSecurity()
            //}
            _security = security;

            position = new MyPosition(_security);

            _logger.Information("Method{@Method}, Security {@Security}", nameof(SetSecurity), security);
        }

        public void ServerMaster_ServerCreateEvent(IConnector newServer)
        {
            if (_connector == newServer) return;               // сервер существует = возвращаемся                                

            //_servers.Add(newServer);

            if (_connector != null)
            {
                //_connector.PortfoliosChangeEvent -= NewServer_PortfoliosChangeEvent;
                //_connector.SecuritiesChangeEvent -= NewServer_SecuritiesChangeEvent;
                //_connector.NeedToReconnectEvent -= NewServer_NeedToReconnectEvent;
                _connector.NewMarketDepthEvent -= NewServer_NewMarketDepthEvent;
                _connector.NewTradeEvent -= NewServer_NewTradeEvent;
                //_connector.NewOrderIncomeEvent -= NewServer_NewOrderIncomeEvent;
                _connector.NewMyTradeEvent -= NewServer_NewMyTradeEvent;
                _connector.ConnectStatusChangeEvent -= NewServer_ConnectStatusChangeEvent;
            }

            _connector = newServer;

            //_connector.PortfoliosChangeEvent += NewServer_PortfoliosChangeEvent;
            //_connector.SecuritiesChangeEvent += NewServer_SecuritiesChangeEvent;
            //_connector.NeedToReconnectEvent += NewServer_NeedToReconnectEvent;
            _connector.NewMarketDepthEvent += NewServer_NewMarketDepthEvent;
            _connector.NewTradeEvent += NewServer_NewTradeEvent;
            //_connector.NewOrderIncomeEvent += NewServer_NewOrderIncomeEvent;
            _connector.NewMyTradeEvent += NewServer_NewMyTradeEvent;
            _connector.ConnectStatusChangeEvent += NewServer_ConnectStatusChangeEvent;
        }

        private void NewServer_ConnectStatusChangeEvent(ConnectStatus connectStatus)
        {

        }

        private void NewServer_PortfoliosChangeEvent(List<Portfolio> newPortfolios)
        {
            ObservableCollection<Portfolio> portfolios = new ObservableCollection<Portfolio>();

            foreach (Portfolio portfolio in newPortfolios)
            {
                portfolios.Add(portfolio);

                //if (portfolio != null && this.ConfigRobot != null && portfolio.Number == this.ConfigRobot.PortfolioNumber)
                //{
                //    SelectedPortfolio = portfolio;
                //}
            }

            Portfolios = portfolios;

            OnPropertyChanged(nameof(Portfolios));

            foreach (Portfolio portfolio in Portfolios)
            {
                if (portfolio.Name == this.ConfigRobot?.PortfolioNumber)
                {
                    this.SelectedPortfolio = portfolio;
                    OnPropertyChanged(nameof(SelectedPortfolio));
                    break;
                }
            }

        }

        private void NewServer_SecuritiesChangeEvent(List<Security> securities)
        {
            if (_configRobot != null
                && !string.IsNullOrEmpty(_configRobot.SecurityName)
                && !string.IsNullOrEmpty(_configRobot.SecurityClass))
            {
                foreach (Security security in securities)
                {
                    if (security.ClassCode == _configRobot.SecurityClass
                        && security.Name == _configRobot.SecurityName)
                    {
                        _security = security;

                        StartSecurity(security);
                    }
                }
            }
            #region фильтр для поиска бумаг по вводу
            //ObservableCollection<string> listSecurities = new ObservableCollection<string>();

            //for (int i = 0; i < securities.Count; i++)
            //{
            //    listSecurities.Add(securities[i].Name);
            //}

            //ListSecurities = listSecurities;
            //OnPropertyChanged(nameof(ListSecurities));

            //_securities = securities;
            ////FilteredItems = listSecurities;         // отфильтрованный список бумаг по умолчанию равен всему списку (тк не отфильтрован)
            #endregion
        }

        private void NewServer_NewTradeEvent(Trade trade)
        {
            #region метод из урока
            //Trade trade = trades.Last();

            //if (_security != null && trade.SecurityNameCode == _security.Name)
            //
            //{Price = trade.Price;

            //for (int i = 0; i < trades.Count; i++)
            //{
            //    Trade eachTrade = trades[i];

            //    Debug.WriteLine($"INSIDE TRADES | {trades[i].SecurityNameCode} | {trades[i].Time:HH:mm:ss.fff} | Price: {trades[i].Price} | Volume: {trades[i].Volume} | Side: {trades[i].Side}");
            //}

            //Trade trade = trades.Last();

            //if (_security != null && trade.SecurityNameCode == _security.Name)
            //{
            //    Price = trade.Price;

            //    for (int i = _lastTradeIndex; i < trades.Count; i++)
            //    {
            //        if (_candles.Count == 0)
            //        {
            //            _candles.Add(new MyCandle(trade, _timeFrame));      // первая свеча
            //        }
            //        else
            //        {
            //            MyCandle lastCandle = _candles.Last();

            //            if (lastCandle.TimeStart <= trade.Time
            //                && trade.Time < lastCandle.TimeStart.AddSeconds((int)_timeFrame))
            //            {
            //                lastCandle.AddTick(trade);                  // при втором
            //            }
            //            else
            //            {
            //                _candles.Add(new MyCandle(trade, _timeFrame));
            //            }
            //        }
            //    }
            //    _lastTradeIndex = trades.Count - 1;
            //}
            #endregion

            if (_security != null && trade.IsinId == _security.IsinId)
            {
                Price = trade.Price;

                Debug.WriteLine($"INSIDE TRADES | {trade.SecurityName} | {trade.DateTime:HH:mm:ss.fff} | Price: {trade.Price} | Volume: {trade.Volume} | Side: {trade.Operation}");

                if (_candles.Count == 0)
                {
                    _candles.Add(new MyCandle(trade, _timeFrame));
                }
                else
                {
                    MyCandle lastCandle = _candles.Last();

                    if (lastCandle.TimeStart <= trade.DateTime
                        && trade.DateTime < lastCandle.TimeStart.AddSeconds((int)_timeFrame))
                    {
                        lastCandle.AddTick(trade);
                    }
                    else
                    {
                        _candles.Add(new MyCandle(trade, _timeFrame));
                    }
                }
            }            
        }
        private void NewServer_NewOrderIncomeEvent(Order order)
        {
            position.AddOrderFromServer(order);                 // проверить при чистой позишн ?
            
            LimitOrder limitOrder = new LimitOrder()           // создаем копию ордера под свой класс
            {
                SecurityName = order.SecurityName,
                Direction = order.Operation,
                PriceOrder = order.Price,
                Volume = order.Volume,
                Status = order.Status,
                Comment = order.Comment
            };

            _dispatcher.Invoke(() =>
            {
                bool res = true;

                foreach (var item in LimitOrders)
                {
                    if (item.Comment == order.Comment
                        && item.Status == order.Status)
                    {
                        res = false;
                        break;
                    }
                }
                if (res) LimitOrders.Add(limitOrder);        // если ордера который пришел нет в LimitOrders, то добавляем свою копию в этот список

            });
        }
        private void NewServer_NewMyTradeEvent(MyTrade myTrade)
        {
            position.AddTrade(myTrade);

            OnPropertyChanged(nameof(OpenPrice));
            OnPropertyChanged(nameof(PositionVolume));
            OnPropertyChanged(nameof(Accum));

        }
        private void NewServer_NewMarketDepthEvent(MarketDepth marketDepth)
        {
            //Security security = _connector.SecuritiesService.GetSecurityForIsinId(marketDepth.IsinId);


            //Debug.WriteLine($"INSIDE TRADES | {security.FullName} | {security.Name}");

            if (_security != null && marketDepth.IsinId == _security.IsinId)
            {

            }
        }
        private void NewServer_NeedToReconnectEvent()
        {

        }
        private void StartSecurity(Security security)
        {
            if (security == null)
            {
                Debug.WriteLine("StartSecurity security == null");
                return;
            }
            Task.Run(async() =>
            {
                if (_connector != null)
                {
                    while (true)
                    {
                        bool res = await _connector.AddSecurityToSubscription(security);

                        if (res)                                     // если удалось загрузить бумагу
                        {
                            position = new MyPosition(security);                // то присваиваем позиции эту бумагу, пока только название

                            if (_configRobot != null)
                            {
                                if (_configRobot.Orders != null)
                                {
                                    foreach (Order order in _configRobot.Orders)
                                    {
                                        position.AddNewOrder(order);
                                    }
                                }

                                if (_configRobot.MyTrades != null)
                                {
                                    foreach (MyTrade myTrade in _configRobot.MyTrades)
                                    {
                                        position.AddTrade(myTrade);
                                    }

                                    OnPropertyChanged(nameof(OpenPrice));
                                    OnPropertyChanged(nameof(PositionVolume));
                                    OnPropertyChanged(nameof(Accum));
                                }
                            }

                            break;
                        }

                        Thread.Sleep(1000);
                    }
                }               
                
            });
        }

        #endregion

    }
}

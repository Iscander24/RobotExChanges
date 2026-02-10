// Ignore Spelling: Paraments

using ControllerExChanges.Entity;
using ControllerExChanges.Enums;
using ControllerExChanges.Interfaces;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using ControllerExChanges.Connectors;
using static ControllerExChanges.Interfaces.IConnector;
using Newtonsoft.Json;
using ControllerExChanges.Services;

namespace ControllerExChanges.Controller
{
    /// <summary>
    /// Класс, отвечающий за все подключения к биржам
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class Controller
    {
        private Controller(ILogger log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));

            LogGlobal = log;

            _log = LogGlobal.ForContext("Class", nameof(Controller));

            _serviceCollection = new ServiceCollection();

            _serviceCollection.AddSingleton<ControllerLogger>();
            _serviceCollection.AddTransient<ConnectParaments>();
            _serviceCollection.AddTransient<QuikConnector>();
            _serviceCollection.AddTransient<IPortfoliosService, PortfoliosService>();
            _serviceCollection.AddTransient<ISecuritiesService, SecuritiesService>();
            _serviceCollection.AddTransient<ICandleService, CandleService>();
            _serviceCollection.AddTransient<ITradesService, TradesService>();
            _serviceCollection.AddTransient<IOrdersService, OrdersService>();

            _serviceProvider = _serviceCollection.BuildServiceProvider();

            Init();
        }

        #region Properties ==========================================================================

        /// <summary>
        /// Список коннекторов, которые создал и использует пользователь
        /// (Возможно несколько одинаковых коннекторов с разными(например на разных счетах) или одинаковыми(не знаю зачем...) настройками)
        /// </summary>
        public List<IConnector> Connectors 
        { 
            get => _connectors;
        }
        List<IConnector> _connectors = new List<IConnector>();

        /// <summary>
        /// Не типизированный контекстом логгер
        /// </summary>
        internal static ILogger LogGlobal
        {
            get
            {
                if (_logGlobal == null)
                {
                    throw new ArgumentNullException(nameof(_logGlobal));
                }
                return _logGlobal;
            }

            set => _logGlobal = value;
        }
        static ILogger? _logGlobal;

        #endregion===============

        #region Fields ==============================================================================

        IServiceCollection _serviceCollection;

        IServiceProvider _serviceProvider;        

        /// <summary>
        /// Типизированный классом Controller логгер
        /// </summary>
        ILogger _log;

        //private static Controller? _controller = null;

        static Controller? _controller = null;

        #endregion===============

        #region public Methods =============================================================================       

        /// <summary>
        /// Получить первый из списка коннектор по типу коннектора
        /// </summary>
        /// <param name="exchangeType"></param>
        /// <returns></returns>
        public IConnector? GetConnector(ExchangeType exchangeType, string nameConnector = "")
        {
            if (nameConnector == "") return Connectors.Find(connector => connector.ExchangeType == exchangeType);

            return Connectors.Find(connector => connector.ExchangeType == exchangeType && connector.ConnectParaments.Name == nameConnector);
        }

        /// <summary>
        /// Получить коннектор из списка по имени
        /// </summary>
        /// <param name="nameConnector"></param>
        /// <returns></returns>
        public IConnector? GetConnector(string nameConnector)
        {
            return Connectors.Find(connector => connector.ConnectParaments.Name == nameConnector);
        }


        /// <summary>
        /// Получить Singleton для контроллера (контроллер в системе может быть только один!)
        /// </summary>
        /// <returns></returns>
        public static Controller GetController(ILogger log) // Refactored
        {
            if (_controller == null)
            {
                _controller = new Controller(log);
            }

            return _controller;
        }

        /// <summary>
        /// Создать новое подключение
        /// </summary>
        /// <param name="exchangeType"></param>
        /// <returns></returns>
        public IConnector? CreateConnector(ExchangeType exchangeType,
                                            ConnectParaments? connectParaments = null) // Refactored
        {
            IConnector? connector = null;

            try
            {
                connector = GetConnectorCreated(exchangeType);
            }
            catch (Exception ex)
            {
                _log.Error("{@MethodName}, Exception = {@Exception}", nameof(CreateConnector), ex);
            }

            if (connector == null) return null; // Если коннектор не создался

            connector.SetParamentsEvent += Connector_EventSetParaments;
            ((BaseConnector)connector).EventMessage += Connector_EventMessage;

            if (connectParaments != null) connector.ConnectParaments.SetParaments(connectParaments);

            _connectors.Add(connector);

            _log.Information("{@MethodName}, CreateConnector = {@ExchangeType}", nameof(CreateConnector), exchangeType);

            EventCreateNewConnector?.Invoke(connector);

            return connector;
        }        


        public void RemoveConnector(IConnector connector)
        {
            _log.Information("{@MethodName}, RemoveConnector {@Сonnector}", nameof(RemoveConnector), connector);

            _connectors.Remove(connector);

            SaveParaments();
        }

        /// <summary>
        /// Получить настройки всех созданных подключений пользователя к биржам
        /// </summary>
        public List<ConnectParaments> GetParaments() // Refactored
        {
            List<ConnectParaments> paraments = new List<ConnectParaments>();

            _log.Verbose("{@MethodName},", nameof(GetParaments));

            if (_connectors == null
                || _connectors.Count == 0) return paraments;

            for(int i=0; i< _connectors.Count; i++)
            {
                paraments.Add(_connectors[i].ConnectParaments);
            }

            _log.Information("{@MethodName}, paraments.count{Count}" , nameof(GetParaments), paraments.Count);

            return paraments;
        }


        public List<ExchangeType> GetExchangeTypes()
        {
            List<ExchangeType> all = Enum.GetValues(typeof(ExchangeType)).Cast<ExchangeType>().ToList();

            return all.FindAll(item => IsWhiteListExchange(item));
        }



        #endregion

        #region private Methods =============================================================================

        private void Init() // Refactored
        {
            _log.Information("{@MethodName}, Start Controller", nameof(Init));

            List<ConnectParaments> paraments = LoadParaments();


            if (paraments.Count > 0)
            {
                _log.Information("{@MethodName}, paraments.Count{@Count}", nameof(Init), paraments.Count);

                CreateConnectors(paraments);
            }
        }

        /// <summary>
        /// Создать коннекторы из параметров
        /// </summary>
        /// <param name="paraments"></param>
        private void CreateConnectors(List<ConnectParaments> paraments) // Refactored
        {
            foreach (ConnectParaments param in paraments)
            {
                IConnector? connector = CreateConnector(param.ExchangeType, param);

                if (connector == null)
                {
                    _log.Error("{@MethodName}, Result = null", nameof(CreateConnectors));
                }
                else
                {
                    connector.ConnectParaments.SetParaments(param);                    

                    if (param.AutoConnect)
                    {
                        Task.Run(() => connector.Connect().Result);
                    }
                }
            }

            _log.Information(" {@MethodName}, CreateConnectors count{Count}", nameof(CreateConnectors), paraments.Count);
        }

        private void Connector_EventMessage(Message message)
        {
            EventMessage?.Invoke(message);
        }

        private void Connector_EventSetParaments(ConnectParaments? paraments)
        {
            SaveParaments();
        }


        /// <summary>
        /// Сохранить параметры в файл Paraments\connectors.param
        /// </summary>
        private async void SaveParaments() // Refactored
        {
            try
            {
                List<ConnectParaments> paraments = GetParaments();

                if (paraments.Count == 0) return;

                if (!Directory.Exists(@"Paraments"))
                {
                    Directory.CreateDirectory(@"Paraments");

                    _log.Information(" {@MethodName}, CreateDirectory Paraments success!", nameof(SaveParaments));
                }

                string str = JsonConvert.SerializeObject(paraments);

                if (!string.IsNullOrEmpty(str))
                {
                    using (StreamWriter writer = new StreamWriter(@"Paraments\connectors.param", false))
                    {
                        writer.WriteLine(str);
                    }

                    _log.Information(" {@MethodName}, Save success!", nameof(SaveParaments));
                }
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                {
                    await Task.Delay(100);

                    SaveParaments();

                    return;
                }
                
                _log.Error("{@MethodName}, Exception = {@Exception}", nameof(SaveParaments), ex);
            }
        }

        /// <summary>
        /// Загрузить параметры из файла Paraments\connectors.param
        /// </summary>
        private List<ConnectParaments> LoadParaments() // Refactored
        {
            List<ConnectParaments>? paraments = new List<ConnectParaments>();

            if (!Directory.Exists(@"Paraments"))
            {
                _log.Information(" {@MethodName}, not Paraments!", nameof(LoadParaments));
                return paraments;
            }

            try
            {
                string[] files = Directory.GetFiles(@"Paraments\");

                bool res = false;

                if (files.Length > 0)
                {
                    foreach (string file in files)
                    {
                        if (file == "Paraments\\connectors.param")
                        {
                            res = true;
                            break;
                        }
                    }
                }

                if (res)
                {
                    using (StreamReader reader = new StreamReader(@"Paraments\connectors.param"))
                    {
                        string? str = reader.ReadLine();

                        if (str != null)
                        {
                            paraments = JsonConvert.DeserializeObject<List<ConnectParaments>>(str);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("{@MethodName}, Exception = {@Exception}", nameof(LoadParaments), ex);
            }

            if (paraments == null)
            {
                paraments = new List<ConnectParaments>();
            }

            _log.Information("{@MethodName}, paraments.count {Paraments} ", nameof(LoadParaments), paraments.Count);

            return paraments;
        }

        /// <summary>
        /// Создать коннектор по типу
        /// </summary>
        /// <param name="exchangeType"></param>
        /// <returns></returns>
        private IConnector? GetConnectorCreated(ExchangeType exchangeType) // Refactored
        {
            IConnector? connector = null;

            _log.Information("{@MethodName}, ExchangeType {@ExchangeType} ", nameof(GetConnectorCreated), exchangeType);

            try
            {
                switch (exchangeType)
                {
                    //case ExchangeType.BinanceFutures:
                    //    connector = _serviceProvider.GetRequiredService<BinanceFuturesConnector>();
                    //    break;


                    case ExchangeType.QuikConnector:
                        connector = _serviceProvider.GetRequiredService<QuikConnector>();
                        break;

                }

                if (connector != null)
                {
                    //connector.ConnectParaments.SetParaments(exchangeType: exchangeType);
                    _log.Information("{@MethodName}, ExchangeType {@ExchangeType} success!", nameof(GetConnectorCreated), exchangeType);
                }               
            }
            catch (Exception ex)
            {
                _log.Error("{@MethodName}, Exception = {@Exception}", nameof(GetConnectorCreated), ex);
            }

            return connector;
        }

        private bool IsWhiteListExchange(ExchangeType exchangeType)
        {
            switch (exchangeType)
            {
                //case ExchangeType.Transaq: return true;

                case ExchangeType.QuikConnector: return true;

                //case ExchangeType.Binance: return true;

                //case ExchangeType.BinanceFutures: return true;

                //case ExchangeType.LiveFutures: return true;

                //case ExchangeType.Okx: return true;

                default: return false;
            }
        }

        #endregion

        #region Events ==============================================================================

        public delegate void eventCreateNewConnector(IConnector connector);
        public event eventCreateNewConnector? EventCreateNewConnector;

        public event eventMessage? EventMessage;

        #endregion


    }

}

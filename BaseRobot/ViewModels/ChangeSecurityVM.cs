using BaseRobot.Commands;
using ControllerExChanges.Controller;
using ControllerExChanges.Entity;
using ControllerExChanges.Enums;
using ControllerExChanges.Interfaces;
using System.Collections.ObjectModel;

namespace BaseRobot.ViewModels
{
    public class ChangeSecurityVM : BaseVM
    {
        public ChangeSecurityVM(Robot robot, Controller controller)
        {
            _robot = robot;

            _controller = controller;

            Connectors = GetServers();
        }

        #region =========================== Fields =========================================

        Robot _robot;

        Controller _controller;

        #endregion

        #region =========================== Properties =========================================

        public ObservableCollection<ExchangeType> Connectors { get; set; }

        public ExchangeType Connector
        {
            get => _connector;

            set
            {
                _connector = value;
                OnPropertyChanged(nameof(Connector));

                CodeClasses = GetCodeClasses(_connector);
                OnPropertyChanged(nameof(CodeClasses));
            }
        }
        private ExchangeType _connector = ExchangeType.None;

        public ObservableCollection<string> CodeClasses { get; set; }

        public string SelectedCodeClass
        {
            get => _selectedCodeClass;

            set
            {
                _selectedCodeClass = value;
                OnPropertyChanged(nameof(SelectedCodeClass));

                Securities = GetSecurities(SelectedCodeClass);
                OnPropertyChanged(nameof(Securities));
            }
        }
        private string _selectedCodeClass = string.Empty;

        public ObservableCollection<SecurityVM> Securities { get; set; }

        public SecurityVM SelectedSecurityVM
        {
            get => _selectedSecurityVM;

            set
            {
                _selectedSecurityVM = value;
                OnPropertyChanged(nameof(SelectedSecurityVM));
            }
        }
        private SecurityVM _selectedSecurityVM;

        #endregion

        #region =========================== Commands =========================================

        private DelegateCommand? _commandSelectedSecurity;

        public DelegateCommand? CommandSelectedSecurity
        {
            get
            {
                if (_commandSelectedSecurity == null)
                {
                    _commandSelectedSecurity = new DelegateCommand(SelectedSecurity);
                }
                return _commandSelectedSecurity;
            }
        }

        #endregion

        #region =========================== Methods =========================================
        /// <summary>
        /// метод для выбора бумаги из списка предложенного сервером. Устанавливает новый сервер для робота, если необходимо. А аткже загружает данные по бумаге
        /// </summary>
        /// <param name="obj"></param>
        private void SelectedSecurity(object? obj)
        {
            IConnector server = GetServer(Connector);

            _robot.ServerMaster_ServerCreateEvent(server);   // точка отсанова

            _robot.SetSecurity(SelectedSecurityVM.GetSecurity());
        }

        /// <summary>
        /// метод возвращающий все подключенные серверы для отображения в окне смены бумаги
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<ExchangeType> GetServers()
        {
            ObservableCollection<ExchangeType> newServers = new ObservableCollection<ExchangeType>();

            List<IConnector>? servers = _controller.Connectors;

            if (servers == null) return newServers;

            foreach (IConnector server in servers)
            {
                if (server != null && server.ConnectStatus == ConnectStatus.Connect)
                {
                    newServers.Add(server.ExchangeType);
                }
            }
            return newServers;
        }

        /// <summary>
        /// метод возвращающий все коды инструментов для отображения в окне смены бумаги
        /// </summary>
        /// <param name="ExchangeType"></param>
        /// <returns></returns>
        private ObservableCollection<string> GetCodeClasses(ExchangeType ExchangeType)
        {
            ObservableCollection<string> codeClasses = new ObservableCollection<string>();

            IConnector? connector = GetServer(ExchangeType);

            if (connector == null) return codeClasses;

            List<Security> securities = connector.SecuritiesService.GetSecuritiesList();

            foreach (Security security in securities)
            {
                if (codeClasses.Count == 0) codeClasses.Add(security.ClassCode);

                else
                {
                    if (!IsCodeClass(security, codeClasses))
                    {
                        codeClasses.Add(security.ClassCode);
                    }
                }
            }
            return codeClasses;
        }

        /// <summary>
        /// метод для подтягивания списка бумаг соответсвующих коду инструмента в выбранном сервере
        /// </summary>
        /// <param name="codeClass"></param>
        /// <returns></returns>
        private ObservableCollection<SecurityVM> GetSecurities(string codeClass)
        {
            ObservableCollection<SecurityVM> securities = new ObservableCollection<SecurityVM>();

            if (Connector == ExchangeType.None) return securities;

            IConnector? server = GetServer(Connector);

            if (server == null) return securities;

            List<Security> controllerSecurities = server.SecuritiesService.GetSecuritiesList();

            foreach (Security security in controllerSecurities)     // заменить на for?
            {
                if (security.ClassCode == codeClass) securities.Add(new SecurityVM(security));
                
                //if (security.ClassCode == codeClass)
                //{
                //    if (security.Name == "SBER")
                //    {

                //    }
                //    securities.Add(new SecurityVM(security));
                //}
            }
            return securities;
        }

        private bool IsCodeClass (Security security, ObservableCollection<string> codeClasses)
        {
            foreach (string codeClass in codeClasses)
            {
                if (codeClass == security.ClassCode) return true;
            }
            return false;
        }

        private IConnector? GetServer(ExchangeType ExchangeType)
        {
            List<IConnector>? connectors = _controller.Connectors;

            if (connectors == null) return null;

            foreach (IConnector server in connectors)
            {
                if (server != null && server.ExchangeType == ExchangeType) return server;
            }
            return null;
        }


        #endregion
    }
}

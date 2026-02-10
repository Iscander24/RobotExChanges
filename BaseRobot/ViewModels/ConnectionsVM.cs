using BaseRobot.Commands;
using ControllerExChanges.Controller;
using ControllerExChanges.Enums;
using ControllerExChanges.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseRobot.ViewModels
{
    public class ConnectionsVM : BaseVM
    {
        public ConnectionsVM(Controller controller, ILogger logger)
        {
            _controller = controller;

            _logger = logger.ForContext<ConnectionsVM>();

            _globalLogger = logger;

            ExchangeTypes = _controller.GetExchangeTypes();

            Init();
        }


        #region =========================== Fields ===========================================

        private Controller _controller;

        private ILogger _logger;

        private ILogger _globalLogger;

        #endregion

        #region =========================== Property =========================================

        public ObservableCollection<ConnectionVM> ConnectionVMs
        {
            get => _connectionVMs;

            set
            {
                _connectionVMs = value;
                OnPropertyChanged(nameof(ConnectionVMs));
            }
        }
        private ObservableCollection<ConnectionVM> _connectionVMs = new ObservableCollection<ConnectionVM>();

        public List<ExchangeType> ExchangeTypes { get; set; }

        #endregion

        #region =========================== Commands =========================================

        private DelegateCommand _commandAddConnector;
        public DelegateCommand CommandAddConnector
        {
            get
            {
                if (_commandAddConnector == null) _commandAddConnector = new DelegateCommand(AddConnector);
                return _commandAddConnector;
            }
        }




        #endregion

        #region =========================== Methods ==========================================

        private void AddConnector(object obj)
        {
            ConnectionVMs.Add(new ConnectionVM(_controller, _globalLogger, ExchangeType.None));
        }

        private void Init()
        {
            ObservableCollection<ConnectionVM> connectionVMs = new();

            List<IConnector> connectors = _controller.Connectors;

            foreach (IConnector connector in connectors)
            {
                connectionVMs.Add(new ConnectionVM(_controller, _globalLogger, connector.ExchangeType));
            }

            ConnectionVMs = connectionVMs;
        }



        #endregion





    }
}

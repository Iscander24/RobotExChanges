using ControllerExChanges.Controller;
using ControllerExChanges.Entity;
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
    public class ConnectionVM : BaseVM
    {
        public ConnectionVM(Controller controller, ILogger logger, ExchangeType exchangeType)
        {
            _controller = controller;

            _logger = logger.ForContext<ConnectionVM>();

            SelectedExchangeType = exchangeType;
        }



        #region =========================== Fields ===========================================

        private Controller _controller;

        private ILogger _logger;

        private IConnector _connector;

        #endregion

        #region =========================== Property =========================================

        public ObservableCollection<ParameterRow> ParameterRows
        {
            get => _parameterRows;

            set
            {
                _parameterRows = value;
                OnPropertyChanged(nameof(ParameterRows));
            }
        }
        private ObservableCollection<ParameterRow> _parameterRows = new();

        /// <summary>
        /// Выбранная биржа в выпадающем списке коннекторов
        /// </summary>
        public ExchangeType SelectedExchangeType
        {
            get => _selectedExchangeType;

            set
            {
                _selectedExchangeType = value;
                OnPropertyChanged(nameof(SelectedExchangeType));

                if (SelectedExchangeType != ExchangeType.None) Init();
            }
        }
        private ExchangeType _selectedExchangeType = ExchangeType.None;

        public ConnectStatus ConnectStatus
        {
            get => _connectStatus;

            set
            {
                _connectStatus = value;
                OnPropertyChanged(nameof(ConnectStatus));
            }
        }
        private ConnectStatus _connectStatus = ConnectStatus.Disconnect;

        /// <summary>
        /// Свойство для вкл/выкл конкретного соединения биржи
        /// </summary>
        public bool OnConnector
        {
            get => _onConnector;
            
            set
            {
                _onConnector = value;
                OnPropertyChanged(nameof(OnConnector));

                if (OnConnector && SelectedExchangeType != ExchangeType.None && _connector != null)
                {
                    _connector.ConnectParaments.SetParaments(nameConnector:"", 
                        listParaments: ParameterRows.ToList(), 
                        autoConnect:false,
                        exchangeType: SelectedExchangeType);

                    if (_connector.ConnectStatus != ConnectStatus.Connect)
                    {
                        Task.Run(() =>
                        {
                            _connector.Connect();
                        });
                    }
                }
            }
        }
        private bool _onConnector;

        #endregion

        #region =========================== Commands =========================================


        #endregion

        #region =========================== Methods ==========================================

        private void Init()
        {
            //IConnector? connector = _controller.GetConnector(SelectedExchangeType) ?? _controller.CreateConnector(SelectedExchangeType);

            IConnector? connector = _controller.GetConnector(SelectedExchangeType);

            if (connector == null) connector = _controller.CreateConnector(SelectedExchangeType);

            if (connector != null)
            {
                connector.ConnectStatusChangeEvent += Connector_ConnectStatusChangeEvent;

                _connector = connector;

                ObservableCollection<ParameterRow> parameterRows = new();

                foreach (ParameterRow parameterRow in connector.ConnectParaments.ListParaments)
                {
                    parameterRows.Add(parameterRow);
                }

                ParameterRows = parameterRows;
            }
            else
            {
                _logger.Error("Method{@Method}, SelectedExchangeType{@SelectedExchangeType}, connector == null", nameof(Init), SelectedExchangeType);
            }
        }

        private void Connector_ConnectStatusChangeEvent(ConnectStatus status)
        {
            ConnectStatus = status;
        }


        #endregion

        #region =========================== Events ===========================================


        #endregion
    }
}

// Ignore Spelling: Paraments

using ControllerExChanges.Enums;
using ControllerExChanges.Interfaces;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static ControllerExChanges.Interfaces.IConnector;

namespace ControllerExChanges.Entity
{
    /// <summary>
    /// Класс, сохраняющий параметры подключения для коннектора
    /// </summary>
    [Serializable]
    public class ConnectParaments
    {
        public ConnectParaments(ControllerLogger logger)
        {
            _logger = logger.Logger.ForContext<ConnectParaments> ();
        }

        public ConnectParaments()
        {

        }


        #region Properties =========================================================================

        /// <summary>
        /// Тип коннектора (биржа)
        /// </summary>
        [JsonInclude]
        public ExchangeType ExchangeType { get; set; } = ExchangeType.None;

        /// <summary>
        /// Уникальное имя коннектора. Задаётся пользователем.
        /// </summary>
        [JsonInclude]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Логин, ключ, емейл или др. подобное
        /// </summary>
        [JsonInclude]
        public List<ParameterRow> ListParaments { get; private set; } = new List<ParameterRow>();


        public bool AutoConnect { get; set; } = false;

        /// <summary>
        /// Базовая валюта для расчётов
        /// </summary>
        [JsonInclude]
        public string BaseCurrency { get; set; } = string.Empty;
        #endregion

        #region Fields ==============================================================================

        ILogger _logger;

        //public 

        #endregion

        #region Methods =============================================================================

        public void SetParaments(ConnectParaments paraments) // Refactored
        {
            if (paraments == null)
            {
                _logger.Warning("{@MethodName}, paraments == null", nameof(SetParaments));
                return;
            }

            this.Name = paraments.Name;
            this.ExchangeType = paraments.ExchangeType;
            this.BaseCurrency = paraments.BaseCurrency;
            this.AutoConnect = paraments.AutoConnect;

            if (paraments.ListParaments.Count > 0 ) this.ListParaments = paraments.ListParaments;

            _logger.Information("{@MethodName}, SetParaments Name{@Name}", nameof(SetParaments), Name);

            SetParamentsEvent?.Invoke(this);
        }

        public void SetParaments(string nameConnector ="",
                                    List<ParameterRow>? listParaments = null , 
                                    bool? autoConnect = null,
                                    ExchangeType exchangeType = ExchangeType.None,
                                    string baseCurrency = "")
        {
            if (nameConnector != "") this.Name = nameConnector;
            if (autoConnect != null) this.AutoConnect = (bool)autoConnect;
            if (exchangeType != ExchangeType.None) this.ExchangeType = exchangeType;
            if (baseCurrency != "") this.BaseCurrency = baseCurrency;

            if (listParaments != null) this.ListParaments = listParaments;

            _logger.Information("{@MethodName}, SetParaments Name{@Name}", nameof(SetParaments), Name);

            SetParamentsEvent?.Invoke(this);
        }


        /// <summary>
        /// Проверить заполненность параметров. 
        /// Если вернул true - параметры заполнены, можно подключаться
        /// </summary>
        /// <returns></returns>
        public bool IsReadyParaments() // Refactored
        {
            _logger.Information("Method {@Method}, ExchangeType {ExchangeType}, Name {Name}",
                nameof(IsReadyParaments), ExchangeType, Name);

            if (ExchangeType == ExchangeType.None) return false;

            foreach (var param in ListParaments)
            {
                if (param.TypeParameter == typeof(string))
                {
                    if (param.Value == null
                        || string.IsNullOrEmpty((string)param.Value)) return false;
                }
            }

            return true;
        }


        #endregion

        #region Events =========================================================================

        public delegate void setParamentsEvent(ConnectParaments paraments);
        public event setParamentsEvent? SetParamentsEvent;

        #endregion
    }
}

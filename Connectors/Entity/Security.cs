using ControllerExChanges.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    /// <summary>
    /// security
    /// инструмент
    /// </summary>
    public class Security
    {
        /// <summary>
        /// security
        /// Ценная бумага
        /// </summary>
        public Security()
        {
            PriceLimitLow = 0;
            PriceLimitHigh = 0;
        }

        #region Properties ================================================================

        public ExchangeType ExchangeType { get; set; } = ExchangeType.None;

        /// <summary>
        /// security name
        /// название инструмента
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// full name
        /// полное название
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// class code
        /// код класса
        /// </summary>
        public string ClassCode { get; set; } = string.Empty;

        /// <summary>
        /// Код базового актива
        /// </summary>
        public string BaseContractCode { get; set; } = string.Empty;

        /// <summary>
        /// Unique tool identifier.
        /// It is used in some platforms as the main instrument key in the trading system.
        /// Уникальный идентификатор инструмента.
        ///  Используется в некоторых платформах как главный ключ инструмента в торговой системе.
        /// </summary>
        public string IsinId { get; set; } = string.Empty;

        /// <summary>
        /// Тип базового актива для срочного рынка
        /// </summary>
        public int AssetClass { get; set; }

        /// <summary>
        /// Расчетная цена после последнего клиринга
        /// </summary>
        public decimal SettlementPrice { get; set; }

        /// <summary>
        /// Обьем торгов за последний день
        /// </summary>
        public decimal VolumeToDay { get; set; }

        /// <summary>
        /// the trading status of this instrument on the stock exchange
        /// состояние торгов этим инструментом на бирже
        /// </summary>
        public TradingStatus State { get; set; }

        /// <summary>
        /// price step, i.e. minimal price change for the instrument
        /// шаг цены, т.е. минимальное изменение цены для инструмента
        /// </summary>
        public decimal PriceStep { get; set; } = 1;

        /// <summary>
        /// lot
        /// лот
        /// </summary>
        public decimal Lot { get; set; }

        /// <summary>
        /// the cost of a step of the price, i.e. how much profit is dripping on the deposit for one step of the price
        /// стоимость шага цены, т.е. сколько профита капает на депозит за один шаг цены
        /// </summary>
        public decimal PriceStepCost { get; set; }

        /// <summary>
        /// warranty coverage
        /// гарантийное обеспечение продавца
        /// </summary>
        public decimal SellersWarranty { get; set; }

        /// <summary>
        /// warranty coverage
        /// гарантийное обеспечение покупателя
        /// </summary>
        public decimal BuyersWarranty { get; set; }

        /// <summary>
        /// security type
        /// тип бумаги
        /// </summary>
        public SecurityType SecurityType { get; set; }

        /// <summary>
        /// the number of decimal places of the instrument price.
        /// количество знаков после запятой цены инструмента.
        /// </summary>
        public int Decimals { get; set; }


        /// <summary>
        /// Lower price limit for bids. If you place an order with a price lower - the system will reject
        /// Нижний лимит цены для заявок. Если выставить ордер с ценой ниже - система отвергнет
        /// </summary>
        public decimal PriceLimitLow { get; set; }

        /// <summary>
        /// Upper price limit for bids. If you place an order with a price higher - the system will reject
        /// Верхний лимит цены для заявок. Если выставить ордер с ценой выше - система отвергнет
        /// </summary>
        public decimal PriceLimitHigh { get; set; }
        // For options
        // для опционов

        /// <summary>
        /// option type
        /// тип опциона
        /// </summary>
        public OptionType OptionType { get; set; }

        /// <summary>
        /// strike
        /// страйк
        /// </summary>
        public decimal Strike { get; set; }

        /// <summary>
        /// expiration date
        /// дата экспирации
        /// </summary>
        public DateTime ExpirationDate { get; set; }

        #endregion

    }
}

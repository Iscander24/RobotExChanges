using ControllerExChanges.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    /// <summary>
    /// order
    /// ордер
    /// </summary>
    public class Order
    {
        public Order()
        {
            
        }

        /// <summary>
        /// instrument code 
        /// Isin код инструмента ордера
        /// </summary>
        public string IsinId { get; set; } = string.Empty;

        /// <summary>
        /// order number in the robot
        /// номер ордера в роботе
        /// </summary>
        public int LocalNumber { get; set; }

        /// <summary>
        /// order number on the exchange
        /// номер ордера на бирже
        /// </summary>
        public long NumberMarket { get; set; } = 0;

        /// <summary>
        /// instrument code for which the transaction took place
        /// код инструмента по которому прошла сделка
        /// </summary>
        public string SecurityName { get; set; } = string.Empty;

        /// <summary>
        /// Код класса
        /// </summary>
        public string ClassCode { get; set; } = string.Empty;

        /// <summary>
        /// account number to which the order belongs
        /// номер счёта которому принадлежит ордер
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// direction
        /// направление операции
        /// </summary>
        public Operation Operation { get; set; } = Operation.None;

        /// <summary>
        /// bid price
        /// цена заявки
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// real price
        /// цена исполнения
        /// </summary>
        public decimal AveragePrice { get; set; }

        /// <summary>
        /// Цена срабатывания стоп лосса
        /// </summary>
        public decimal StopPrice { get; set; }

        /// <summary>
        /// Проскальзывание для стоп ордера в шагах цены
        /// </summary>
        public decimal SlipPageForStopOrder { get; set; }

        /// <summary>
        /// volume
        /// объём
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// execute volume
        /// объём исполнившийся
        /// </summary>
        public decimal VolumeFilled { get; set; }


        /// <summary>
        /// order status: None, Pending, Done, Patrial, Fail
        /// статус ордера: None, Pending, Done, Patrial, Fail
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.None;


        /// <summary>
        /// order price type. Limit, Market
        /// тип цены ордера. Limit, Market
        /// </summary>
        public OrderType OrderType { get; set; }


        /// <summary>
        /// user comment
        /// комментарий пользователя
        /// </summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// time of the first response from the stock exchange on the order. Server time
        /// время первого отклика от биржи по ордеру. Время севрера.
        /// </summary>
        public DateTime TimeCallBack { get; set; } = DateTime.MinValue;

        /// <summary>
        /// time of order removal from the system. Server time
        /// время снятия ордера из системы. Время сервера
        /// </summary>
        public DateTime TimeCanceled { get; set; } = DateTime.MinValue;

        /// <summary>
        /// order execution time. Server time
        /// время исполнения ордера. Время сервера
        /// </summary>
        public DateTime TimeFilled { get; set; } = DateTime.MinValue;

        /// <summary>
        /// order creation time in OsApi. Server time
        /// время создания ордера в OsApi. Время сервера
        /// </summary>
        public DateTime TimeCreated { get; set; } = DateTime.MinValue;


        public ExchangeType ExchangeType { get; set; }
        // deals with which the order was opened and calculation of the order execution price
        // сделки, которыми открывался ордер и расчёт цены исполнения ордера

    }
}

using ControllerExChanges.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ControllerExChanges.Interfaces.IConnector;

namespace ControllerExChanges.Interfaces
{
    // Не пытаемся сохранить единую ссылку на первоначальный ордер.
    // Каждое событие по ордеру, может порождать новый обьект. 
    // Для учета ордеров в роботах, нужно хранить номера ордеров или комментарии

    public interface IOrdersService
    {
        /// <summary>
        /// Словарь всех ордеров. Ключ NumberMarket
        /// </summary>
        ConcurrentDictionary<long, Order> KeyMyOrders { get; }

        /// <summary>
        /// Все ордера отправленные на регистрацию бирже. Ключ = comment
        /// Возможно этот словарь даже лишний....
        /// </summary>
        ConcurrentDictionary<string, Order> SentOrders { get; }

        /// <summary>
        /// Словарь все мои сделки. Ключ IsinId
        /// </summary>
        ConcurrentDictionary<string, List<MyTrade>> KeyMyTrades { get; }

        /// <summary>
        /// Записать новую мою сделку
        /// </summary>
        /// <param name="trade"></param>
        void SetMyTrade(MyTrade myTrade);

        /// <summary>
        /// Получить все мои сделки для IsinId
        /// </summary>
        /// <param name="isinId"></param>
        /// <returns></returns>
        List<MyTrade> GetMyTradeListFromIsinId(string isinId);

        /// <summary>
        /// Записать ордер пришедший с биржи
        /// </summary>
        /// <param name="trade"></param>
        void SetOrderFromExchange(Order order);

        /// <summary>
        /// Записать ордер отправленный юзером
        /// </summary>
        /// <param name="order"></param>
        void SetOrderFromUser(Order order);

        /// <summary>
        /// Запросить с биржи все мои ордера
        /// </summary>
        Task<List<Order>> GetOrders();

        /// <summary>
        /// Событие, обновился ордер
        /// </summary>
        event newOrderEvent? NewOrderEvent;

        /// <summary>
        /// Событие, пришла новая моя сделка
        /// </summary>
        event newMyTradeEvent? NewMyTradeEvent;


        event eventMessage? EventMessage;
    }
}

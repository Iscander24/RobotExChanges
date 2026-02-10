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
    public interface ITradesService
    {
        /// <summary>
        /// Словарь всех обезличенных сделок. Ключ IsinId
        /// </summary>
        ConcurrentDictionary<string, List<Trade>> KeyTrades { get; }

        /// <summary>
        /// Сохранять или нет обезличенные сделки в памяти.
        /// По умолчанию - Сохранять
        /// </summary>
        /// <param name="saveTrades"></param>
        void SetSaveTrades(bool saveTrades);

        /// <summary>
        /// Записать новую обезличенную сделку
        /// </summary>
        /// <param name="trade"></param>
        void SetTrade(Trade trade);

        /// <summary>
        /// Записать историческую обезличенную сделку
        /// </summary>
        /// <param name="trade"></param>
        void SetHistoryTrade(Trade trade);

        /// <summary>
        /// Получить все сделки для IsinId
        /// </summary>
        /// <param name="isinId"></param>
        /// <returns></returns>
        List<Trade>? GetListTradesFromIsinId(string isinId);

        /// <summary>
        /// Событие, пришла новая обезличенная сделка
        /// </summary>
        event newTradeEvent? NewTradeEvent;

        /// <summary>
        /// Обезличенная сделка, без сохранения всех сделок.
        /// Прямая, без очереди, в основном потоке
        /// </summary>
        event newTradeEvent? LastNewTrade;
    }
}

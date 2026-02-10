using ControllerExChanges.Entity;
using ControllerExChanges.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ControllerExChanges.Interfaces.IConnector;

namespace ControllerExChanges.Interfaces
{
    public interface ICandleManagerToSecurity
    {
        #region Properties =======================================================================


        /// <summary>
        /// Базовые свечи таймфрейм 1min
        /// </summary>
        List<Candle> BaseCandles { get; }

        /// <summary>
        /// Словарь свечей. Ключ => TimeFrame
        /// </summary>
        ConcurrentDictionary<TimeFrame, List<Candle>> DictionaryCandles { get; }

        /// <summary>
        /// Бумага
        /// </summary>
        Security Security { get; }

        #endregion



        #region Methods =========================================================================

        /// <summary>
        /// Подписаться на свечи
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        Task<bool> SubscribeToCandles(TimeFrame timeFrame = TimeFrame.Min1);

        /// <summary>
        /// Отписаться от свечей
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        Task<bool> UnSubscribeToCandles(TimeFrame timeFrame);

        void SetTrade(Trade trade, bool isCreateClusters, decimal ask = decimal.MinValue, decimal bid = decimal.MinValue);


        #endregion

        #region Events =========================================================================

        event eventCancelCandle? EventCancelCandle;

        event eventChangeCandle? EventChangeCandle;

        #endregion
    }
}

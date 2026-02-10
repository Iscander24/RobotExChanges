using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Enums
{
    /// <summary>
    /// stock market conditions
    /// состояние бумаги на бирже
    /// </summary>
    public enum TradingStatus
    {
        /// <summary>
        /// trading on the paper is active
        /// торги по бумаге активны
        /// </summary>
        Active,

        /// <summary>
        /// paper auction is closed.
        /// торги по бумаге закрыты
        /// </summary>
        Closed,

        /// <summary>
        /// we don't know if the bidding's going on
        /// неизвестно, идут ли торги
        /// </summary>
        UnKnown
    }
}

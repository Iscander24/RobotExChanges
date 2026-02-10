using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Enums
{
    /// <summary>
    /// instrumental type
    /// тип инструмента
    /// </summary>
    public enum SecurityType
    {
        /// <summary>
        /// none
        /// не определено
        /// </summary>
        None,

        /// <summary>
        /// currency. Including crypt
        /// валюта. В т.ч. и крипта
        /// </summary>
        Currency,

        /// <summary>
        /// акция
        /// </summary>
        Stock,

        /// <summary>
        /// облигация
        /// </summary>
        Bond,

        /// <summary>
        /// futures
        /// фьючерс
        /// </summary>
        Futures,

        /// <summary>
        /// option
        /// опцион
        /// </summary>
        Option,

        /// <summary>
        /// index индекс
        /// </summary>
        Index
    }
}

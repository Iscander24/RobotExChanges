using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    /// <summary>
    /// class representing one price level in a glass
    /// класс представляющий один ценовой уровень в стакане
    /// </summary>
    public struct MarketDepthLevel
    {

        /// <summary>
        /// number of contracts for sale at this price level
        /// количество контрактов на продажу по этому уровню цены
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// price
        /// цена
        /// </summary>
        public decimal Price { get; set; }

    }
}

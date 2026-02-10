using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Enums
{
    public enum OrderType
    {
        /// <summary>
        /// limit order. Those. bid at a certain price
        /// лимитная заявка. Т.е. заявка по определённой цене
        /// </summary>
        Limit,

        /// <summary>
        /// market application. Those. application at any price
        /// рыночная заявка. Т.е. заявка по любой цене
        /// </summary>
        Market,

        /// <summary>
        /// iceberg application. Those. An application whose volume is not fully visible in the glass.
        /// айсберг заявка. Т.е. заявка объём которой полностью не виден в стакане.
        /// </summary>
        Iceberg,

        /// <summary>
        /// Стоп заявка
        /// </summary>
        Stop,

        /// <summary>
        /// Заявка на закрытие позиции (нужно для Okx)
        /// </summary>
        Take
    }
}

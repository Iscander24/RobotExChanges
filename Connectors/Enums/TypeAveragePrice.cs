using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Enums
{
    public enum TypeAveragePrice
    {
        /// <summary>
        /// Первый вошел, первый вышел
        /// </summary>
        FIFO,
        /// <summary>
        /// Последний вошел, первый вышел
        /// </summary>
        LIFO,
        /// <summary>
        /// Средняя цена
        /// </summary>
        DCA
    }
}

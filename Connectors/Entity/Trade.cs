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
    /// Обезличенная сделка по инструменту
    /// </summary>
    public struct Trade
    {
        public Trade() { }
        
        /// <summary>
        /// The name of the trade's security/Имя бумаги
        /// </summary>
        public string SecurityName { get; set; } = string.Empty;

        /// <summary>
        /// The class code of the trade's security/Код класса
        /// </summary>
        public string SecurityClassCode { get; set; } = string.Empty;

        /// <summary>
        /// instrument code for which the transaction took place
        /// Isin код инструмента по которому прошла сделка
        /// </summary>
        public string IsinId { get; set; } = string.Empty;

        /// <summary>
        /// transaction number in the system
        /// номер сделки в системе
        /// </summary>

        public long Number { get; set; } = 0;

        /// <summary>
        /// volume
        /// объём
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// transaction price
        /// цена сделки
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// deal time
        /// время сделки
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        ///  transaction direction
        /// направление сделки
        /// </summary>
        public Operation Operation { get; set; } = Operation.None;
    }
}

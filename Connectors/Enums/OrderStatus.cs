using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Enums
{
    /// <summary>
    /// Order status
    /// статус Ордера
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// none
        /// отсутствует
        /// </summary>
        None,

        /// <summary>
        /// accepted by the exchange and exhibited in the system
        /// принята биржей и выставленна в систему
        /// </summary>
        Active,

        /// <summary>
        /// waiting for registration
        /// ожидает регистрации
        /// </summary>
        Pending,

        /// <summary>
        /// done
        /// исполнен
        /// </summary>
        Filled,

        /// <summary>
        /// partitial done
        /// исполнен частично
        /// </summary>
        PartiallyFilled,

        /// <summary>
        /// error
        /// произошла ошибка
        /// </summary>
        Failed,

        /// <summary>
        /// cancel
        /// отменён
        /// </summary>
        Canceled
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Enums
{
    /// <summary>
    /// Статус подключения коннектора
    /// </summary>
    public enum ConnectStatus
    {
        /// <summary>
        /// connected
        /// подключен
        /// </summary>
        Connect,

        /// <summary>
        /// disconnected
        /// отключен
        /// </summary>
        Disconnect,

        /// <summary>
        /// Переподключение
        /// </summary>
        Reconnecting
    }
}

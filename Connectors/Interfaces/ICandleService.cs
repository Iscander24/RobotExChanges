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
    public interface ICandleService
    {


        #region Properties =======================================================================

        public ConcurrentDictionary<string, CandleManagerToSecurity> Managers { get; }

        #endregion

        #region Methods =========================================================================

        Task<bool> SubscribeToCandles(Security security,
                                        TimeFrame timeFrame = TimeFrame.Min1,
                                        int countCandles = 1440,
                                        Action<int>? LoadingInfo = null);

        Task<bool> UnSubscribeToCandles(Security security, TimeFrame timeFrame);

        Task<List<Candle>> GetCandles(Security security, 
                                        TimeFrame timeFrame,
                                        int countCandles = 1440,
                                        DateTime? timeStart = null,
                                        DateTime? timeEnd = null,
                                        Action<int>? LoadingInfo = null);

        void SetTrade(Trade trade, bool isCreateClusters, decimal ask = decimal.MinValue, decimal bid = decimal.MinValue);


        #endregion

        #region Events =========================================================================


        event eventCancelCandle? EventCancelCandle;

        
        event eventChangeCandle? EventChangeCandle;

        #endregion
    }
}

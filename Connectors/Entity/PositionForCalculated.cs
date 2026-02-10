using ControllerExChanges.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class PositionForCalculated
    {
        public PositionForCalculated(TypeAveragePrice typeAveragePrice)
        {
            _typeAveragePrice = typeAveragePrice;
        }

        #region Properties ===================================================================================

        /// <summary>
        /// Объём позиции
        /// </summary>
        public decimal Volume
        {
            get => _volume;

            set
            {
                _volume = value;
                CaclulateOpenPrice();
                EventChangePosition?.Invoke();

                if (Volume == 0)
                {
                    EventNewPosition?.Invoke();
                }
            }
        }
        decimal _volume;


        /// <summary>
        /// Средняя цена открытой позиции
        /// </summary>
        public decimal OpenPrice { get; set; }

        /// <summary>
        /// Накопленная прибыль от закрытых сделок
        /// </summary>
        public decimal Accum { get; set; }

        /// <summary>
        /// Сделки, открывающие позицию
        /// </summary>
        public List<MyTradeToCalc> OpenTrades { get; set; } = new List<MyTradeToCalc>();

        #endregion

        #region Fields ===================================================================================

        TypeAveragePrice _typeAveragePrice;

        public List<ClosedMyTrade> ClosedMyTrades = new List<ClosedMyTrade>();

        #endregion

        #region Methods ===================================================================================

        public void SetClosedPosition(decimal closePrice)
        {
            EventClosePosition?.Invoke(closePrice);
        }

        public void Copy(PositionForCalculated nePosition)
        {
            _typeAveragePrice = nePosition.GetTypeAveragePrice();
            OpenTrades = nePosition.OpenTrades;
            Volume = nePosition.Volume;
            OpenPrice = nePosition.OpenPrice;
            Accum = nePosition.Accum;
            ClosedMyTrades = nePosition.ClosedMyTrades;
        }

        public TypeAveragePrice GetTypeAveragePrice()
        {
            return _typeAveragePrice;
        }

        private void CaclulateOpenPrice()
        {
            decimal averagePrice = 0;
            decimal volume = 0;

            foreach (MyTradeToCalc open in OpenTrades)
            {
                if (volume + open.CurrentVolume != 0)
                {
                    averagePrice = (averagePrice * volume + open.Price * open.CurrentVolume) / (volume + open.CurrentVolume);
                }

                volume += open.CurrentVolume;
            }

            OpenPrice = averagePrice;

            if (_typeAveragePrice == TypeAveragePrice.DCA
                && Volume != 0)
            {
                OpenPrice = averagePrice - (Accum / Volume);
            }
        }

        #endregion

        #region Events =========================================================================

        public delegate void eventChangePosition();
        public event eventChangePosition? EventChangePosition;

        public delegate void eventClosePosition(decimal closePrice);
        public event eventClosePosition? EventClosePosition;

        public event eventChangePosition? EventNewPosition;

        #endregion
    }
}

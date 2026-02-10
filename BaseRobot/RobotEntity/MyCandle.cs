

using ControllerExChanges.Entity;
using ControllerExChanges.Enums;

namespace BaseRobot.RobotEntity
{
    public class MyCandle
    {
        public MyCandle() { }

        public MyCandle(Trade trade, TimeFrame timeFrame)
        {
            Open = trade.Price;
            Close = trade.Price;
            High = trade.Price;
            Low = trade.Price;
            Volume = trade.Volume;

            SecurityName = trade.SecurityName;

            TimeFrame = timeFrame;

            SetDateTime(timeFrame, trade.DateTime);
        }

        public DateTime TimeStart { get; set; }

        public TimeFrame TimeFrame { get; set; }

        /// <summary>
        /// Opening price
        /// </summary>
        public decimal Open { get; set; }

        /// <summary>
        /// Maximum price for the period
        /// </summary>
        public decimal High { get; set; }

        /// <summary>
        /// Closing price
        /// </summary>
        public decimal Close { get; set; }

        /// <summary>
        /// Minimum price for the period
        /// </summary>
        public decimal Low { get; set; }

        /// <summary>
        /// Volume
        /// </summary>
        public decimal Volume { get; set; }

        public string SecurityName { get; set; }

        public void AddTick(Trade trade)
        {
            if (trade.Price < Low) Low = trade.Price;
            else if (trade.Price > High) High = trade.Price;

            Close = trade.Price;

            Volume += trade.Volume;  // не убрать ли двойглй объем?
        }

        private void SetDateTime(TimeFrame timeFrame, DateTime dateTime)
        {
            DateTime start = dateTime.Date;

            int dt = (int)((dateTime - start).TotalSeconds / (int)timeFrame);

            TimeStart = start.AddSeconds(dt * (int)timeFrame);
        }

    }
}

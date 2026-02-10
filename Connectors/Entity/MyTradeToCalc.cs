using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class MyTradeToCalc: MyTrade
    {
        public MyTradeToCalc(MyTrade trade)
        {
            Copy(trade);
        }

        public decimal CurrentVolume
        {
            get => Volume - CountedVolume;
        }

        /// <summary>
        /// Посчитанный объём
        /// </summary>
        public decimal CountedVolume = 0;

        public decimal Accum = 0;

        public decimal ClosePrice = 0;

        private void Copy(MyTrade trade)
        {
            this.SecurityClassCode = trade.SecurityClassCode;
            this.SecurityName = trade.SecurityName;
            this.Account = trade.Account;
            this.Number = trade.Number;
            this.NumberPosition = trade.NumberPosition;
            this.IsinId = trade.IsinId;
            this.Volume = trade.Volume;
            this.Price = trade.Price;
            this.DateTime = trade.DateTime;
            this.ParentOrderNumber = trade.ParentOrderNumber;
            this.Operation = trade.Operation;
            this.ComissionExchange = trade.ComissionExchange;
            this.ComissionBroker = trade.ComissionBroker;
            this.ComissionProp = trade.ComissionProp;
            this.ComissionProp2 = trade.ComissionProp2;
        }
    }
}

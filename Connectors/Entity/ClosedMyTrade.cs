using ControllerExChanges.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class ClosedMyTrade
    {
        public ClosedMyTrade(MyTrade myTradeClose,
                                ExchangeType exchangeType,
                                decimal volume,
                                decimal openPrice,
                                DateTime dateTimeOpen,
                                decimal stepPrice,
                                decimal cost,
                                decimal openCommiss)
        {
            IsinId = myTradeClose.IsinId;
            SecName = myTradeClose.SecurityName;
            Account = myTradeClose.Account;
            ExchangeType = exchangeType;
            ClosePrice = myTradeClose.Price;
            OpenPrice = openPrice;
            Volume = volume;
            DateTimeOpen = dateTimeOpen;
            DateTimeClose = myTradeClose.DateTime;
            Operation = myTradeClose.Operation == Operation.Buy ? Operation.Sell : Operation.Buy;   
            Comission = CalcComiss(myTradeClose, Volume, openCommiss);

            Punkts = (int)(Math.Abs(ClosePrice - OpenPrice)/ stepPrice);

            Accum = (ClosePrice - OpenPrice) * Volume / stepPrice * cost;

            if (Operation == Operation.Sell) Accum  = - 1m * Accum;

            if (Accum < 0) Punkts  = - 1 * Punkts;
        }

        public Operation Operation { get; set; }

        public string IsinId { get; private set; }

        public string SecName { get; private set; }

        public string Account { get; private set; }

        public ExchangeType ExchangeType { get; private set; }
        
        public decimal OpenPrice { get; private set; }

        public decimal ClosePrice { get; set; }

        public decimal Volume { get; set; }

        public decimal Accum { get; set; }

        public decimal Result => Accum - Comission;

        public int Punkts { get; set; }

        public decimal Comission { get; set; }

        public DateTime DateTimeOpen { get; private set; }

        public DateTime DateTimeClose { get; set; }

        private decimal CalcComiss(MyTrade myTrade, decimal volume, decimal openCommiss)
        {
            decimal comiss = myTrade.ComissionExchange;
            comiss += myTrade.ComissionBroker;
            comiss += myTrade.ComissionProp;
            comiss += myTrade.ComissionProp2;

            comiss /= myTrade.Volume;

            return comiss * volume + openCommiss;
        }
    }
}

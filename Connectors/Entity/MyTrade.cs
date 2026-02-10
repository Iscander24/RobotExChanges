using ControllerExChanges.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class MyTrade 
    {
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

        /// <summary>
        /// parent's warrant number
        /// номер ордера родителя
        /// </summary>
        public long ParentOrderNumber { get; set; } = 0;

        /// <summary>
        /// the robot's position number 
        /// номер позиции у робота 
        /// </summary>
        public string NumberPosition { get; set; } = string.Empty;

        /// <summary>
        /// Номер счета
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// Комиссия биржи
        /// </summary>
        public decimal ComissionExchange { get; set; }

        /// <summary>
        /// Комиссия брокера
        /// comission value
        /// </summary>
        public decimal ComissionBroker { get; set; }

        /// <summary>
        /// Комиссия Проп компании 
        /// </summary>
        public decimal ComissionProp { get; set; }

        /// <summary>
        /// Комиссия Проп компании 2
        /// </summary>
        public decimal ComissionProp2 { get; set; }


        public  MyTrade Copy()
        {
            MyTrade myTrade = new MyTrade();
            myTrade.SecurityClassCode= SecurityClassCode;
            myTrade.SecurityName= SecurityName;
            myTrade.Account= Account;
            myTrade.Number = Number;
            myTrade.NumberPosition = NumberPosition;
            myTrade.IsinId = IsinId;
            myTrade.Volume = Volume;
            myTrade.Price = Price;
            myTrade.DateTime = DateTime;
            myTrade.ParentOrderNumber = ParentOrderNumber;
            myTrade.Operation = Operation;
            myTrade.ComissionExchange = ComissionExchange;
            myTrade.ComissionBroker = ComissionBroker;
            myTrade.ComissionProp = ComissionProp;
            myTrade.ComissionProp2 = ComissionProp2;

            return myTrade;
        }
    }
}

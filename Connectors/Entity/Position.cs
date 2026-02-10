using ControllerExChanges.Entity;
using ControllerExChanges.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class Position
    {
        public Position(Security? security,
                            ILogger? logger,
                            ExchangeType exchangeType)
        {
            Security = security;
            _positionForCalculated = new PositionForCalculated(TypeAveragePrice);

            _exchangeType = exchangeType;

            _positionForCalculated.EventChangePosition += _positionForCalculated_EventChangePosition;
            _positionForCalculated.EventNewPosition += _positionForCalculated_EventNewPosition;
            _positionForCalculated.EventClosePosition += _positionForCalculated_EventClosePosition;

            if (logger != null)
            {
                _logger = logger.ForContext<Position>();
            }            
        }        

        #region Properties =======================================================================

        /// <summary>
        /// Бумага, по которой открыта позиция
        /// </summary>
        public Security? Security { get; set; }

        public Operation Operation { get; set; } = Operation.None;

        /// <summary>
        /// Объём позиции
        /// </summary>
        public decimal Volume => _positionForCalculated.Volume;

        /// <summary>
        /// Средняя цена открытой позиции
        /// </summary>
        public decimal OpenPrice
        {
            get
            {
                if ( Security != null)
                {
                    return ((int)(_positionForCalculated.OpenPrice / Security.PriceStep)) * Security.PriceStep;
                }

                return _positionForCalculated.OpenPrice;
            }                
        }

        /// <summary>
        /// Накопленная прибыль от закрытых сделок
        /// </summary>
        public decimal Accum => _positionForCalculated.Accum;


        public decimal Margine { get; set; }

        /// <summary>
        /// Все сделки позиции
        /// </summary>
        public List<MyTrade> MyTrades { get; set; } = new List<MyTrade>();

        /// <summary>
        /// Ордера, относящиеся к этой позиции
        /// </summary>
        public List<Order> Orders { get; set; } = new List<Order>();


        public TypeAveragePrice TypeAveragePrice
        {
            get => _typeAveragePrice;

            set
            {
                _typeAveragePrice = value;
                ReCalcToNewTypeAveragePrice();
            }
        }
        TypeAveragePrice _typeAveragePrice = Enums.TypeAveragePrice.FIFO;

        public ExchangeType ExchangeType => _exchangeType;

        #endregion

        #region Fields ========================================================================

        PositionForCalculated _positionForCalculated;

        ILogger? _logger = null;

        ExchangeType _exchangeType;

        #endregion

        #region Methods =======================================================================

        public List<MyTradeToCalc> GetOpenTrades()
        {
            return _positionForCalculated.OpenTrades;
        }

        public PositionForCalculated GetPositionForCalculated()
        {
            return _positionForCalculated;
        }

        public void AddOrder(Order order)
        {
            if (order.NumberMarket == 0) return;

            Order? item = Orders.Find(item => item.NumberMarket == order.NumberMarket);


            if (item == null) Orders.Add(order);
            else item = order;

            _logger?.Information("Method{@Method}, Order{@Order}", nameof(AddOrder), order);
        }

        /// <summary>
        /// Добавить сделку. Возвращает true, если сделка принадлежит данной позиции.
        /// Возвращает false, если сделка новая в этой позиции
        /// </summary>
        /// <param name="trade"></param>
        /// <returns></returns>
        public virtual bool AddMyTrade(MyTrade trade)
        {
            //if (Orders.Find(order => order.NumberMarket == trade.ParentOrderNumber) == null) return false;

            if (MyTrades.Find(item => item.Number == trade.Number) != null) return false;            

            MyTrade myTrade = trade.Copy();

            MyTrades.Add(myTrade);

            _logger?.Information("Method{@Method}, MyTrade{@MyTrade}", nameof(AddMyTrade), myTrade);

            AddMyTrade(myTrade, _positionForCalculated, TypeAveragePrice);

            if (_positionForCalculated.Volume == 0) EventClosePosition?.Invoke(this, myTrade.Price);

            return true;
        }

        /// <summary>
        /// Обнылить позицию
        /// </summary>
        public void Clear()
        {
            MyTrades.Clear();
            Orders.Clear();
            Operation = Operation.None;
            _positionForCalculated.Accum = 0;
            _positionForCalculated.Volume = 0;
            _positionForCalculated.OpenPrice = 0;
            _positionForCalculated.OpenTrades.Clear();
            _positionForCalculated.ClosedMyTrades.Clear();
        }

        public void PositionCorrecting(decimal newVolume, decimal newOpenPrice)
        {
            if (newOpenPrice == OpenPrice)
            {
                if (newVolume == Volume) return;


            }
            else
            {

            }
        }

        private void ReCalcToNewTypeAveragePrice()
        {
            if (MyTrades.Count == 0) return;

            PositionForCalculated position = new PositionForCalculated(TypeAveragePrice);

            foreach (MyTrade trade in MyTrades)
            {
                AddMyTrade(trade, position, TypeAveragePrice);
            }

            _positionForCalculated.Copy(position);
        }

        private void AddMyTrade(MyTrade myTrade, PositionForCalculated position, TypeAveragePrice typeAveragePrice)
        {
            if (position.Volume == 0
                || (position.Volume > 0 && myTrade.Operation == Operation.Buy)
                || (position.Volume < 0 && myTrade.Operation == Operation.Sell))
            {
                position.OpenTrades.Add(new MyTradeToCalc(myTrade));

                if (myTrade.Operation == Operation.Buy) position.Volume += myTrade.Volume;
                else position.Volume -= myTrade.Volume;

                
            }
            else if ((position.Volume > 0 && myTrade.Operation == Operation.Sell)
                      || (position.Volume < 0 && myTrade.Operation == Operation.Buy))
            {
                MyTradeToCalc myTradeToCalc = new MyTradeToCalc(myTrade);

                if (Math.Abs(position.Volume) >= myTrade.Volume)
                {
                    Calculate(typeAveragePrice, myTradeToCalc, position);

                    if (myTrade.Operation == Operation.Buy) position.Volume += myTrade.Volume;
                    else position.Volume -= myTrade.Volume;

                    if (position.Volume == 0)
                    {
                        position.SetClosedPosition(myTrade.Price);

                        position.Volume = 0;
                        position.OpenPrice = 0;
                        position.Accum = 0;
                        position.OpenTrades.Clear();
                        //position.ClosedMyTrades.Clear();
                    }
                }
                else
                {
                    myTradeToCalc.Volume = Math.Abs(position.Volume);

                    Calculate(typeAveragePrice, myTradeToCalc, position);

                    position.SetClosedPosition(myTrade.Price);

                    MyTrade newMyTrade = myTrade.Copy();
                    newMyTrade.Volume -= Math.Abs(position.Volume);

                    position.Volume = 0;
                    position.OpenPrice = 0;
                    position.Accum = 0;
                    position.OpenTrades.Clear();
                    position.ClosedMyTrades.Clear();

                    AddMyTrade(newMyTrade, position, typeAveragePrice);
                }
            }
        }       


        private void Calculate(TypeAveragePrice typeAveragePrice, MyTradeToCalc myTrade, PositionForCalculated position)
        {
            switch (typeAveragePrice)
            {
                case TypeAveragePrice.FIFO:
                    CalcFIFO(myTrade, position);
                    break;

                case TypeAveragePrice.LIFO:
                    CalcLIFO(myTrade, position);
                    break;

                case TypeAveragePrice.DCA:
                    CalcFIFO(myTrade, position);
                    break;
            }
        }

        /// <summary>
        /// Посчитать сделку.
        /// Новая сделка поставляется противоположной открываемым и меньшего объёма, чем есть открытая позиция
        /// </summary>
        /// <param name="myTrade"></param>
        private void CalcFIFO(MyTradeToCalc myTrade,
                                PositionForCalculated position)
        {
            decimal accum = 0;

            if (Security == null) return;

            foreach (MyTradeToCalc open in position.OpenTrades)
            {
                decimal volume = open.Volume - open.CountedVolume;

                if (volume > 0)
                {
                    decimal res = 0;

                    if (volume >= myTrade.CurrentVolume)
                    {
                        res = (myTrade.Price - open.Price) * myTrade.CurrentVolume;

                        position.ClosedMyTrades.Add(new ClosedMyTrade(myTrade,
                                                                        _exchangeType,
                                                                        myTrade.CurrentVolume,
                                                                        open.Price,
                                                                        open.DateTime,
                                                                        Security.PriceStep,
                                                                        Security.PriceStepCost,
                                                                        open.ComissionBroker + open.ComissionExchange + open.ComissionProp + open.ComissionProp2));

                        open.CountedVolume += myTrade.CurrentVolume;

                        myTrade.CountedVolume += myTrade.CurrentVolume;

                        accum += res;                        

                        break;
                    }
                    else
                    {
                        res = (myTrade.Price - open.Price) * volume;

                        position.ClosedMyTrades.Add(new ClosedMyTrade(myTrade,
                                                                        _exchangeType,
                                                                        volume,
                                                                        open.Price,
                                                                        open.DateTime,
                                                                        Security.PriceStep,
                                                                        Security.PriceStepCost,
                                                                        open.ComissionBroker + open.ComissionExchange + open.ComissionProp + open.ComissionProp2));

                        open.CountedVolume += volume;

                        myTrade.CountedVolume += volume;
                    }

                    open.Accum += res;

                    open.ComissionExchange += myTrade.ComissionExchange;
                    open.ComissionBroker += myTrade.ComissionBroker;
                    open.ComissionProp += myTrade.ComissionProp;
                    open.ComissionProp2 += myTrade.ComissionProp2;

                    accum += res;
                }
            }

            position.Accum += accum;
        }

        private void CalcLIFO(MyTradeToCalc myTrade, PositionForCalculated position)
        {
            decimal accum = 0;

            if (Security == null) return;

            for (int i= position.OpenTrades.Count-1; i >= 0; i--)
            {
                MyTradeToCalc open = position.OpenTrades[i];

                decimal volume = open.Volume - open.CountedVolume;

                if (volume > 0)
                {
                    decimal res = 0;

                    if (volume >= myTrade.CurrentVolume)
                    {
                        res = (myTrade.Price - open.Price) * myTrade.CurrentVolume;

                        position.ClosedMyTrades.Add(new ClosedMyTrade(myTrade,
                                                                        _exchangeType,
                                                                        myTrade.CurrentVolume,
                                                                        open.Price,
                                                                        open.DateTime,
                                                                        Security.PriceStep,
                                                                        Security.PriceStepCost,
                                                                        open.ComissionBroker + open.ComissionExchange + open.ComissionProp + open.ComissionProp2));

                        open.CountedVolume += myTrade.CurrentVolume;

                        myTrade.CountedVolume += myTrade.CurrentVolume;

                        accum += res;

                        break;
                    }
                    else
                    {
                        res = (myTrade.Price - open.Price) * volume;

                        position.ClosedMyTrades.Add(new ClosedMyTrade(myTrade,
                                                                        _exchangeType,
                                                                        volume,
                                                                        open.Price,
                                                                        open.DateTime,
                                                                        Security.PriceStep,
                                                                        Security.PriceStepCost,
                                                                        open.ComissionBroker + open.ComissionExchange + open.ComissionProp + open.ComissionProp2));

                        open.CountedVolume += volume;

                        myTrade.CountedVolume += volume;
                    }

                    open.Accum += res;

                    open.ComissionExchange += myTrade.ComissionExchange;
                    open.ComissionBroker += myTrade.ComissionBroker;
                    open.ComissionProp += myTrade.ComissionProp;
                    open.ComissionProp2 += myTrade.ComissionProp2;

                    accum += res;
                }
            }

            position.Accum += accum;
        }


        private void _positionForCalculated_EventClosePosition(decimal closePrice)
        {
            EventClosePosition?.Invoke(this, closePrice);
        }

        private void _positionForCalculated_EventNewPosition()
        {
            EventNewPosition?.Invoke(this);
        }

        private void _positionForCalculated_EventChangePosition()
        {
            EventChangePosition?.Invoke(this);
        }

        #endregion

        #region Events =========================================================================

        public delegate void eventChangePosition(Position position);
        public event eventChangePosition? EventChangePosition;

        public delegate void eventClosePosition(Position position, decimal closePrice);
        public event eventClosePosition? EventClosePosition;

        public event eventChangePosition? EventNewPosition;

        #endregion
    }
}
using Binance.Net.Objects.Models.Futures;
using Bybit.Net.Enums;
using Bybit.Net.Objects.Models.V5;
using OrderStatus = Bybit.Net.Enums.OrderStatus;


namespace ControllerExChanges.Connectors.Bybit
{
    internal static class Extentions
    {
        internal static Entity.MyTrade Set(this BybitUserTrade bybitUserTrade,
                                            Enums.ExchangeType exchangeType,
                                            string accountName,
                                            string referal,
                                            Entity.Security security)
        {
            Entity.MyTrade myTrade = new Entity.MyTrade();

            myTrade.SecurityClassCode = security.ClassCode;
            myTrade.SecurityName = bybitUserTrade.Symbol;
            myTrade.Number = long.TryParse(bybitUserTrade.TradeId, out var id) ? id : 0;
            myTrade.ParentOrderNumber = long.TryParse(bybitUserTrade.OrderId, out var idOrd) ? idOrd : 0;
            myTrade.Price = bybitUserTrade.Price;
            myTrade.Volume = bybitUserTrade.Quantity;
            myTrade.DateTime = bybitUserTrade.Timestamp;
            myTrade.Account = accountName;
            myTrade.IsinId = bybitUserTrade.Symbol; // accountName + 
            myTrade.Operation = bybitUserTrade.Side == OrderSide.Buy ? Enums.Operation.Buy : Enums.Operation.Sell;

            return myTrade;
        }




        internal static Entity.Order Set(this BybitOrder order,
                                            Enums.ExchangeType exchangeType,
                                            string accountName,
                                            string referal,
                                            Entity.Security security)
        {
            Entity.Order newOrder = new Entity.Order();

            newOrder.Status = GetStatus(order.Status);
            newOrder.NumberMarket = long.TryParse(order.OrderId, out var id) ? id : 0;
            newOrder.IsinId = order.Symbol; // accountName + 
            newOrder.Account = accountName;
            newOrder.Price = order.Price ?? 0m;
            newOrder.ExchangeType = exchangeType;
            newOrder.OrderType = GetOrderType(order.OrderType);
            newOrder.Operation = order.Side == OrderSide.Buy ? Enums.Operation.Buy : Enums.Operation.Sell;
            if (order.CreateTime > DateTime.MinValue) newOrder.TimeCreated = order.CreateTime;
            newOrder.TimeCallBack = newOrder.Status == Enums.OrderStatus.Active ? order.UpdateTime : DateTime.MinValue;
            newOrder.TimeCanceled = newOrder.Status == Enums.OrderStatus.Canceled ? order.UpdateTime : DateTime.MinValue;
            newOrder.TimeFilled = newOrder.Status == Enums.OrderStatus.Filled ? order.UpdateTime : DateTime.MinValue;
            newOrder.Comment = order.ClientOrderId.Replace(referal, "");
            newOrder.SecurityName = order.Symbol;
            newOrder.ClassCode = security.ClassCode;
            newOrder.Volume = order.Quantity;
            newOrder.VolumeFilled = order.QuantityFilled ?? 0m;
            //GetMyTrades(order.MyTrades, ref newOrder);

            return newOrder;
        }


        //internal static Entity.Order Set(this BinanceFuturesStreamOrderUpdateData order,
        //                                    Enums.ExchangeType exchangeType,
        //                                    string accountName,
        //                                    string referal)
        //{
        //    Entity.Order newOrder = new Entity.Order();

        //    newOrder.Status = GetStatus(order.Status);
        //    newOrder.NumberMarket = order.OrderId;
        //    newOrder.IsinId = order.Symbol;  // accountName + 
        //    newOrder.Account = accountName;
        //    newOrder.Price = order.Price;
        //    newOrder.ExchangeType = exchangeType;
        //    newOrder.OrderType = GetOrderType(order.Type);
        //    newOrder.Operation = order.Side == OrderSide.Buy ? Enums.Operation.Buy : Enums.Operation.Sell;
        //    //newOrder.TimeCreated = order.UpdateTime;
        //    newOrder.TimeCallBack = newOrder.Status == Enums.OrderStatus.Active ? order.UpdateTime : DateTime.MinValue;
        //    newOrder.TimeCanceled = newOrder.Status == Enums.OrderStatus.Canceled ? order.UpdateTime : DateTime.MinValue;
        //    newOrder.TimeFilled = newOrder.Status == Enums.OrderStatus.Filled ? order.UpdateTime : DateTime.MinValue;
        //    newOrder.Comment = order.ClientOrderId.Replace(referal, "");
        //    newOrder.SecurityName = order.Symbol;
        //    newOrder.Volume = order.Quantity;
        //    newOrder.VolumeFilled = order.QuantityOfLastFilledTrade;
        //    //GetMyTrades(order.MyTrades, ref newOrder);

        //    return newOrder;
        //}

        internal static Enums.OrderStatus GetStatus(OrderStatus orderStatus)
        {
            switch (orderStatus)
            {
                case OrderStatus.New:
                    return Enums.OrderStatus.Active;

                case OrderStatus.PartiallyFilled:
                    return Enums.OrderStatus.PartiallyFilled;

                case OrderStatus.Filled:
                    return Enums.OrderStatus.Filled;

            }

            return Enums.OrderStatus.Canceled;
        }

        internal static Enums.OrderType GetOrderType(OrderType orderType)
        {
            switch (orderType)
            {
                case OrderType.Limit: return Enums.OrderType.Limit;

                case OrderType.Market: return Enums.OrderType.Market;

                case OrderType.LimitMaker: return Enums.OrderType.Limit;
            }

            return Enums.OrderType.Limit;
        }
    }
}

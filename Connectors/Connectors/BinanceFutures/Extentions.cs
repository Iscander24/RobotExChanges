using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Futures.Socket;

namespace ControllerExChanges.Connectors.BinanceFutures
{
    internal static class Extentions
    {
        internal static Entity.Candle Set(this IBinanceStreamKline kline, bool isCreateClusters)
        {
            Enums.TimeFrame timeFrame = kline.Interval.Set();


            Entity.Candle candle = new Entity.Candle(timeFrame, new Entity.Trade()
            {
                DateTime = kline.OpenTime,
                Price = kline.OpenPrice,
                Volume = kline.Volume,
            },
            isCreateClusters: isCreateClusters);

            candle.AddTick(new Entity.Trade() { DateTime = kline.OpenTime, Price = kline.HighPrice });
            candle.AddTick(new Entity.Trade() { DateTime = kline.OpenTime, Price = kline.LowPrice });
            candle.AddTick(new Entity.Trade() { DateTime = kline.CloseTime, Price = kline.ClosePrice });

            return candle;
        }

        internal static KlineInterval Set(this Enums.TimeFrame timeFrame)
        {
            switch (timeFrame)
            {
                case Enums.TimeFrame.Min1: return KlineInterval.OneMinute;
                case Enums.TimeFrame.Min2: return KlineInterval.OneMinute;
                case Enums.TimeFrame.Min3: return KlineInterval.ThreeMinutes;
                case Enums.TimeFrame.Min5: return KlineInterval.FiveMinutes;
                case Enums.TimeFrame.Min15: return KlineInterval.FifteenMinutes;
                case Enums.TimeFrame.Min30: return KlineInterval.ThirtyMinutes;
                case Enums.TimeFrame.Hour1: return KlineInterval.OneHour;
                case Enums.TimeFrame.Hour2: return KlineInterval.TwoHour;
                case Enums.TimeFrame.Hour4: return KlineInterval.FourHour;
                case Enums.TimeFrame.Day: return KlineInterval.OneDay;

            }

            return KlineInterval.OneMinute;
        }

        internal static Enums.TimeFrame Set(this KlineInterval klineInterval)
        {
            switch (klineInterval)
            {
                case KlineInterval.OneMinute: return Enums.TimeFrame.Min1;
                case KlineInterval.ThreeMinutes: return Enums.TimeFrame.Min3;
                case KlineInterval.FiveMinutes: return Enums.TimeFrame.Min5;
                case KlineInterval.FifteenMinutes: return Enums.TimeFrame.Min15;
                case KlineInterval.ThirtyMinutes: return Enums.TimeFrame.Min30;
                case KlineInterval.OneHour: return Enums.TimeFrame.Hour1;
                case KlineInterval.TwoHour: return Enums.TimeFrame.Hour2;
                case KlineInterval.FourHour: return Enums.TimeFrame.Hour4;
                case KlineInterval.OneDay: return Enums.TimeFrame.Day;
            }

            return Enums.TimeFrame.Min1;
        }

        internal static Entity.Candle Set(this IBinanceKline kline, Enums.TimeFrame timeFrame, bool isCreateClusters)
        {
            Entity.Candle candle = new Entity.Candle(timeFrame, new Entity.Trade()
            {
                DateTime = kline.OpenTime,
                Price = kline.OpenPrice,
                Volume = kline.Volume,
            },
            isCreateClusters: isCreateClusters);

            candle.AddTick(new Entity.Trade() { DateTime = kline.OpenTime, Price = kline.HighPrice });
            candle.AddTick(new Entity.Trade() { DateTime = kline.OpenTime, Price = kline.LowPrice });
            candle.AddTick(new Entity.Trade() { DateTime = kline.CloseTime, Price = kline.ClosePrice });

            return candle;
        }

        internal static Entity.MyTrade Set(this BinanceFuturesUsdtTrade binanceTrade,
                                            Enums.ExchangeType exchangeType,
                                            string accountName,
                                            string referal,
                                            Entity.Security security)
        {
            Entity.MyTrade myTrade = new Entity.MyTrade();

            myTrade.SecurityClassCode = security.ClassCode;
            myTrade.SecurityName = binanceTrade.Symbol;
            myTrade.Number = binanceTrade.Id;
            myTrade.ParentOrderNumber = binanceTrade.OrderId;
            myTrade.Price = binanceTrade.Price;
            myTrade.Volume = binanceTrade.Quantity;
            myTrade.DateTime = binanceTrade.Timestamp;
            myTrade.Account = accountName;
            myTrade.IsinId = binanceTrade.Symbol; // accountName + 
            myTrade.Operation = binanceTrade.Buyer ? Enums.Operation.Buy : Enums.Operation.Sell;

            if (binanceTrade.FeeAsset != "BNB") myTrade.ComissionExchange = binanceTrade.Fee;

            return myTrade;
        }

        internal static Entity.Order Set(this BinanceFuturesOrder order,
                                            Enums.ExchangeType exchangeType,
                                            string accountName,
                                            string referal,
                                            Entity.Security security)
        {
            Entity.Order newOrder = new Entity.Order();

            newOrder.Status = GetStatus(order.Status);
            newOrder.NumberMarket = order.Id;
            newOrder.IsinId = order.Symbol; // accountName + 
            newOrder.Account = accountName;
            newOrder.Price = order.Price;
            newOrder.ExchangeType = exchangeType;
            newOrder.OrderType = GetOrderType(order.Type);
            newOrder.Operation = order.Side == OrderSide.Buy ? Enums.Operation.Buy : Enums.Operation.Sell;
            if (order.CreateTime > DateTime.MinValue) newOrder.TimeCreated = order.CreateTime;
            newOrder.TimeCallBack = newOrder.Status == Enums.OrderStatus.Active ? order.UpdateTime : DateTime.MinValue;
            newOrder.TimeCanceled = newOrder.Status == Enums.OrderStatus.Canceled ? order.UpdateTime : DateTime.MinValue;
            newOrder.TimeFilled = newOrder.Status == Enums.OrderStatus.Filled ? order.UpdateTime : DateTime.MinValue;
            newOrder.Comment = order.ClientOrderId.Replace(referal, "");
            newOrder.SecurityName = order.Symbol;
            newOrder.ClassCode = security.ClassCode;
            newOrder.Volume = order.Quantity;
            newOrder.VolumeFilled = order.QuantityFilled;
            //GetMyTrades(order.MyTrades, ref newOrder);

            return newOrder;
        }

        internal static Entity.Order Set(this BinanceFuturesStreamOrderUpdateData order,
                                            Enums.ExchangeType exchangeType,
                                            string accountName,
                                            string referal)
        {
            Entity.Order newOrder = new Entity.Order();

            newOrder.Status = GetStatus(order.Status);
            newOrder.NumberMarket = order.OrderId;
            newOrder.IsinId = order.Symbol;  // accountName + 
            newOrder.Account = accountName;
            newOrder.Price = order.Price;
            newOrder.ExchangeType = exchangeType;
            newOrder.OrderType = GetOrderType(order.Type);
            newOrder.Operation = order.Side == OrderSide.Buy ? Enums.Operation.Buy : Enums.Operation.Sell;
            //newOrder.TimeCreated = order.UpdateTime;
            newOrder.TimeCallBack = newOrder.Status == Enums.OrderStatus.Active ? order.UpdateTime : DateTime.MinValue;
            newOrder.TimeCanceled = newOrder.Status == Enums.OrderStatus.Canceled ? order.UpdateTime : DateTime.MinValue;
            newOrder.TimeFilled = newOrder.Status == Enums.OrderStatus.Filled ? order.UpdateTime : DateTime.MinValue;
            newOrder.Comment = order.ClientOrderId.Replace(referal, "");
            newOrder.SecurityName = order.Symbol;
            newOrder.Volume = order.Quantity;
            newOrder.VolumeFilled = order.QuantityOfLastFilledTrade;
            //GetMyTrades(order.MyTrades, ref newOrder);

            return newOrder;
        }

        internal static Enums.OrderType GetOrderType(FuturesOrderType orderType)
        {
            switch (orderType)
            {
                case FuturesOrderType.Limit: return Enums.OrderType.Limit;

                case FuturesOrderType.Market: return Enums.OrderType.Market;

                case FuturesOrderType.StopMarket: return Enums.OrderType.Stop;

                case FuturesOrderType.TakeProfitMarket: return Enums.OrderType.Limit;
            }

            return Enums.OrderType.Limit;
        }

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
    }
}

using ControllerExChanges.Entity;
using ControllerExChanges.Interfaces;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ControllerExChanges.Interfaces.IConnector;

namespace ControllerExChanges.Services
{
    // Не пытаемся сохранить единую ссылку на первоначальный ордер.
    // Каждое событие по ордеру, может порождать новый обьект. 
    // Для учета ордеров в роботах, нужно хранить номера ордеров или комментарии


    public class OrdersService : IOrdersService
    {
        /// <summary>
        /// Сервис для обработки ордеров и сделок пользователя
        /// </summary>
        /// <param name="logger"></param>
        public OrdersService(ControllerLogger logger)
        {
            _logger = logger.Logger.ForContext<OrdersService>();

            _global = logger.Logger;

            Init();
        }

        ~OrdersService()
        {
            _isRun = false;
        }

        #region Properties ================================================================================

        public ConcurrentDictionary<long, Order> KeyMyOrders => _keyOrders;

        public ConcurrentDictionary<string, Order> SentOrders => _sentOrders;

        public ConcurrentDictionary<string, List<MyTrade>> KeyMyTrades => _keyMyTrades;

        #endregion

        #region Fields ===================================================================================

        /// <summary>
        /// Словарь всех ордеров. Ключ NumberMarket
        /// </summary>
        ConcurrentDictionary<long, Order> _keyOrders = new ConcurrentDictionary<long, Order>();

        /// <summary>
        /// Все ордера отправленные на регистрацию бирже. Ключ = comment
        /// </summary>
        ConcurrentDictionary<string, Order> _sentOrders = new ConcurrentDictionary<string, Order>();

        /// <summary>
        /// Новые ордера в конвеере, ожидающие отправки потребителям через событие
        /// </summary>
        ConcurrentQueue<Order> _newOrders = new ConcurrentQueue<Order>();

        /// <summary>
        /// Словарь все мои сделки. Ключ IsinId
        /// </summary>
        ConcurrentDictionary<string, List<MyTrade>> _keyMyTrades = new ConcurrentDictionary<string, List<MyTrade>>();

        /// <summary>
        /// Новые мои сделки в конвеере, ожидающие отправки потребителям через событие
        /// </summary>
        ConcurrentQueue<MyTrade> _newMyTrades = new ConcurrentQueue<MyTrade>();

        /// <summary>
        /// Словарь списков сделок, ожидающих своего ордера (гипотетическая ситуация)
        /// </summary>
        ConcurrentDictionary<long, List<MyTrade>> _waitingMyTrades = new ConcurrentDictionary<long, List<MyTrade>>();

        ILogger _logger;

        ILogger _global;

        bool _isRun = true;

        #endregion

        #region Methods ==================================================================================  
        
        private void Init()
        {
            Task.Run(() =>
            {
                Transporter();
            });
        }

        public void SetOrderFromExchange(Order newOrder)
        {
            _logger.Information("{@MethodName}, new Order {@Order} ", nameof(SetOrderFromExchange), newOrder);

            if (newOrder.Status == Enums.OrderStatus.Failed)
            {
                EventMessage?.Invoke(new Message(title: "Order failed",
                                                    text: newOrder.Comment,
                                                    account: newOrder.Account,
                                                    securityName: newOrder.SecurityName));
            }

            _keyOrders.AddOrUpdate(newOrder.NumberMarket, newOrder, (key, value) => value = newOrder);            

            if (_sentOrders.TryGetValue(newOrder.Comment, out Order? userOrder))
            {
                userOrder.NumberMarket = newOrder.NumberMarket;
                userOrder.Volume = newOrder.Volume;
                userOrder.VolumeFilled = newOrder.VolumeFilled;
                userOrder.Status = newOrder.Status;
                userOrder.TimeCallBack = newOrder.TimeCallBack;
                userOrder.TimeCanceled = newOrder.TimeCanceled;
                userOrder.TimeFilled = newOrder.TimeFilled;
                if (newOrder.TimeCreated == DateTime.MinValue)
                {
                    newOrder.TimeCreated = userOrder.TimeCreated;
                }
            }

            _newOrders.Enqueue(newOrder);
        }

        public void SetOrderFromUser(Order order)
        {
            if (order.TimeCreated == DateTime.MinValue) order.TimeCreated = DateTime.UtcNow;

            _sentOrders.AddOrUpdate(order.Comment, order, (key, value) => value = order);

            _logger.Information("{@MethodName}, send Order {@Order} ", nameof(SetOrderFromUser), order);
        }



        public List<MyTrade> GetMyTradeListFromIsinId(string isinId)
        {
            List<MyTrade>? myTradeList = null;

            _keyMyTrades.TryGetValue(isinId, out myTradeList);

            if (myTradeList == null) { myTradeList = new List<MyTrade>(); }

            //_logger.Verbose("{@MethodName}, myTradeList.Count = {count} ", nameof(GetMyTradeListFromIsinId), myTradeList.Count);

            return myTradeList;
        }

        public void SetMyTrade(MyTrade myTrade)
        {
            //_logger.Information("{@MethodName}, MyTrade {@MyTrade} ", nameof(SetMyTrade), myTrade);

            List<MyTrade>? myTradeList = null;

            if (_keyMyTrades.TryGetValue(myTrade.IsinId, out myTradeList))
            {
                if (myTradeList.Find(trade => trade.Number == myTrade.Number) != null) return;

                myTradeList.Add(myTrade);
            }
            else
            {
                myTradeList = new List<MyTrade>() { myTrade };

                _keyMyTrades.AddOrUpdate(myTrade.IsinId, myTradeList, (key, value) => value = myTradeList);
            }

            if (SendToOrder(myTrade)) _newMyTrades.Enqueue(myTrade);
        }

        /// <summary>
        /// Распределяем сделки по ордерам.
        /// Если такого ордера еще нет с биржи (на всякий случай), то отправляем в словарь ожидания
        /// </summary>
        /// <param name="myTrade"></param>
        /// <returns></returns>
        private bool SendToOrder(MyTrade myTrade)
        {
            Order? order = null;

            if (_keyOrders.TryGetValue(myTrade.ParentOrderNumber, out order))
            {
                // Если такой родительский ордер есть
                //order.SetTrade(myTrade);

                return true;
            }

            // Если такой ордер еще не пришёл с биржи, отправляем в лист ожидания

            List<MyTrade>? myTrades = null;

            if (_waitingMyTrades.TryGetValue(myTrade.ParentOrderNumber, out myTrades))
            {
                myTrades.Add(myTrade);
            }
            else
            {
                myTrades = new List<MyTrade>() { myTrade };

                _waitingMyTrades.AddOrUpdate(myTrade.ParentOrderNumber, myTrades, (key, value) => value = myTrades);
            }

            return false;
        }

        private Order? CheckWaitingMyTrades()
        {
            foreach (var item in _waitingMyTrades)
            {
                for (int i=0; i< item.Value.Count; i++)
                {
                    if (_keyOrders.TryGetValue(item.Value[i].ParentOrderNumber, out Order? order))
                    {
                        // Если такой родительский ордер есть

                        return order;
                    }
                }
            }            

            return null;
        }

        /// <summary>
        /// Конвейер для ордеров в отдельном потоке
        /// </summary>
        private async void Transporter()
        {
            while (_isRun)
            {
                if (_newOrders.TryDequeue(out Order? order))
                {
                    try
                    {
                        _logger.Debug("{@MethodName}, NewOrderEvent = {@Order}", nameof(Transporter), order);

                        NewOrderEvent?.Invoke(order);

                        //using(ContextDB contextDB = new ContextDB(_global))
                        //{
                        //    //contextDB.Orders.Find(order);
                        //}

                        // Проверяем, нет ли сделок совершенных по этому ордеру
                        // и пришедших раньше ордера

                        RemoveWaitingMyTrades(order);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("{@MethodName}, Exception = {@Exception}", nameof(Transporter), ex);
                    }
                }

                if (_newMyTrades.TryDequeue(out MyTrade? myTrade))
                {
                    try
                    {
                        _logger.Debug("{@MethodName}, NewMyTradeEvent = {@MyTrade}", nameof(Transporter), myTrade);

                        NewMyTradeEvent?.Invoke(myTrade);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("{@MethodName}, Exception = {@Exception}", nameof(Transporter), ex);
                    }
                }

                if (_waitingMyTrades.Count > 0)
                {
                    Order? ord = CheckWaitingMyTrades();

                    if (ord != null)
                    {
                        RemoveWaitingMyTrades(ord);
                    }
                }

                await Task.Delay(1);
            }

            if (_isRun)
            {
                Init();
            }
        }

        private void RemoveWaitingMyTrades(Order order)
        {
            if (_waitingMyTrades.TryRemove(order.NumberMarket, out List<MyTrade>? myTrades))
            {
                for (int i = 0; i < myTrades.Count; i++)
                {
                    _newMyTrades.Enqueue(myTrades[i]);

                    //_logger.Debug("{@MethodName}, MyTrade {@MyTrade}", nameof(RemoveWaitingMyTrades), myTrades[i]);
                }

                _logger.Debug("{@MethodName}, _waitingMyTrades myTrades.Count {@Count}," +
                    "Order{@Order}", nameof(RemoveWaitingMyTrades), myTrades.Count, order);
            }
        }

        public async Task<List<Order>> GetOrders()
        {
            _logger.Debug("{@MethodName}, GetOrders ", nameof(GetOrders));

            List<Order> orders = new List<Order>();

            await Task.Delay(1);

            return _keyOrders.Values.ToList();
        }

        #endregion

        #region Events =========================================================================

        public event IConnector.newOrderEvent? NewOrderEvent;

        public event IConnector.newMyTradeEvent? NewMyTradeEvent;

        public event eventMessage? EventMessage;

        #endregion
    }
}

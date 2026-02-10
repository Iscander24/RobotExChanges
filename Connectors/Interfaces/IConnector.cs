using ControllerExChanges.Entity;
using ControllerExChanges.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Interfaces
{
    public interface IConnector
    {
        #region Properties ======================================================================

        /// <summary>
        /// Тип коннектора
        /// </summary>
        ExchangeType ExchangeType { get; }

        /// <summary>
        /// Параметры подключения к бирже
        /// </summary>
        ConnectParaments ConnectParaments { get; }

        /// <summary>
        /// Статус подключения к бирже
        /// </summary>
        ConnectStatus ConnectStatus { get; }

        /// <summary>
        /// Время на бирже
        /// </summary>
        DateTime ConnectorTime { get; }

        /// <summary>
        /// Портфели с данного подключения
        /// </summary>
        IPortfoliosService PortfoliosService { get; }

        /// <summary>
        /// Обезличенные сделки с данного подключения
        /// </summary>
        ITradesService TradesService { get; }

        /// <summary>
        /// Все инструменты на этой бирже. Ключ - IsinId
        /// </summary>
        ISecuritiesService SecuritiesService { get;  }

        /// <summary>
        /// Ордера на этой бирже
        /// </summary>
        IOrdersService OrdersService { get; }

        /// <summary>
        /// Сервис работы со свечами и кластерами
        /// </summary>
        ICandleService CandleService { get; }

        /// <summary>
        /// Создавать кластера
        /// </summary>
        bool IsCreateClusters { get; }


        #endregion

        #region Methods =========================================================================

        /// <summary>
        /// Подключиться к бирже
        /// </summary>
        Task<ConnectStatus> Connect();

        /// <summary>
        /// Отключиться от биржы
        /// </summary>
        Task Disconnect();

        /// <summary>
        /// Очистить и известить подписчиков
        /// </summary>
        Task Dispose();

        /// <summary>
        /// Подписаться на инструмент
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        //Task<bool> SubscribeToSecurity(Security security);
        Task<bool> AddSecurityToSubscription(Security security);

        /// <summary>
        /// Подписаться на инструмент. Получаемые данные: Стакан котировок
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        Task<bool> AddSecurityToSubscriptionMarketDepth(Security security);

        /// <summary>
        /// Подписаться на инструмент. Получаемые данные: Свечи
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        Task<bool> AddSecurityToSubscriptionCandles(Security security, TimeFrame timeFrame = TimeFrame.Min1);

        /// <summary>
        /// Отписаться от инструмента
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        //Task<bool> UnsubscribeToSecurity(Security security);
        Task<bool> RemoveSecurityFromSubscription(Security security);

        /// <summary>
        /// Отправить ордер на биржу
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<bool> SendOrder(Order order);

        /// <summary>
        /// Отменить ордер
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<bool> CancelOrder(Order order);

        /// <summary>
        /// запросить портфели
        /// </summary>
        /// <returns></returns>
        //Task<bool> GetPortfolios();

        
        void SetCreateClusters(bool isCreateClusters);

        string GetComment(string portfolioNumber);


        List<TimeFrame> GetTimeFrames();

        #endregion

        #region Events =========================================================================

        delegate void connectStatusChangeEvent(ConnectStatus status);
        /// <summary>
        /// Событие изменения статуса подключения к бирже
        /// </summary>
        event connectStatusChangeEvent? ConnectStatusChangeEvent;

        delegate void securitiesChangeEvent(ConcurrentDictionary<string, Security> securities);
        /// <summary>
        /// Событие изменения по иснтрументам. Возвращает словарь с ключом по IsinId
        /// </summary>
        event securitiesChangeEvent? SecuritiesChangeEvent;

        delegate void newMarketDepthEvent(MarketDepth marketDepth);
        /// <summary>
        /// Событие, пришел новый стакан
        /// </summary>
        event newMarketDepthEvent? NewMarketDepthEvent;

        delegate void newTradeEvent(Trade trade);
        /// <summary>
        /// Событие, пришла новая обезличенная сделка
        /// </summary>
        event newTradeEvent? NewTradeEvent;

        /// <summary>
        /// Обезличенная сделка, без сохранения трейдов
        /// </summary>
        event newTradeEvent? LastTradeEvent;

        delegate void newOrderEvent(Order order);
        /// <summary>
        /// Событие, обновился ордер
        /// </summary>
        event newOrderEvent? NewOrderEvent;

        delegate void newMyTradeEvent(MyTrade myTrade);
        /// <summary>
        /// Событие, пришла моя сделка
        /// </summary>
        event newMyTradeEvent? NewMyTradeEvent;

        delegate void portfoliosChangeEvent(Dictionary<string, Portfolio> portfolios);
        /// <summary>
        /// Событие, изменилось состояние портфелей
        /// </summary>
        event portfoliosChangeEvent? PortfoliosChangeEvent;

        delegate void setParamentsEvent(ConnectParaments paraments);
        /// <summary>
        /// Событие установки новых параметров подключения коннектора.
        /// Вызывается при установлении новых параметров (предусмотреть в методе SetParaments())
        /// </summary>
        event setParamentsEvent? SetParamentsEvent;

        delegate void eventCancelCandle(Security security, TimeFrame timeFrame, List<Candle> candles);

        /// <summary>
        /// Событие, свеча сформировалась.
        /// </summary>
        event eventCancelCandle? EventCancelCandle;

        delegate void eventChangeCandle(Security security, TimeFrame timeFrame, List<Candle> candles);       

        /// <summary>
        /// Событие. Изменилась свеча
        /// </summary>
        event eventChangeCandle? EventChangeCandle;


        event Action? DisposeEvent;

        delegate void eventMessage(Message message);
        /// <summary>
        /// Сообщение от биржи
        /// </summary>
        event eventMessage? EventMessage;

        #endregion
    }
}

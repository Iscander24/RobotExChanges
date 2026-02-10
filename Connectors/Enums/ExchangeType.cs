using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Enums
{
    public enum ExchangeType
    {
        /// <summary>
        /// exchange type not defined
        /// Тип сервера не назначен
        /// </summary>
        None,

        /// <summary>
        /// подключение к LIve Investing акции
        /// </summary>
        LiveMoex,

        /// <summary>
        /// подключение к LIve Investing фьючерсы
        /// </summary>
        LiveFutures,

        /// <summary>
        /// подключение к LIve Investing валюта
        /// </summary>
        LiveCurrency,

        /// <summary>
        /// connection to Russian broker Tinkoff Invest
        /// подключение к Тинькофф Инвест (выдающих кредиты под 70% годовых)
        /// </summary>
        Tinkoff,

        /// <summary>
        /// cryptocurrency exchange Hitbtc
        /// биржа криптовалют Hitbtc
        /// </summary>
        Hitbtc,

        /// <summary>
        /// cryptocurrency exchange FTX
        /// биржа криптовалют FTX
        /// </summary>
        FTX,

        /// <summary>
        /// cryptocurrency exchange Gate.io
        /// биржа криптовалют Gate.io
        /// </summary>ghj
        GateIo,

        /// <summary>
        /// Futures of cryptocurrency exchange Gate.io
        /// Фьючерсы биржи криптовалют Gate.io
        /// </summary>
        GateIoFutures,

        /// <summary>
        /// cryptocurrency exchange ZB
        /// биржа криптовалют ZB
        /// </summary>
        Zb,

        /// <summary>
        /// Livecoin exchange
        /// биржа Livecoin
        /// </summary>
        Livecoin,

        /// <summary>
        /// BitMax exchange
        /// биржа BitMax
        /// </summary>
        BitMax,

        /// <summary>
        /// transaq
        /// транзак
        /// </summary>
        Transaq,

        /// <summary>
        /// LMax exchange
        /// биржа LMax
        /// </summary>
        Lmax,

        /// <summary>
        /// cryptocurrency exchange Bitfinex
        /// биржа криптовалют Bitfinex
        /// </summary>
        Bitfinex,

        /// <summary>
        /// cryptocurrency exchange Binance
        /// биржа криптовалют Binance
        /// </summary>
        Binance,

        /// <summary>
        /// cryptocurrency exchange Binance Futures
        /// биржа криптовалют Binance, секция фьючеры
        /// </summary>
        BinanceFutures,

        /// <summary>
        /// cryptocurrency exchange Exmo
        /// биржа криптовалют Exmo
        /// </summary>
        Exmo,

        /// <summary>
        /// terminal Ninja Trader
        /// нинзя трейдер
        /// </summary>
        NinjaTrader,

        /// <summary>
        /// cryptocurrency exchange Kraken
        /// биржа криптовалют Kraken
        /// </summary>
        Kraken,

        /// <summary>
        /// forex broker Oanda
        /// форекс брокер Oanda
        /// </summary>
        Oanda,

        /// <summary>
        /// cryptocurrency exchange BitMEX
        /// биржа криптовалют BitMEX
        /// </summary>
        BitMex,

        /// <summary>
        /// cryptocurrency exchange BitStamp
        /// биржа криптовалют BitStamp
        /// </summary>
        BitStamp,

        /// <summary>
        /// optimizer
        /// Оптимизатор
        /// </summary>
        Optimizer,

        /// <summary>
        /// miner
        /// Майнер
        /// </summary>
        Miner,

        /// <summary>
        /// connection to terminal Quik by LUA
        /// Квик луа
        /// </summary>
        QuikConnector,

        /// <summary>
        /// SmartCom
        /// Смарт-Ком
        /// </summary>
        SmartCom,

        /// <summary>
        /// Plaza 2
        /// Плаза 2
        /// </summary>
        Plaza,

        /// <summary>
        /// Tester
        /// Тестер
        /// </summary>
        Tester,

        /// <summary>
        /// IB
        /// </summary>
        InteractivBrokers,

        /// <summary>
        /// AstsBridge, he is also the gateway or TEAP
        /// AstsBridge, он же ШЛЮЗ, он же TEAP 
        /// </summary>
        AstsBridge,

        /// <summary>
        /// Дата сервер московской биржи
        /// </summary>
        MoexDataServer,

        /// <summary>
        /// MFD web exchange
        /// </summary>
        MfdWeb,

        /// <summary>
        /// Huobi Spot
        /// </summary>
        HuobiSpot,

        /// <summary>
        /// Huobi Futures
        /// </summary>
        HuobiFutures,

        /// <summary>
        /// Huobi Futures Swap
        /// </summary>
        HuobiFuturesSwap,

        /// <summary>
        /// Bybit exchange
        /// </summary>
        ByBit,

        /// <summary>
        /// Коннектор на генераторе случайных числах
        /// </summary>
        RandomConnector,

        Okx,

        CommExFutures,

        /// <summary>
        /// BingX exchange
        /// </summary>
        BingX
    }
}

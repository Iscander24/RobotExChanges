using ControllerExChanges.Entity;
using ControllerExChanges.Enums;
using ControllerExChanges.Interfaces;
using ScottPlot;
using ScottPlot.Plottable;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BaseRobot.ViewModels
{
    public class ChartWindowVM : BaseVM
    {
        public ChartWindowVM(ILogger logger, IConnector connector, Security security)
        {
            _logger = logger.ForContext<ChartWindowVM>();

            _connector = connector;

            _security = security;

            _dispatcher = Dispatcher.CurrentDispatcher;

            Init();
        }
        

        #region =========================== Fields =========================================

        private ILogger _logger;

        private IConnector _connector;

        private Security _security;

        private Dispatcher _dispatcher;

        private List<Candle> _candles = new List<Candle>();

        private FinancePlot? _financePlot;

        #endregion

        #region =========================== Properties =========================================

        public WpfPlot WpfPlot { get; set; } = new WpfPlot();

        #endregion

        #region =========================== Methods =========================================

        public void Unsubscribe()
        {
            _connector.EventChangeCandle -= _connector_EventChangeCandle;

            WpfPlot.AxesChanged -= WpfPlot_AxesChanged;

            WpfPlot.MouseDoubleClick -= WpfPlot_MouseDoubleClick;
        }
        
        
        private async void Init()
        {
            _connector.EventChangeCandle += _connector_EventChangeCandle;

            List <Candle> candles = await _connector.CandleService.GetCandles(_security, TimeFrame.Min30);

            _connector_EventChangeCandle(_security, TimeFrame.Min30, candles);

            WpfPlot.AxesChanged += WpfPlot_AxesChanged;
            WpfPlot.MouseDoubleClick += WpfPlot_MouseDoubleClick;

            WpfPlot.Plot.YAxis.Ticks(false);
            WpfPlot.Plot.YAxis.Grid(false);
            WpfPlot.Plot.YAxis.IsVisible = false;

            WpfPlot.Plot.RightAxis.Ticks(true);
            WpfPlot.Plot.RightAxis.Grid(true);

            WpfPlot.Plot.XAxis.DateTimeFormat(true);
        }

        private void _connector_EventChangeCandle(Security security, TimeFrame timeFrame, List<Candle> candles)
        {
            if (_security == null || security.IsinId != _security.IsinId) return;

            if (_dispatcher.CheckAccess() == false)
            {
                _dispatcher.Invoke(() =>
                {
                    _connector_EventChangeCandle(security, timeFrame, candles);
                });
                return;
            }


            TimeSpan timeSpan = new TimeSpan(0, (int)timeFrame, 0);

            WpfPlot.Plot.RenderLock();

            if (_financePlot == null)
            {
                lock (new object())
                {
                    OHLC[] oHLCs = new OHLC[candles.Count];

                    for (int i = 0; i < candles.Count; i++)
                    {
                        OHLC ohlc = new OHLC((double)candles[i].Open,
                                             (double)candles[i].High,
                                             (double)candles[i].Low,
                                             (double)candles[i].Close,
                                             candles[i].DateTime,
                                             timeSpan);
                        oHLCs[i] = ohlc;

                        _candles.Add(candles[i]);
                    }

                    _financePlot = WpfPlot.Plot.AddCandlesticks(oHLCs);

                    _financePlot.YAxisIndex = 1;

                    SetBoundaries();
                }
            }

            else
            {
                if (candles.Count > 0)
                {
                    Candle currentCandle = candles.Last();

                    Candle previousCandle = _candles.Last();

                    OHLC ohlc = new OHLC((double)currentCandle.Open,
                                             (double)currentCandle.High,
                                             (double)currentCandle.Low,
                                             (double)currentCandle.Close,
                                             currentCandle.DateTime,
                                             timeSpan);

                    if (previousCandle.DateTime == currentCandle.DateTime)
                    {
                        IOHLC lastOHLC = _financePlot.Last();

                        lastOHLC.Open = (double)currentCandle.Open;
                        lastOHLC.High = (double)currentCandle.High;
                        lastOHLC.Low = (double)currentCandle.Low;
                        lastOHLC.Close = (double)currentCandle.Close;

                    }
                    else if (previousCandle.DateTime < currentCandle.DateTime)
                    {
                        _financePlot.Add(ohlc);

                        _candles.Add(currentCandle);
                    }
                }
            }
            WpfPlot.Plot.RenderUnlock();
            WpfPlot.Refresh();
        }

        private void WpfPlot_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (_candles.Count == 0) return;

                (float x, float y) = WpfPlot.GetMousePixel();

                double yMax;
                double yMin;

                AxisLimits axisLimits = WpfPlot.Plot.GetAxisLimits(0, 1);

                DateTime start = DateTime.FromOADate(axisLimits.XMin);

                DateTime end = DateTime.FromOADate(axisLimits.XMax);

                if (x > WpfPlot.ActualWidth - 60)
                {
                    yMax = (double)_candles.Where(candle => candle.DateTime >= start && candle.DateTime <= end).Max(candle => candle.High);

                    yMin = (double)_candles.Where(candle => candle.DateTime >= start && candle.DateTime <= end).Min(candle => candle.Low);

                    double spread = (yMax - yMin) * 0.02;

                    WpfPlot.Plot.RenderLock();

                    WpfPlot.Plot.SetAxisLimitsY(yMin - spread, yMax + spread, 1);

                    WpfPlot.Plot.RenderUnlock();

                    WpfPlot.Refresh();
                }
                else if (y > WpfPlot.ActualHeight - 60)
                {
                    int countVisibleCandles = _candles.Where(candle => candle.DateTime >= start && candle.DateTime <= end).Count();

                    double xMax = _candles.Last().DateTime.ToOADate();

                    int index = _candles.Count - countVisibleCandles;

                    double xMin = _candles[index].DateTime.ToOADate();

                    double shift = 0.00138889;

                    WpfPlot.Plot.RenderLock();

                    WpfPlot.Plot.SetAxisLimitsX(xMin, xMax + shift, 0);

                    WpfPlot.Plot.RenderUnlock();

                    WpfPlot.Refresh();


                    //AxisLimits shiftedLimits = WpfPlot.Plot.GetAxisLimits(0, 1).WithPan(0.5, 0);

                    //WpfPlot.Plot.RenderLock();

                    //WpfPlot.Plot.SetAxisLimitsX(xMin, shiftedLimits.XMax, 0);
                }
            }

            catch (Exception ex)
            {
                _logger.Error("Method{@Method}, Exception{@Exception}", nameof(WpfPlot_MouseDoubleClick), ex);
            }
        }

        private void WpfPlot_AxesChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            e.Handled = false;

            (float x, float y) = WpfPlot.GetMousePixel();

            if (x > WpfPlot.ActualWidth - 60) WpfPlot.Configuration.LockHorizontalAxis = true;
            else WpfPlot.Configuration.LockHorizontalAxis = false;

            if (y > WpfPlot.ActualHeight - 40) WpfPlot.Configuration.LockVerticalAxis = true;
            else WpfPlot.Configuration.LockVerticalAxis = false;

        }
        
        /// <summary>
        /// метод устанавливающий границы просмотра на оси X (необходимо использовать только при заполненном List<Candle>_candles)
        /// </summary>
        private void SetBoundaries()
        {
            if (_candles.Count < 2) return;

            double boundXPeriods = 0.005;

            double xMin = _candles[0].DateTime.ToOADate();
            double xMax = _candles.Last().DateTime.ToOADate();

            WpfPlot.Plot.XAxis.SetBoundary(xMin - boundXPeriods, xMax + boundXPeriods);
        }

        #endregion
    }
}

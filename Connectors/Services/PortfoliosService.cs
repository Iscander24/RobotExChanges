using ControllerExChanges.Entity;
using ControllerExChanges.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ControllerExChanges.Interfaces.IConnector;

namespace ControllerExChanges.Services
{
    public class PortfoliosService : IPortfoliosService
    {
        public PortfoliosService(ControllerLogger logger)
        {
            _logger = logger.Logger.ForContext<PortfoliosService>();
        }


        #region Properties ================================================================================

        /// <summary>
        /// Словарь портфелей на бирже (Счетов с активами)
        /// </summary>
        public Dictionary<string, Portfolio> Portfolios => _portfolios;

        #endregion

        #region Fields ===================================================================================

        /// <summary>
        /// Словарь портфелей на бирже (Счетов с активами)
        /// </summary>
        protected Dictionary<string, Portfolio> _portfolios = new Dictionary<string, Portfolio>();

        ILogger _logger;        

        #endregion

        #region Methods ==================================================================================

        /// <summary>
        /// Установить новые портфели с биржи
        /// </summary>
        /// <param name="portfolios"></param>
        public void SetPortfolios(List<Portfolio> portfolios) // Refactored
        {
            if (portfolios.Count == 0) return;
            
            for (int i = 0; i < portfolios.Count; i++)
            {
                Portfolio? portfolio;

                if (_portfolios.TryGetValue(portfolios[i].Name, out portfolio))
                {
                    portfolio.UpDate(portfolios[i]);
                }
                else
                {
                    _portfolios.Add(portfolios[i].Name, portfolios[i]);
                }
            }

            PortfoliosChangeEvent?.Invoke(_portfolios);

            //_logger.Information("{@MethodName}, _portfolios.Count {Count}" , nameof(SetPortfolios), _portfolios.Count);
        }

        /// <summary>
        /// Получить портфель по имени (номеру счета)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Portfolio? GetPortfolioForName(string name) // Refactored
        {
            if (string.IsNullOrEmpty(name))
            {
                _logger.Warning("{@MethodName}, name == Empty ", nameof(GetPortfolioForName));
                return null;
            }

            if (_portfolios == null
                || _portfolios.Count == 0)
            {
                _logger.Warning("{@MethodName}, _portfolios.Count == 0 ", nameof(GetPortfolioForName));
                return null;
            }

            Portfolio? portfolio = null;

            _portfolios.TryGetValue(name, out portfolio);

            if (portfolio == null)
            {
                _logger.Warning("{@MethodName}, portfolio == null ", nameof(GetPortfolioForName));
                return null;
            }

            _logger.Verbose("{@MethodName}, Portfolio {@Portfolio} ", nameof(GetPortfolioForName), portfolio);

            return portfolio;
        }


        public List<Portfolio> GetPortfoliosList() // Refactored
        {
            List<Portfolio> portfolios = new List<Portfolio>();

            foreach (var portfolio in _portfolios)
            {
                if (portfolio.Value != null)
                {
                    portfolios.Add(portfolio.Value);
                }
            }

            _logger.Verbose("{@MethodName}, Portfolios.count = {Count}", nameof(GetPortfoliosList), portfolios.Count);

            return portfolios;
        }

        #endregion

        #region Events ===========================================================================

        public event IConnector.portfoliosChangeEvent? PortfoliosChangeEvent;

        #endregion
    }
}

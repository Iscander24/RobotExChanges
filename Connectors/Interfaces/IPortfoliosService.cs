using ControllerExChanges.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Interfaces
{
    public interface IPortfoliosService
    {


        /// <summary>
        /// Словарь портфелей на бирже (Счетов с активами)
        /// </summary>
        Dictionary<string, Portfolio> Portfolios { get; }

        /// <summary>
        /// Установить новые портфели с биржи
        /// </summary>
        /// <param name="portfolios"></param>
        void SetPortfolios(List<Portfolio> portfolios);


        /// <summary>
        /// Получить портфель по имени (номеру счета)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Portfolio? GetPortfolioForName(string name);

        /// <summary>
        /// Получить List портфелей
        /// </summary>
        /// <returns></returns>
        List<Portfolio> GetPortfoliosList();

        /// <summary>
        /// Событие, изменилось состояние портфелей
        /// </summary>
        public event IConnector.portfoliosChangeEvent? PortfoliosChangeEvent;
    }
}

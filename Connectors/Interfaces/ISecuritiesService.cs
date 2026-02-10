using ControllerExChanges.Entity;
using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Interfaces
{
    public interface ISecuritiesService
    {
        
        
        /// <summary>
        /// Словарь бумаг на бирже 
        /// </summary>
        ConcurrentDictionary<string, Security> Securities { get; }

        /// <summary>
        /// Словарь для сопоставления IsinId по Name security. Key = Name security
        /// </summary>
        ConcurrentDictionary<string, string> SecNameKeys { get; }

        /// <summary>
        /// Словарь для сопоставления IsinId по Name и Class security. Key = Name + ClassCode security
        /// </summary>
        ConcurrentDictionary<string, string> SecNameAndClassKeys { get; }


        /// <summary>
        /// Установить новые портфели с биржи
        /// </summary>
        /// <param name="securities"></param>
        void SetSecurities(List<Security> securities);

        /// <summary>
        /// Установить новую бумагу с биржи
        /// </summary>
        /// <param name="security"></param>
        void SetSecurity(Security security);

        /// <summary>
        /// Получить бумагу по isinId 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Security? GetSecurityForIsinId(string isinId);

        /// <summary>
        /// Получить бумагу по имени
        /// </summary>
        /// <param name="secName"></param>
        /// <returns></returns>
        Security? GetSecurityFromSecNameAndClass(string secName, string secCode = "");

        /// <summary>
        /// Получить List бумаг
        /// </summary>
        /// <returns></returns>
        List<Security> GetSecuritiesList();

        /// <summary>
        /// Событие изменения по иснтрументам. Возвращает словарь с ключом по IsinId
        /// </summary>
        public event IConnector.securitiesChangeEvent? SecuritiesChangeEvent;
    }
}

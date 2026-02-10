using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    /// <summary>
    /// Портфель ценных бумаг. (Счёт на бирже с активами)
    /// </summary>
    public class Portfolio
    {
        public Portfolio()
        {

        }

        /// <summary>
        /// Номер счёта
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Список доступных валют
        /// </summary>
        public List<Deposit> Deposits { get; set; } = new List<Deposit>();

        public void UpDate(Portfolio newData)
        {
            foreach (var deposit in newData.Deposits)
            {
                Deposit? depo = Deposits.Find(dep => dep.Asset == deposit.Asset);

                if (depo != null)
                {
                    depo.UpDate(deposit);
                }
                else
                {
                    Deposits.Add(deposit);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class Deposit
    {
        public Deposit()
        {

        }

        /// <summary>
        /// Актив
        /// </summary>
        public string Asset { get; set; } = string.Empty;

        /// <summary>
        /// Депозит на начало периода
        /// </summary>
        public decimal Many { get; set; }

        /// <summary>
        /// Текущий депозит
        /// </summary>
        public decimal CurrentMany { get; set; }

        /// <summary>
        /// Заблокированные деньги
        /// </summary>
        public decimal BlockedMoney { get; set; }

        /// <summary>
        /// session profit
        /// Вариационная маржа
        /// </summary>
        public decimal Profit { get; set; }

        public void UpDate(Deposit newData)
        {
            Many = newData.Many;
            CurrentMany = newData.CurrentMany;
            BlockedMoney = newData.BlockedMoney;
            Profit = newData.Profit;
        }
    }
}

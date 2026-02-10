using ControllerExChanges.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseRobot.ViewModels
{
    public class SecurityVM
    {
        public SecurityVM(Security security)
        {
            _security = security;
        }

        Security _security;

        public string Name
        {
            get => _security.Name;
        }

        public DateTime Expiration
        {
            get => _security.ExpirationDate;
        }

        public decimal Lot
        {
            get => _security.Lot;
        }

        public decimal Go
        {
            get => _security.SellersWarranty;
        }

        public string FullName
        {
            get => _security.FullName;
        }

        public Security GetSecurity()
        {
            return _security;
        }
    }
}

using ControllerExChanges.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Interfaces
{
    public interface ICandle
    {
        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Open { get; set; }

        public decimal Close { get; set; }

        public decimal Volume { get; set; }

        public DateTime DateTime { get; set; }

        public TimeFrame TimeFrame { get; set; }

        public string IsinId { get; set; }

        public string NameSecurity { get; set; }
    }
}

using ControllerExChanges.Entity;
using ControllerExChanges.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseRobot.RobotEntity
{
    public class ConfigRobot
    {
        public ConfigRobot() { }

        public string Header { get; set; }

        public ExchangeType ExchangeType{ get; set; }

        public string SecurityName { get; set; }

        public string SecurityClass { get; set; }

        public string SecurityIsinId { get; set; }

        public string PortfolioNumber { get; set; }

        public List<Order> Orders { get; set; }

        public List<MyTrade> MyTrades { get; set; }
    }
}

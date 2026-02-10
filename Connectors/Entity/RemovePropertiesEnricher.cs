using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class RemovePropertiesEnricher: ILogEventEnricher
    {
        public void Enrich(LogEvent le, ILogEventPropertyFactory lepf)
        {
            le.RemovePropertyIfPresent("OK-ACCESS-PASSPHRASE");
            le.RemovePropertyIfPresent("OK-ACCESS-SIGN");
        }
    }
}

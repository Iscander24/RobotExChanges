
using Serilog;
using ControllerExChanges.Controller;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class ControllerLogger
    {
        public ILogger Logger { get; } = Controller.Controller.LogGlobal;
    }
}

using Microsoft.Extensions.DependencyInjection;
using BaseRobot.ViewModels;
using BaseRobot.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseRobot.ServicesRobot
{
    public class RobotFactory
    {
        //public static ServiceProvider ServiceProvider;

        public static Robot CreateRobot()
        {
            return Program.MyHost.Services.GetRequiredService<Robot>();
        }
    }
}

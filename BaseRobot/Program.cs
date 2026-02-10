using BaseRobot.ViewModels;
using BaseRobot.Views;
using ControllerExChanges.Controller;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;
using System.IO;


namespace BaseRobot
{    
    public class Program
    {
        static Controller _controller;

        public static IHost MyHost;
        
        
        [STAThread]
        public static void Main()
        {
            ILogger logger = BuildLogger();

            _controller = Controller.GetController(logger);
            
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder();

            hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<App>();
                services.AddSingleton<ILogger>(serviceProvider => logger);                
                services.AddSingleton<MyBot>();
                services.AddSingleton<MyBotVM>(); 
                services.AddTransient<Robot>();
                services.AddSingleton<ConnectionsVM>();

                if (_controller != null) services.AddSingleton<Controller>(serviceProvider => _controller);

                // if (_controller != null) services.AddSingleton<Controller>(_controller);
            });

            IHost host = hostBuilder.Build();

            MyHost = host;  //  для RobotFactory

            App? app = host.Services.GetService<App>();  // вызывать чере host. или через MyHost? В чем разница

            try
            {
                logger.Information("Method {@Method}, app?.Run()", nameof(Main));

                app?.Run();
            }
            catch(Exception ex)
            {
                logger.Error("Method {@Method}, Exception {@Exception}", nameof(Main), ex);
            }
            
        }

        private static ILogger BuildLogger()
        {
            if (!Directory.Exists(@"MyLog"))
            {
                Directory.CreateDirectory(@"MyLog");
            }

            ILogger logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.File(new CompactJsonFormatter(), @"MyLog\" + DateTime.Now.ToShortDateString() + "_bot.log",
                                                                        rollingInterval: RollingInterval.Day,
                                                                        encoding: System.Text.Encoding.UTF8)
                            .CreateLogger();

            return logger;
        }

    }
}

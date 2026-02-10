using MahApps.Metro.Controls;
using Serilog;


namespace BaseRobot.Views
{
    /// <summary>
    /// Логика взаимодействия для MyBot.xaml
    /// </summary>
    public partial class MyBot : MetroWindow
    {
        public MyBot(ILogger logger)
        {
            InitializeComponent();

            //_vm = new MyBotVM(this);

            //DataContext = _vm;
            //DataContext = new MyBotVM(this, logger);
        }

        //private MyBotVM _vm;
    }
}

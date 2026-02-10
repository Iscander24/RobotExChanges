using BaseRobot.ViewModels;
using BaseRobot.Views;
using System.Windows;


namespace BaseRobot
{
    public class App : Application
    {
        public App(MyBot myBot, MyBotVM myBotVM)
        {
            _mainWindow = myBot;

            _mainWindow.DataContext = myBotVM;

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
        }

        private void CurrentDomain_FirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private MyBot _mainWindow;


        protected override void OnStartup(StartupEventArgs e)
        {
            _mainWindow.Show();

            base.OnStartup(e);

            //Resources.MergedDictionaries.Add(new ResourceDictionary()
            //{
            //    Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml", UriKind.Absolute)
            //});

            //Resources.MergedDictionaries.Add(new ResourceDictionary()
            //{
            //    Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml", UriKind.Absolute)
            //});

            //Resources.MergedDictionaries.Add(new ResourceDictionary()
            //{
            //    Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Themes/Dark.Blue.xaml", UriKind.Absolute)
            //});

        }
    }
}

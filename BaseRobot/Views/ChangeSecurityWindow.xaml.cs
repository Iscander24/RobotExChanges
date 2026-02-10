using BaseRobot.ViewModels;
using ControllerExChanges.Controller;
using MahApps.Metro.Controls;

namespace BaseRobot.Views
{
    /// <summary>
    /// Логика взаимодействия для ChangeSecurityWindow.xaml
    /// </summary>
    public partial class ChangeSecurityWindow : MetroWindow
    {
        public ChangeSecurityWindow(Robot robot, Controller controller)
        {
            InitializeComponent();

            DataContext = new ChangeSecurityVM(robot, controller);
        }
    }
}

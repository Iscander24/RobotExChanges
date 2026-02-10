using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BaseRobot.ViewModels
{
    public class BaseVM : INotifyPropertyChanged
    {
        #region ==================================== Methods =============================================

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region ==================================== Events =============================================

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}


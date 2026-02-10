using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BaseRobot.Commands
{
    public class DelegateCommand : ICommand
    {
        public DelegateCommand(DelegateFunction function)
        {
            _function = function;
        }

        public DelegateCommand(DelegateFunctionNoParam functionNoParam)
        {
            _functionNoParam = functionNoParam;
        }

        public delegate void DelegateFunction(object obj);

        private DelegateFunction _function;

        public delegate void DelegateFunctionNoParam();

        private DelegateFunctionNoParam _functionNoParam;


        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (_functionNoParam != null) _functionNoParam();

            else _function?.Invoke(parameter);

        }
    }
}

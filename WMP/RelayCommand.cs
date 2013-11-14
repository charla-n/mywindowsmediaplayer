using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WMP
{
    public class RelayCommand : ICommand
    {
        Action<object> _execute;
        Action _executeNoObj;
        Func<bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<bool> canExecute)
        {
            if (execute != null)
            {
                _executeNoObj = null;
                _execute = execute;
                _canExecute = canExecute;
            }
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute != null)
            {
                _execute = null;
                _executeNoObj = execute;
                _canExecute = canExecute;
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;
            else
                return _canExecute();
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        void ICommand.Execute(object parameter)
        {
            if (_executeNoObj != null)
                _executeNoObj();
            else
                _execute(parameter);
        }
    }
}

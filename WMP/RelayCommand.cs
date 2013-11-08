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
        Action _execute;
        Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute != null)
            {
                _execute = execute;
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
            _execute();
        }
    }
}

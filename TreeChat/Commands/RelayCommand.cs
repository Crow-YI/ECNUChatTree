using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TreeChat.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecte;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecte = null)
        {
            _execute = execute;
            _canExecte = canExecte;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _canExecte == null || _canExecte(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}

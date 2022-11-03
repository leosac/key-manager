using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyManagerCommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool> _canExecute;

        public KeyManagerCommand(Action<object?> execute)
            : this(execute, null)
        { }

        public KeyManagerCommand(Action<object?> execute, Func<object?, bool>? canExecute)
        {
            if (execute is null) throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute ?? (x => true);
        }

        public bool CanExecute(object? parameter) => _canExecute(parameter);

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Refresh() => CommandManager.InvalidateRequerySuggested();
    }
}

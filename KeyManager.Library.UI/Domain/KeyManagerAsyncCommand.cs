using System.Windows.Input;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyManagerAsyncCommand<T> : IKeyManagerAsyncCommand<T>
    {
        public event EventHandler? CanExecuteChanged;

        private bool _isExecuting;
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool> _canExecute;
        private readonly IKeyManagerErrorHandler _errorHandler;

        public KeyManagerAsyncCommand(Func<T, Task> execute, Func<T, bool> canExecute = null, IKeyManagerErrorHandler errorHandler = null)
        {
            _execute = execute;
            _canExecute = canExecute;
            _errorHandler = errorHandler;
        }

        public bool CanExecute(T parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
        }

        public async Task ExecuteAsync(T parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    await _execute(parameter);
                }
                finally
                {
                    _isExecuting = false;
                }
            }

            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Explicit implementations
        bool ICommand.CanExecute(object? parameter)
        {
            return CanExecute((T)parameter);
        }

        void ICommand.Execute(object? parameter)
        {
            try
            {
                ExecuteAsync((T)parameter);
            }
            catch (Exception ex)
            {
                _errorHandler?.HandleError(ex);
            }
        }
        #endregion
    }
}

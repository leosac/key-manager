using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public interface IKeyManagerAsyncCommand<T> : ICommand
    {
        Task ExecuteAsync(T parameter);
        bool CanExecute(T parameter);
    }
}

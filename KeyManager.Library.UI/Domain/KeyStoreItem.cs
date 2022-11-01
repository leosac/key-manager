using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyStoreItem : ViewModelBase
    {
        private readonly KeyStoreFactory _factory;

        private KeyStorePropertiesControlViewModel? _dataContext;

        private object? _content;

        public KeyStoreItem(KeyStoreFactory factory, KeyStorePropertiesControlViewModel? dataContext = null)
        {
            _factory = factory;
            _dataContext = dataContext ?? factory.CreateKeyStorePropertiesControlViewModel();
        }

        public string Name { get => _factory.Name; }

        public KeyStoreFactory Factory { get => _factory; }

        public object? Content => _content ??= CreateContent();

        public KeyStorePropertiesControlViewModel? DataContext
        {
            get => _dataContext;
        }

        private object? CreateContent()
        {
            var content = _factory.CreateKeyStorePropertiesControl();
            if (_dataContext != null && content is FrameworkElement element)
            {
                element.DataContext = _dataContext;
            }
            return content;
        }
    }
}

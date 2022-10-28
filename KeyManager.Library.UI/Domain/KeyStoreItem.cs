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

        private object? _content;

        public KeyStoreItem(KeyStoreFactory factory)
        {
            _factory = factory;
        }

        public string Name { get => _factory.Name; }

        public object? Content => _content ??= CreateContent();

        private object? CreateContent()
        {
            var content = _factory.CreateKeyStorePropertiesControl();
            return content;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyEntryItem : ViewModelBase
    {
        private readonly KeyEntryFactory _factory;

        private KeyEntryPropertiesControlViewModel? _dataContext;

        private object? _content;

        public KeyEntryItem(KeyEntryFactory factory, KeyEntryPropertiesControlViewModel? dataContext = null)
        {
            _factory = factory;
            _dataContext = dataContext ?? factory.CreateKeyEntryPropertiesControlViewModel();
        }

        public KeyEntryFactory Factory
        {
            get => _factory;
        }

        public string Name { get => _factory.Name; }

        public KeyEntryPropertiesControlViewModel? DataContext { get => _dataContext; }

        public object? Content => _content ??= CreateContent();

        private object? CreateContent()
        {
            var content = _factory.CreateKeyEntryPropertiesControl();
            if (_dataContext != null && content is FrameworkElement element)
            {
                element.DataContext = _dataContext;
            }
            return content;
        }
    }
}

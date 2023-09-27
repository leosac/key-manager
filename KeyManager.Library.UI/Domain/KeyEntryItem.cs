using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyEntryItem : ObservableValidator
    {
        private readonly KeyEntryUIFactory _factory;

        private KeyEntryPropertiesControlViewModel? _dataContext;

        private object? _content;

        public KeyEntryItem(KeyEntryUIFactory factory, KeyEntryPropertiesControlViewModel? dataContext = null)
        {
            _factory = factory;
            _dataContext = dataContext ?? factory.CreateKeyEntryPropertiesControlViewModel();
        }

        public KeyEntryUIFactory Factory
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

using System;
using System.Windows.Controls;
using System.Windows;
using Leosac.KeyManager.Library.Plugin.Domain;

namespace Leosac.KeyManager.Domain
{
    public class NavItem : ViewModelBase
    {
        private readonly Type _contentType;
        private object? _dataContext;

        private object? _content;
        private ScrollBarVisibility _horizontalScrollBarVisibilityRequirement = ScrollBarVisibility.Auto;
        private ScrollBarVisibility _verticalScrollBarVisibilityRequirement = ScrollBarVisibility.Auto;
        private Thickness _marginRequirement = new(16);

        public NavItem(string name, Type contentType, string icon, object? dataContext = null)
        {
            Name = name;
            _contentType = contentType;
            Icon = icon;
            _dataContext = dataContext;
        }

        public string Name { get; }

        public string Icon { get; }

        public object? Content => _content ??= CreateContent();

        public object? DataContext
        {
            get => _dataContext;
        }

        public ScrollBarVisibility HorizontalScrollBarVisibilityRequirement
        {
            get => _horizontalScrollBarVisibilityRequirement;
            set => SetProperty(ref _horizontalScrollBarVisibilityRequirement, value);
        }

        public ScrollBarVisibility VerticalScrollBarVisibilityRequirement
        {
            get => _verticalScrollBarVisibilityRequirement;
            set => SetProperty(ref _verticalScrollBarVisibilityRequirement, value);
        }

        public Thickness MarginRequirement
        {
            get => _marginRequirement;
            set => SetProperty(ref _marginRequirement, value);
        }

        private object? CreateContent()
        {
            var content = Activator.CreateInstance(_contentType);
            if (_dataContext != null && content is FrameworkElement element)
            {
                element.DataContext = _dataContext;
            }

            return content;
        }
    }
}

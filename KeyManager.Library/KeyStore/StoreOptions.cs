using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class StoreOptions : ObservableObject
    {
        public StoreOptions()
        {
            _wrappingKey = new WrappingKey();
            _resolveKeyLinks = true;
            _resolveVariables = true;
        }

        private WrappingKey _wrappingKey;
        public WrappingKey WrappingKey
        {
            get => _wrappingKey;
            set => SetProperty(ref _wrappingKey, value);
        }

        private bool _generateKeys;
        public bool GenerateKeys
        {
            get => _generateKeys;
            set => SetProperty(ref _generateKeys, value);
        }

        private bool _resolveKeyLinks;
        public bool ResolveKeyLinks
        {
            get => _resolveKeyLinks;
            set => SetProperty(ref _resolveKeyLinks, value);
        }

        private bool _resolveVariables;
        public bool ResolveVariables
        {
            get => _resolveVariables;
            set => SetProperty(ref _resolveVariables, value);
        }

        private bool _dryRun;
        public bool DryRun
        {
            get => _dryRun;
            set => SetProperty(ref _dryRun, value);
        }

        private string? _publishVariable;
        public string? PublishVariable
        {
            get => _publishVariable;
            set => SetProperty(ref _publishVariable, value);
        }
    }
}

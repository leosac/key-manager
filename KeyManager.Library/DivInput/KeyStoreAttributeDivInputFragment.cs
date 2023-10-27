namespace Leosac.KeyManager.Library.DivInput
{
    public class KeyStoreAttributeDivInputFragment : DivInputFragment
    {
        public KeyStoreAttributeDivInputFragment()
        {
            _attribute = string.Empty;
        }

        public override string Name => "Key Store Attribute";

        private string _attribute;
        public string Attribute
        {
            get => _attribute;
            set => SetProperty(ref _attribute, value);
        }

        public override object Clone()
        {
            return new KeyStoreAttributeDivInputFragment
            {
                Attribute = Attribute
            };
        }

        public override string GetFragment(DivInputContext context)
        {
            if (context.KeyStore != null && context.KeyStore.Attributes.ContainsKey(Attribute))
            {
                return context.KeyStore.Attributes[Attribute];
            }

            return string.Empty;
        }
    }
}

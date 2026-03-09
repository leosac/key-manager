namespace Leosac.KeyManager.Library.KeyStore.KeePass.UI.Domain
{
    public sealed class ProfileItem
    {
        public string Name { get; init; } = "";
        public int EntryCount { get; set; }
        public bool IsNonExhaustive { get; init; }
        public bool IsRoot { get; init; }
        public string DisplayName
        {
            get
            {
                if (Name == KeePassKeyStorePropertiesControlViewModel.DefaultProfileName)
                    return Name;
                var root = IsRoot ? Properties.Resources.DefaultProfile : string.Empty;
                return $"{Name}{root} ({EntryCount})";
            }
        }
    }
}

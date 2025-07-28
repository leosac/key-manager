using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyLinkDialogViewModel : LinkDialogViewModel
    {
        public KeyLinkDialogViewModel()
        {
            ImportResultCommand = new RelayCommand(
                () =>
                {
                    MaterialDesignThemes.Wpf.DrawerHost.CloseDrawerCommand.Execute(Dock.Bottom, null);
                    MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(true, null);
                });
        }

        public RelayCommand ImportResultCommand { get; }

        public KeyLink? KeyLink
        {
            get => Link as KeyLink;
        }

        public override async Task RunLinkImpl(KeyStore.KeyStore ks)
        {
            if (KeyLink != null)
            {
                var divContext = new DivInput.DivInputContext
                {
                    AdditionalKeyStoreAttributes = KeyStoreAttributes
                };
                LinkResult = await ks.ResolveKeyLink(KeyLink.KeyIdentifier, Class, KeyLink.ContainerSelector, KeyStore.KeyStore.ComputeDivInput(divContext, KeyLink.DivInput));
            }
        }
    }
}

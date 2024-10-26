using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
using log4net;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;

namespace Leosac.KeyManager.Library.KeyStore.File.UI.Domain
{
    public class FileKeyStoreToolsControlViewModel : KeyStoreAdditionalControlViewModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public FileKeyStoreToolsControlViewModel()
        {
            ImportCommand = new RelayCommand(Import);
            ExportCommand = new RelayCommand(Export);
            ResetEncryptionKeyCommand = new AsyncRelayCommand(ResetEncryptionKey);
            _newKeyStore = new FileKeyStore();
        }

        public override KeyStore? KeyStore
        {
            get => base.KeyStore;
            set
            {
                base.KeyStore = value;
                NewKeyStore.Properties = value?.Properties?.Clone() as KeyStoreProperties;
                if (NewKeyStore.Properties != null)
                {
                    NewKeyStore.Properties.StoreSecret = false;
                    NewKeyStore.Properties.Secret = string.Empty;
                }
            }
        }

        protected KeyStore _newKeyStore;
        public KeyStore NewKeyStore
        {
            get => _newKeyStore;
            set => SetProperty(ref _newKeyStore, value);
        }

        public RelayCommand ImportCommand { get; }

        public RelayCommand ExportCommand { get; }

        public AsyncRelayCommand ResetEncryptionKeyCommand { get; }

        private void Import()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "File Key Store Archive (*.zip)|*.zip";
            ofd.CheckFileExists = true;
            if (KeyStore is FileKeyStore ks && ofd.ShowDialog() == true)
            {
                FileKeyStoreArchive.Import(ofd.FileName, ks);
            }
        }

        private void Export()
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "File Key Store Archive (*.zip)|*.zip";
            if (KeyStore is FileKeyStore ks && sfd.ShowDialog() == true)
            {
                try
                {
                    FileKeyStoreArchive.Export(sfd.FileName, ks);
                }
                catch (Exception ex)
                {
                    log.Error("Export of the File Key Store as an archive failed.", ex);
                    SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
                }
            }
        }

        private async Task ResetEncryptionKey()
        {
            if (KeyStore != null)
            {
                KeyStore.Options = new StoreOptions()
                {
                    ResolveKeyLinks = false,
                    ResolveVariables = false
                };
                await KeyStore.Publish(NewKeyStore, null, null, null);
                KeyStore.Properties = NewKeyStore.Properties;

                DialogHost.CloseDialogCommand.Execute(null, null);
            }
        }
    }
}

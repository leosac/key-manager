using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
using log4net;
using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;

namespace Leosac.KeyManager.Library.KeyStore.File.UI.Domain
{
    public class FileKeyStoreImportExportControlViewModel : KeyStoreAdditionalControlViewModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public FileKeyStoreImportExportControlViewModel()
        {
            ImportCommand = new RelayCommand(Import);

            ExportCommand = new RelayCommand(Export);
        }

        public RelayCommand ImportCommand { get; }

        public RelayCommand ExportCommand { get; }

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
    }
}

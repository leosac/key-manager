using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.KeyStore.NXP_SAM.ISLOG;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using Microsoft.Win32;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.ISLOG.Domain
{
    public class ISLOGKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public ISLOGKeyStorePropertiesControlViewModel()
        {
            _properties = new ISLOGKeyStoreProperties();
            BrowseCommand = new RelayCommand(() =>
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "Xml files (*.xml)|*.xml";
                ofd.FileName = ISLOGProperties!.TemplateFile;
                ofd.CheckPathExists = true;
                if (ofd.ShowDialog() == true)
                {
                    ISLOGProperties.TemplateFile = ofd.FileName;
                }
            });
        }

        public ISLOGKeyStoreProperties? ISLOGProperties
        {
            get { return Properties as ISLOGKeyStoreProperties; }
        }

        public RelayCommand BrowseCommand { get; }
    }
}

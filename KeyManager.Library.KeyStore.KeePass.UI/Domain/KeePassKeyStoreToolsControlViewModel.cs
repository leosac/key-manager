using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.KeePass.UI.Domain
{
    public class KeePassKeyStoreToolsControlViewModel : KeyStoreAdditionalControlViewModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public KeePassKeyStoreToolsControlViewModel()
        {
        }
    }
}

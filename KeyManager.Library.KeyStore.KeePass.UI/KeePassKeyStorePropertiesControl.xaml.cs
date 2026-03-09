using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using Leosac.KeyManager.Library.KeyStore.KeePass.UI.Domain;
using log4net;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace KeyManager.Library.KeyStore.KeePass.UI
{
    public partial class KeePassKeyStorePropertiesControl : UserControl
    {
        public KeePassKeyStorePropertiesControl()
        {
            InitializeComponent();
        }
    }
}
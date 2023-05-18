using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    public class DialogHelper
    {
        public static async Task<object?> ForceShow(UserControl dialog, string dialogIdentifier)
        {
            if (DialogHost.IsDialogOpen(dialogIdentifier))
            {
                DialogHost.Close(dialogIdentifier);
            }
            return await DialogHost.Show(dialog, dialogIdentifier);
        }
    }
}

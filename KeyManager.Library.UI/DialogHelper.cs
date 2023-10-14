using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    public static class DialogHelper
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

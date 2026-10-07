using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.UI;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;

namespace Leosac.KeyManager.Domain
{
    public class HomeControlViewModel : ObservableValidator
    {
        public HomeControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
        }

        public bool UserRolesEnabled => UserRoleContext.IsEnabled;

        public string UserName => UserRoleContext.UserName;

        public string UserRole => UserRoleContext.Role.ToString();

        public AsyncRelayCommand<object>? KeyStoreCommand { get; set; }

        public RelayCommand? FavoritesCommand { get; set; }
    }
}

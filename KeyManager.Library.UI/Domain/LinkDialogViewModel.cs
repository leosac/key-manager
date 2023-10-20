using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.KeyStore;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public abstract class LinkDialogViewModel : ObservableValidator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        protected LinkDialogViewModel()
        {
            RunLinkCommand = new AsyncRelayCommand(
                () =>
                {
                    MaterialDesignThemes.Wpf.DrawerHost.OpenDrawerCommand.Execute(null, null);
                    AllowImport = false;
                    return RunLink();
                });

            RunLinkForImportCommand = new AsyncRelayCommand(
                () =>
                {
                    MaterialDesignThemes.Wpf.DrawerHost.OpenDrawerCommand.Execute(null, null);
                    AllowImport = true;
                    return RunLink();
                });

            _class = KeyEntryClass.Symmetric;
        }

        private Link? _link;
        public Link? Link

        {
            get => _link;
            set => SetProperty(ref _link, value);
        }

        private KeyEntryClass _class;
        public KeyEntryClass Class

        {
            get => _class;
            set => SetProperty(ref _class, value);
        }

        private string? _linkResult;
        public string? LinkResult
        {
            get => _linkResult;
            set => SetProperty(ref _linkResult, value);
        }

        private string? _linkError;
        public string? LinkError
        {
            get => _linkError;
            set => SetProperty(ref _linkError, value);
        }

        private string? _divInputResult;
        public string? DivInputResult
        {
            get => _divInputResult;
            set => SetProperty(ref _divInputResult, value);
        }

        private bool _allowImport;

        public bool AllowImport
        {
            get => _allowImport;
            set => SetProperty(ref _allowImport, value);
        }

        public AsyncRelayCommand RunLinkCommand { get; }

        public AsyncRelayCommand RunLinkForImportCommand { get; }

        public abstract Task RunLinkImpl(KeyStore.KeyStore ks);

        public async Task RunLink()
        {
            log.Info(string.Format("Running the link manually..."));
            LinkError = null;
            if (Link != null && !string.IsNullOrEmpty(Link.KeyStoreFavorite) && Link.KeyIdentifier.IsConfigured())
            {
                var fav = Favorites.GetSingletonInstance().KeyStores.Where(f => f.Name.ToLower() == Link.KeyStoreFavorite.ToLower()).FirstOrDefault();
                if (fav != null)
                {
                    var ks = fav.CreateKeyStore();
                    if (ks != null)
                    {
                        try
                        {
                            await ks.Open();
                            await RunLinkImpl(ks);
                            await ks.Close();

                            log.Info(string.Format("Link execution completed."));
                        }
                        catch (KeyStoreException ex)
                        {
                            LinkError = ex.Message;
                        }
                        catch (Exception ex)
                        {
                            log.Error(string.Format("Unexpected error when resolving the link: {0}", ex.Message));
                            LinkError = string.Format("Unexpected error: {0}", ex.Message);
                        }
                    }
                    else
                    {
                        log.Error(string.Format("Cannot create the key store from Favorite `{0}`.", Link.KeyStoreFavorite));
                        LinkError = "Cannot create the key store from Favorite.";
                    }
                }
                else
                {
                    log.Error(string.Format("Cannot found the linked key store `{0}`.", Link.KeyStoreFavorite));
                    LinkError = "Cannot found the linked key store.";
                }
            }
            else
            {
                log.Error("The Link is not correctly setup.");
                LinkError = "The Link is not correctly setup.";
            }
        }
    }
}

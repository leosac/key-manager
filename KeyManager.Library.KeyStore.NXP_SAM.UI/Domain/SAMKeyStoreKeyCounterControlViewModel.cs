using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain
{
    public class SAMKeyStoreKeyCounterControlViewModel : KeyStoreAdditionalControlViewModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public SAMKeyStoreKeyCounterControlViewModel()
        {
            _selectedCounterIdentifier = 0;
            CounterIdentifiers = new ObservableCollection<byte>();
            for (byte i = 0; i < 16; ++i)
            {
                CounterIdentifiers.Add(i);
            }
            EditKeyUsageCounterCommand = new AsyncRelayCommand<byte?>(
                async identifier =>
                {
                    if (identifier != null)
                    {
                        try
                        {
                            var model = new SAMKeyUsageCounterDialogViewModel()
                            {
                                Counter = (KeyStore as SAMKeyStore)!.GetCounter(identifier.Value)
                            };

                            var dialog = new SAMKeyUsageCounterDialog()
                            {
                                DataContext = model
                            };

                            await UpdateKeyCounter(dialog);
                        }
                        catch (KeyStoreException ex)
                        {
                            SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex, "Key Store Error");
                        }
                    }
                });
        }

        private byte _selectedCounterIdentifier;
        
        public byte SelectedCounterIdentifier
        {
            get => _selectedCounterIdentifier;
            set => SetProperty(ref _selectedCounterIdentifier, value);
        }

        public ObservableCollection<byte> CounterIdentifiers { get; set; }

        public AsyncRelayCommand<byte?> EditKeyUsageCounterCommand { get; set; }

        private async Task UpdateKeyCounter(SAMKeyUsageCounterDialog dialog)
        {
            object? ret = await DialogHost.Show(dialog, "KeyCounterDialog");
            try
            {
                if (ret != null && dialog.DataContext is SAMKeyUsageCounterDialogViewModel model)
                {
                    if (model.Counter != null)
                    {
                        (KeyStore as SAMKeyStore)?.UpdateCounter(model.Counter);
                    }
                }
            }
            catch (KeyStoreException ex)
            {
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex, "Key Store Error");
                await UpdateKeyCounter(dialog);
            }
            catch (Exception ex)
            {
                log.Error("Updating the Key Usage Counter failed unexpected.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
                await UpdateKeyCounter(dialog);
            }
        }
    }
}

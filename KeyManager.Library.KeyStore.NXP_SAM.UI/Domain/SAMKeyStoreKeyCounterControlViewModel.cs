﻿using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using log4net;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            EditKeyUsageCounterCommand = new KeyManagerAsyncCommand<byte?>(async
                identifier =>
                {
                    if (identifier != null)
                    {
                        var model = new SAMKeyUsageCounterDialogViewModel()
                        {
                            Counter = (KeyStore as SAMKeyStore)?.GetCounter(identifier.Value)
                        };

                        var dialog = new SAMKeyUsageCounterDialog()
                        {
                            DataContext = model
                        };

                        UpdateKeyCounter(dialog);
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

        public KeyManagerAsyncCommand<byte?> EditKeyUsageCounterCommand { get; set; }

        private async void UpdateKeyCounter(SAMKeyUsageCounterDialog dialog)
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
                if (SnackbarMessageQueue != null)
                    SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex, "Key Store Error");
                UpdateKeyCounter(dialog);
            }
            catch (Exception ex)
            {
                log.Error("Updating the Key Usage Counter failed unexpected.", ex);
                if (SnackbarMessageQueue != null)
                    SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
                UpdateKeyCounter(dialog);
            }
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Domain
{
    public class KeyControlViewModel : ViewModelBase
    {
        public KeyControlViewModel()
        {
            ShowPassword = false;
        }

        private bool _showPassword;
        private KeyManager.Library.Key? _key;

        public bool ShowPassword
        {
            get => _showPassword;
            set => SetProperty(ref _showPassword, value);
        }

        public KeyManager.Library.Key? Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }
    }
}

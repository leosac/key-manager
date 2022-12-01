using Leosac.KeyManager.Library.DivInput;
using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library
{
    public class KeyLink : Link
    {
        public KeyLink() : base()
        {
            
        }

        private string? _containerSelector;

        public string? ContainerSelector
        {
            get => _containerSelector;
            set => SetProperty(ref _containerSelector, value);
        }
    }
}

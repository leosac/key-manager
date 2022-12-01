using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library
{
    public class KeyMaterial : KMObject
    {
        public KeyMaterial(string value = "", string? name = null)
        {
            _value = value;
            _name = name;
        }

        [JsonIgnore]
        public EventHandler<string>? BeforeValueChanged;

        protected void OnBeforeValueChange(string value)
        {
            if (BeforeValueChanged != null)
            {
                BeforeValueChanged(this, value);
            }
        }

        private string _value;

        public string Value
        {
            get => _value;
            set
            {
                OnBeforeValueChange(value);
                SetProperty(ref _value, value);
            }
        }

        private string? _name;

        public string? Name
        {
            get => _name;
            set { SetProperty(ref _name, value); }
        }
    }
}

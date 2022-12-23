using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library
{
    public class KeyMaterial : KMObject
    {
        public const string PRIVATE_KEY = "Private Key";
        public const string PUBLIC_KEY = "Public Key";

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

        public static T? GetFormattedValue<T>(string value, KeyValueFormat format) where T : class
        {
            T? v;
            switch (format)
            {
                case KeyValueFormat.HexString:
                    if (typeof(T) != typeof(string))
                        throw new InvalidCastException();
                    v = value as T;
                    break;
                case KeyValueFormat.HexStringWithSpace:
                    if (typeof(T) != typeof(string))
                        throw new InvalidCastException();
                    v = Regex.Replace(value, ".{2}", "$0 ") as T;
                    break;
                case KeyValueFormat.Binary:
                default:
                    if (typeof(T) != typeof(byte[]))
                        throw new InvalidCastException();
                    v = Convert.FromHexString(value) as T;
                    break;
            }
            return v;
        }

        public T? GetFormattedValue<T>(KeyValueFormat format) where T : class
        {
            return GetFormattedValue<T>(Value, format);
        }

        public void SetFormattedValue(object value, KeyValueFormat format)
        {
            switch (format)
            {
                case KeyValueFormat.HexString:
                case KeyValueFormat.HexStringWithSpace:
                    Value = value as string;
                    break;
                case KeyValueFormat.Binary:
                default:
                    Value = Convert.ToHexString(value as byte[]);
                    break;
            }
        }
    }
}

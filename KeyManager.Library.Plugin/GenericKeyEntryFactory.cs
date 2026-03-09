using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.Plugin
{
    public abstract class GenericKeyEntryFactory<T1,T2> : KeyEntryFactory
        where T1 : KeyStore.KeyEntry, new()
        where T2 : KeyStore.KeyEntryProperties, new()
    {
        public override KeyStore.KeyEntry CreateKeyEntry()
        {
            return new T1();
        }

        public override Type GetKeyEntryType()
        {
            return typeof(T1);
        }

        public override Type GetPropertiesType()
        {
            return typeof(T2);
        }

        public override KeyStore.KeyEntryProperties CreateKeyEntryProperties()
        {
            return new T2();
        }

        public override KeyStore.KeyEntryProperties? CreateKeyEntryProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<T2>(serialized);
        }
    }
}

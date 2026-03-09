using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.Plugin
{
    public abstract class GenericKeyStoreFactory<T1, T2> : KeyStoreFactory
        where T1 : KeyStore.KeyStore, new()
        where T2 : KeyStore.KeyStoreProperties, new()
    {
        public override KeyStore.KeyStore CreateKeyStore()
        {
            return new T1();
        }

        public override Type? GetPropertiesType()
        {
            return typeof(T2);
        }

        public override KeyStore.KeyStoreProperties CreateKeyStoreProperties()
        {
            return new T2();
        }

        public override KeyStore.KeyStoreProperties? CreateKeyStoreProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<T2>(serialized);
        }
    }
}

using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.ISLOG
{
    public class ISLOGKeyStoreFactory : KeyStoreFactory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public override string Name => "ISLOG SAM Manager Template";

        public override KeyStore CreateKeyStore()
        {
            return new ISLOGKeyStore();
        }

        public override Type GetPropertiesType()
        {
            return typeof(ISLOGKeyStoreProperties);
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new ISLOGKeyStoreProperties();
        }

        public override KeyStoreProperties? CreateKeyStoreProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<ISLOGKeyStoreProperties>(serialized);
        }
    }
}

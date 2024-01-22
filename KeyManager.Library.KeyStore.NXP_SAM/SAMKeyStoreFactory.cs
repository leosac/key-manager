using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMKeyStoreFactory : KeyStoreFactory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public override string Name => "NXP SAM AV2/AV3";

        public override KeyStore CreateKeyStore()
        {
            return new SAMKeyStore();
        }

        public override Type GetPropertiesType()
        {
            return typeof(SAMKeyStoreProperties);
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new SAMKeyStoreProperties();
        }

        public override KeyStoreProperties? CreateKeyStoreProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<SAMKeyStoreProperties>(serialized);
        }
    }
}

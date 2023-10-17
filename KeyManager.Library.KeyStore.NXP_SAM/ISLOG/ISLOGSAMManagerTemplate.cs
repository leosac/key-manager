using System.Xml.Linq;
using System.Xml.XPath;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.ISLOG
{
    public class ISLOGSAMManagerTemplate
    {
        public static void Import(string fileName, KeyStore keyStore)
        {
            if (System.IO.File.Exists(fileName))
            {
                var xdoc = XDocument.Load(fileName);
                var entries = xdoc.XPathSelectElements("/XMLSAMConfiguration/Keyentrys/item");
                foreach (var entry in entries)
                {
                    // TODO: implements xml parsing here
                }
            }
        }
    }
}

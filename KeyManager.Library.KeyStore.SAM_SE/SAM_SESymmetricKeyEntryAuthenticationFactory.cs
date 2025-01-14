/*
** File Name: SAM_SESymmetricKeyEntryAuthenticationFactory.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file is a factory one. It ensures that we have all the correct object to work with.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryAuthenticationFactory : KeyEntryFactory
    {
        public override string Name => "SAM-SE Authentication Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => new KeyEntryClass[] { KeyEntryClass.Symmetric };

        public override KeyEntry CreateKeyEntry()
        {
            return new SAM_SESymmetricKeyEntryAuthentication();
        }

        public override Type GetPropertiesType()
        {
            return typeof(SAM_SESymmetricKeyEntryAuthenticationProperties);
        }

        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new SAM_SESymmetricKeyEntryAuthenticationProperties();
        }

        public override KeyEntryProperties? CreateKeyEntryProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<SAM_SESymmetricKeyEntryAuthenticationProperties>(serialized);
        }
    }
}

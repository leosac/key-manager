/*
** File Name: SAM_SESymmetricKeyEntryAuthentication.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file generate a Key Entry for Authentication, an objet inherited from Key Manager.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.Properties;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryAuthentication : SAM_SESymmetricKeyEntry
    {
        public SAM_SESymmetricKeyEntryAuthentication() : base()
        {
            Identifier.Id = "SCP3";
            Properties = new SAM_SESymmetricKeyEntryAuthenticationProperties();
            Variant = new KeyEntryVariant() { Name = "AES128" };
        }

        [JsonIgnore]
        public SAM_SESymmetricKeyEntryAuthenticationProperties? SAM_SEAuthenticationProperties
        {
            get { return Properties as SAM_SESymmetricKeyEntryAuthenticationProperties; }
        }

        public override KeyEntryClass KClass => KeyEntryClass.Symmetric;

        public override IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter = null)
        {
            var variants = new List<KeyEntryVariant>();

            if (classFilter == null || classFilter == KeyEntryClass.Symmetric)
            {
                var DESFirevar = new KeyEntryVariant() { Name = "AES128" };
                DESFirevar.KeyContainers.Add(new KeyContainer(Resources.KeyValue, new Key(["AES", KeyEntryClass.Symmetric.ToString()], 16)));
                variants.Add(DESFirevar);
            }

            return variants;
        }
    }
}

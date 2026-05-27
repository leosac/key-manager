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
    public class SAM_SESymmetricKeyEntryReader : SAM_SESymmetricKeyEntry
    {
        public SAM_SESymmetricKeyEntryReader() : base()
        {
            Identifier.Id = "KEYK";
            Properties = new SAM_SESymmetricKeyEntryReaderProperties();
            Variant = new KeyEntryVariant() { Name = "AES128" };
            Variant.KeyContainers.Add(new KeyContainer(Resources.KeyValueNew, new Key(["AES", KeyEntryClass.Symmetric.ToString()], 16)));
            Variant.KeyContainers.Add(new KeyContainer(Resources.KeyValueCurrent, new Key(["AES", KeyEntryClass.Symmetric.ToString()], 16)));
        }

        [JsonIgnore]
        public SAM_SESymmetricKeyEntryReaderProperties? SAM_SEReaderProperties
        {
            get { return Properties as SAM_SESymmetricKeyEntryReaderProperties; }
        }

        public override KeyEntryClass KClass => KeyEntryClass.Symmetric;

        public override IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter = null)
        {
            var variants = new List<KeyEntryVariant>();

            if (classFilter == null || classFilter == KeyEntryClass.Symmetric)
            {
                var DESFirevar = new KeyEntryVariant() { Name = "AES128" };
                DESFirevar.KeyContainers.Add(new KeyContainer(Resources.KeyValueNew, new Key(["AES", KeyEntryClass.Symmetric.ToString()], 16)));
                DESFirevar.KeyContainers.Add(new KeyContainer(Resources.KeyValueCurrent, new Key(["AES", KeyEntryClass.Symmetric.ToString()], 16)));
                variants.Add(DESFirevar);
            }

            return variants;
        }
    }
}

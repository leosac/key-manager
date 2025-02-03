/*
** File Name: SAM_SESymmetricKeyEntryDESFireUID.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file generate a Key Entry for DESFire UID, an objet inherited from Key Manager.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.Properties;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryDESFireUID : SAM_SESymmetricKeyEntry
    {
        public SAM_SESymmetricKeyEntryDESFireUID() : base()
        {
            Identifier.Id = "UIDX";
            Properties = new SAM_SESymmetricKeyEntryDESFireUIDProperties();
            Variant = new KeyEntryVariant() { Name = "AES128" };
            Variant.KeyContainers.Add(new KeyContainer(Resources.KeyValue, new Key(["AES", KeyEntryClass.Symmetric.ToString()], 16)));
        }

        public SAM_SESymmetricKeyEntryDESFireUID(string id) : base()
        {
            Identifier.Id = id;
            Properties = new SAM_SESymmetricKeyEntryDESFireUIDProperties();
            Variant = new KeyEntryVariant() { Name = "AES128" };
            Variant.KeyContainers.Add(new KeyContainer(Resources.KeyValue, new Key(["AES", KeyEntryClass.Symmetric.ToString()], 16)));
        }

        [JsonIgnore]
        public SAM_SESymmetricKeyEntryDESFireUIDProperties? SAM_SEDESFireUIDProperties
        {
            get { return Properties as SAM_SESymmetricKeyEntryDESFireUIDProperties; }
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

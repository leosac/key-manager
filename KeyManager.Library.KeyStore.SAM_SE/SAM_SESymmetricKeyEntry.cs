﻿/*
** File Name: SAM_SESymmetricKeyEntry.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file generate a generic Key Entry, an objet inherited from Key Manager.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.Properties;
using Newtonsoft.Json;
using static Leosac.KeyManager.Library.KeyStore.SAM_SE.SAM_SESymmetricKeyEntryProperties;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntry : KeyEntry
    {
        public SAM_SESymmetricKeyEntry() : base()
        {
            Identifier.Id = "XXXX";
            Properties = new SAM_SESymmetricKeyEntryProperties();
            Variant = new KeyEntryVariant() { Name = "AES128" };
        }

        public SAM_SESymmetricKeyEntry(SAM_SEKeyEntryType type) : base()
        {
            Identifier.Id = "XXXX";
            Properties = new SAM_SESymmetricKeyEntryProperties();
            SAM_SEProperties!.KeyEntryType = type;
            Variant = new KeyEntryVariant() { Name = "AES128" };
        }

        [JsonIgnore]
        public SAM_SESymmetricKeyEntryProperties? SAM_SEProperties
        {
            get { return Properties as SAM_SESymmetricKeyEntryProperties; }
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

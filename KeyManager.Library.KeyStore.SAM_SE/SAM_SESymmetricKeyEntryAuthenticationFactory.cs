/*
** File Name: SAM_SESymmetricKeyEntryAuthenticationFactory.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file is a factory one. It ensures that we have all the correct object to work with.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryAuthenticationFactory : GenericKeyEntryFactory<SAM_SESymmetricKeyEntryAuthentication, SAM_SESymmetricKeyEntryAuthenticationProperties>
    {
        public override string Name => "SAM-SE Authentication Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => [KeyEntryClass.Symmetric];
    }
}

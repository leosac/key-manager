/*
** File Name: SAM_SESymmetricKeyEntryDESFireUIFactory.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file is a factory one. It ensures that we have all the correct object to work with.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI
{
    public class SAM_SESymmetricKeyEntryDESFireUIFactory : KeyEntryUIFactory
    {
        public SAM_SESymmetricKeyEntryDESFireUIFactory()
        {
            targetFactory = new SAM_SESymmetricKeyEntryDESFireFactory();
        }

        public override string Name => "SAM-SE DESFire Key Entry";

        public override Type? GetPropertiesType()
        {
            return typeof(SAM_SESymmetricKeyEntryDESFireProperties);
        }

        public override UserControl CreateKeyEntryPropertiesControl()
        {
            return new SAM_SESymmetricKeyEntryDESFirePropertiesControl();
        }

        public override KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel()
        {
            return new SAM_SESymmetricKeyEntryDESFirePropertiesControlViewModel();
        }
    }
}

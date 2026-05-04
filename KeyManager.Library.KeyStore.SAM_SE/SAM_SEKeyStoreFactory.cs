/*
** File Name: SAM_SEKeyStoreFactory.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file is a factory one. It ensures that we have all the correct object to work with.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SEKeyStoreFactory : GenericKeyStoreFactory<SAM_SEKeyStore, SAM_SEKeyStoreProperties>
    {
        private readonly uint MAJOR = 1;
        private readonly uint MINOR = 5;
        private readonly uint DVL = 6;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public override string Name => "NXP SAM-SE (Synchronic)";

        public override KeyStore CreateKeyStore()
        {
            log.Info(String.Format("Plugin {0} / Version : {1}.{2}.{3}",Name,MAJOR,MINOR,DVL));
            return base.CreateKeyStore();
        }
    }
}

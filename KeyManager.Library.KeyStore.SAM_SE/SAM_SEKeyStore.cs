/*
** File Name: SAM_SEKeyStore.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file regroups all the features of SAM-SE KeyStore.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.Properties;
using static Leosac.KeyManager.Library.KeyStore.SAM_SE.SAM_SESymmetricKeyEntryDESFireProperties;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SEKeyStore : KeyStore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IList<KeyEntryId> _keyEntriesId = [];
        private readonly Dictionary<string,bool> _keyEntriesActive = [];

        public SAM_SEDllEntryPoint SAM_SEDll = new();

        public SAM_SEKeyStoreProperties GetFileProperties()
        {
            var p = Properties as SAM_SEKeyStoreProperties;
            if (p == null)
                throw new KeyStoreException(Resources.KeyStorePropertiesMissing);
            return p;
        }

        public SAM_SEDllEntryPoint? GetSAM_SEDll()
        {
            return SAM_SEDll;
        }

        public override string Name => "NXP SAM-SE (Synchronic)";

        public override bool CanCreateKeyEntries => false;

        public override bool CanDeleteKeyEntries => false;

        public override bool CanReorderKeyEntries => false;

        public override IEnumerable<KeyEntryClass> SupportedClasses
        {
            get => new KeyEntryClass[] { KeyEntryClass.Symmetric };
        }

        public override Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            if(identifier.Id != null && _keyEntriesId.Where(k => k.Id == identifier.Id).ToList().Count == 1)
            {
                if (keClass == KeyEntryClass.Symmetric)
                {
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public override Task Close(bool secretCleanup = true)
        {
            SAM_SEDll.DeselectProgrammingStation(GetFileProperties().ProgrammingStationPath);
            return base.Close(secretCleanup);
        }

        public override Task Open()
        {
            log.Info("Opening SAM-SE KeyStore ...");

            SAM_SEDll.SelectProgrammingStation(GetFileProperties().ProgrammingStationPath);

            int ret;

            //Depending on GUI, we connect with default secret or not
            if (GetFileProperties().DefaultKey == true)
            {
                if (GetFileProperties().Secret!.Length != 0)
                    log.Warn("A password have been typed but it's not used");
                ret = SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.ConnectToSAM_SE(null, 0);
            }
            else
            {
                ret = SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.ConnectToSAM_SE(GetFileProperties().Secret!, Convert.ToUInt16(GetFileProperties().Secret!.Length));
            }

            //Auto update SAM-SE
            if (GetFileProperties().AutoUpdate)
            {
                SAM_SELockLevel lockLvl = SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.GetLockLevel();
                //We try a command that we know will throw a Warning about file not being up to date
                try
                {
                    SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.SetLockLevel(lockLvl);
                }
                catch (KeyStoreException ex)
                {
                    //If this is the warning we search about
                    if (ex.Message == Resources.SAM_SEErrorOldVersion)
                    {
                        //Then we call the method to update the configuration file
                        SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.SetDefaultConfigurationFile();
                    }
                }
            }

            //Auto lock SAM-SE
            if (GetFileProperties().AutoLock)
            {
                SAM_SELockLevel lockLvl = (SAM_SELockLevel)GetFileProperties().Locked;
                SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.SetLockLevel(lockLvl);
            }

            if (ret != 0)
                log.Error(String.Format("Error {0} while connecting", ret));

            log.Info("SAM-SE KeyStore opened");
            return Task.CompletedTask;
        }

        public override Task Create(IChangeKeyEntry change)
        {
            //It's not allowed to create new objects inside a SAM-SE
            return Task.CompletedTask;
        }

        public override Task Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing = false)
        {
            //It's not allowed to delete new objects inside a SAM-SE
            return Task.CompletedTask;
        }

        public override async Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            if (!await CheckKeyEntryExists(identifier, keClass))
            {
                log.Error(string.Format("The key entry `{0}` do not exists.", identifier));
                throw new KeyStoreException(Resources.KeyStoreKeyEntryMissing);
            }

            //For each key inside the SAM-SE, we create an object
            SAM_SEDllObject? DllKey = SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.GetKey(identifier.Id!);
            if (DllKey == null)
            {
                log.Error(string.Format("The DLL doesn't know the key entry `{0}`.", identifier));
                throw new KeyStoreException(Resources.KeyStoreKeyEntryMissing);
            }

            SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType type = DllKey.GetKeyType();

            SAM_SESymmetricKeyEntry? keyEntry;
            switch (type)
            {
                case SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.Authenticate:
                    keyEntry = new SAM_SESymmetricKeyEntryAuthentication();
                    break;
                case SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFire:
                    keyEntry = new SAM_SESymmetricKeyEntryDESFire(identifier.Id!, DllKey.GetUidKeyLinked()!, _keyEntriesActive[identifier.Id!]);
                    break;
                case SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFireUID:
                    keyEntry = new SAM_SESymmetricKeyEntryDESFireUID(identifier.Id!);
                    break;
                default:
                case SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.Default:
                    keyEntry = null;
                    return keyEntry;
            }

            //Keeping track of Label
            keyEntry.Identifier.Label = identifier.Label;

            //Getting metadatas of every object
            GetMetadataKeyEntry(keyEntry);
            GetPolitics(keyEntry);

            //If the key entry is a DESFire one, it will have a UID link
            if (keyEntry is SAM_SESymmetricKeyEntryDESFire des)
            {
                //We get the linked key
                KeyEntry? keyEntryUID = GetFromId(des.SAM_SEDESFireProperties!.Div.UidLinkId!, KeyEntryClass.Symmetric);
                if (keyEntryUID != null && keyEntryUID is SAM_SESymmetricKeyEntryDESFireUID desUid)
                {
                    //And we assign the values from DESFire UID to DESFire so we can display them
                    des.SAM_SEDESFireProperties!.Div.UidLinkKeyNum = desUid.SAM_SEDESFireUIDProperties!.KeyNum;
                    des.SAM_SEDESFireProperties!.Div.UidLinkEnable = desUid.SAM_SEDESFireUIDProperties!.Enable;
                }
            }
            return keyEntry;
        }

        public KeyEntry? GetFromId(string id, KeyEntryClass keClass)
        {
            if (id == null)
                return null;

            return Get(new KeyEntryId(id),keClass).Result;
        }

        public override Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass = null)
        {
            log.Info(String.Format("Getting all key entries (class: `{0}`)...", keClass));
            _keyEntriesId.Clear();

            //Extracting informations from the configuration file inside the SAM-SE
            SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.DownloadMetadatas();

            //Getting the lock level of the SAM-SE
            GetFileProperties().LockedLevel = SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.GetLockLevel();
            log.Info(String.Format("Lock Level : {0}", GetFileProperties().LockedLevel));

            //Getting list Id from DLL
            SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.GetKeysList(_keyEntriesId);

            InitConfigurationActive(_keyEntriesId);

            log.Info(String.Format("{0} key entries returned.", _keyEntriesId.Count));
            return Task.FromResult(_keyEntriesId);
        }

        public override Task Store(IList<IChangeKeyEntry> changes)
        {
            log.Info(String.Format("Storing `{0}` key entries...", changes.Count));
            //Getting all the informations from the SAM-SE before updating its entries
            GetAll();
            foreach (var change in changes)
            {
                Update(change, true);
            }
            log.Info("Key Entries storing completed.");
            return Task.CompletedTask;
        }

        public override Task Update(IChangeKeyEntry change, bool ignoreIfMissing = false)
        {
            log.Info(String.Format("Updating key entry `{0}`...", change.Identifier));

            //Getting object using informations from arguments
            KeyEntry? keyEntry = GetFromId(change.Identifier.Id!, change.KClass) ?? throw new KeyStoreException(Resources.KeyStoreKeyEntryMissing);

            if (change is SAM_SESymmetricKeyEntry keyEntryModification)
            {
                //We need to update the Identifier, cause the Label may have changed
                keyEntry.Identifier = keyEntryModification.Identifier;

                if (keyEntryModification is SAM_SESymmetricKeyEntryAuthentication)
                {
                    //Authenticate Key, not much to do
                    UpdatePassword(keyEntryModification);
                }
                else if (keyEntryModification is SAM_SESymmetricKeyEntryDESFire || 
                         keyEntryModification is SAM_SESymmetricKeyEntryDESFireUID)
                {
                    if (keyEntryModification is SAM_SESymmetricKeyEntryDESFire keyEntryModificationDESFire)
                    {
                        UpdateConfigurationActive(keyEntryModificationDESFire.Identifier, keyEntryModificationDESFire.SAM_SEDESFireProperties!.ReadingMode == SAM_SEDESFireMode.IDP);
                    }
                    //Updating the object
                    if (UpdateObject(keyEntryModification) != 0)
                    {
                        log.Error(String.Format("Error while updating the secret of key entry `{0}`.", keyEntryModification.Identifier));
                    }
                    //Updating the metadatas of the object
                    if (SetMetadataKeyEntry(keyEntryModification) != 0)
                    {
                        log.Error(String.Format("Error while updating DESFire metadata of key entry `{0}`.", keyEntryModification.Identifier));
                    }
                }
                else
                {
                    throw new KeyStoreException(Resources.KeyStoreKeyEntryTypeMissing);
                }

                //Notifying the KML that we updated a Key Entry
                OnKeyEntryUpdated(change);
                log.Info(String.Format("Key entry `{0}` updated.", change.Identifier));

                //Clear the password string so nothing is left in the RAM
                ClearPassword(keyEntryModification);
            }
            else
            {
                throw new KeyStoreException(Resources.KeyStoreEntryNotSupported);
            }

            return Task.CompletedTask;
        }

        //This method updates the activation/deactivation of configuration based on the previous one :
        //Activation Ex :   Conf 1 = Private ID ; Conf 2 is active but disabled
        //                  Conf 2 is modified to Private ID ; Then Conf 3 needs to be activated ; and so on
        //Deactivation Ex : Conf 1 = Private ID ; Conf 2 = Private ID ; Conf 3 = Private ID ; Conf 4 = Private ID
        //                  Conf 2 is modified to Disabled ; Then Conf 3 and 4 need to be deactivated
        //The exception is for "BIOA", biometric conf which is enabled if Conf 1 is Private ID
        private void UpdateConfigurationActive(KeyEntryId changeKeyEntryId, bool nextConfActive)
        {
            bool start = false;
            foreach (KeyEntryId keyEntryId in _keyEntriesId)
            {
                //We find the modified key entry
                if (keyEntryId.Id == changeKeyEntryId.Id)
                {
                    start = true;
                }
                //We wait one lap to modify the next key entry
                else if (start)
                {
                    //We modify the dictionnary which contains the active or not info
                    if (keyEntryId.Id!.StartsWith("DF"))
                        _keyEntriesActive[keyEntryId.Id!] = nextConfActive;
                    //Specific case of "BIOA" which is active if "DF1A" is not Disabled
                    if (changeKeyEntryId.Id == "DF1A")
                        _keyEntriesActive["BIOA"] = nextConfActive;
                    //If the next conf is active, then we need to stop
                    if (nextConfActive)
                    {
                        break;
                    }
                    //If the conf is inactive
                    else
                    {
                        //Then we need to loop on every DESFire Key entry after the one to put them to disabled
                        if (SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.GetKey(keyEntryId.Id!) is SAM_SEDllDESFire desfire)
                        {
                            //If the key is a "DFXX" one, then we follow the same way as others
                            //But if it's "BIOA" we need to know if the inactive comes from "DF1A" to apply it
                            if (keyEntryId.Id!.StartsWith("DF") ||
                                keyEntryId.Id! == "BIOA" && changeKeyEntryId.Id! == "DF1A")
                                desfire.SetReadingMode(SAM_SEDESFireMode.Disable);
                        }
                    }
                }
            }
            if (!nextConfActive)
                SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.UploadMetadata();
        }

        //This method uses the informations from DLL to update the list which handles which conf is active or not
        private void InitConfigurationActive(IList<KeyEntryId> keyEntries)
        {
            //We start at true, cause the first DESFire configuration must be active
            bool active = true;
            //We make a loop on every key entry
            foreach (KeyEntryId keyEntry in keyEntries)
            {
                //The ones who starts with DF are the ones we want to explore
                if (keyEntry.Id!.StartsWith("DF"))
                {
                    //The first conf we encounter is the DF1A, so we can say it's active
                    _keyEntriesActive[keyEntry.Id!] = active;
                    //Then we get the key on the dll
                    SAM_SEDllObject? temp = SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.GetKey(keyEntry.Id!);
                    //Making sure it's a DESFire one
                    if (temp is SAM_SEDllDESFire des)
                    {
                        //Then we get the reading mode information, and if it's IDP, then we put true in active
                        active = des.GetReadingMode() == SAM_SEDESFireMode.IDP;
                    }
                }
                //The other ones are not active or not, we just put false inside
                else
                    _keyEntriesActive[keyEntry.Id!] = false;
            }

            //Now we handle the specific case, it has been created with false as parameter
            if (_keyEntriesActive.ContainsKey("BIOA"))
            {
                //To activate the BIOA conf, we want to check that at least DF1A to be IDP
                SAM_SEDllObject? temp = SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.GetKey("DF1A");
                if (temp is SAM_SEDllDESFire des)
                {
                    //We check this info and set it inside the dictionnary
                    _keyEntriesActive["BIOA"] = des.GetReadingMode() == SAM_SEDESFireMode.IDP;
                }
            }
        }

        //Method to update the key of an object
        private int UpdateObject(SAM_SESymmetricKeyEntry keyEntry)
        {
            int ret;
            string? key = keyEntry.Variant!.KeyContainers[0].Key.GetAggregatedValueAsString();
            if (string.IsNullOrEmpty(key))
            {
                return 0;
            }
            ret = SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.GetKey(keyEntry.Identifier.Id!)!.UploadKey(key);
            if (ret != 0)
            {
                log.Error(String.Format("Error {0} modifying object.", ret));
            }
            return ret;
        }

        //Method to update the password used to connect to the SAM-SE
        private void UpdatePassword(SAM_SESymmetricKeyEntry keyEntry)
        {
            if (keyEntry is SAM_SESymmetricKeyEntryAuthentication keyAuth)
            {
                //No password in GUI, so no need to change the password
                if (keyAuth.SAM_SEAuthenticationProperties!.Password == string.Empty)
                {
                    log.Info("There is no password to modify.");
                    return;
                }
                //If we're here, then there is a password to update
                log.Info("Modifying password...");

                //Checking the validity of the new password
                if (!keyAuth.SAM_SEAuthenticationProperties!.PasswordValid)
                    throw new KeyStoreException(Resources.KeyStorePasswordInvalid);
                if (!keyAuth.SAM_SEAuthenticationProperties!.PasswordMatch)
                    throw new KeyStoreException(Resources.KeyStorePasswordNotMatching);

                //Here, the password is valid, so we can update it
                //First we make sure the object is the right type
                if (SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.GetKey("SCP3") is SAM_SEDllAuthenticate temp)
                {
                    //Then we call the DLL method to update the password
                    int ret = temp.ChangePassword(keyAuth.SAM_SEAuthenticationProperties!.Password, Convert.ToUInt16(keyAuth.SAM_SEAuthenticationProperties!.Password.Length));
                    if (ret != 0)
                    {
                        log.Error(String.Format("Error {0} modifying password.", ret));
                    }
                    log.Info("Password modified.");
                    return;
                }
                else
                {
                    log.Info("The key is not an Authenticate type.");
                    return;
                }
            }
            else
            {
                log.Info("The key is not an Authenticate type.");
                return;
            }
        }

        //Method used to clear the informations from the password
        private static void ClearPassword(SAM_SESymmetricKeyEntry keyEntry)
        {
            if (keyEntry is SAM_SESymmetricKeyEntryAuthentication keyAuth)
            {
                //We clear the password modified once the modification is done, or not
                keyAuth.SAM_SEAuthenticationProperties!.Password = string.Empty;
                keyAuth.SAM_SEAuthenticationProperties!.PasswordConfirmation = string.Empty;
            }
        }

        //Method to regroup all the call of metadatas getters methods inside only method
        private void GetMetadataKeyEntry(SAM_SESymmetricKeyEntry keyEntry)
        {
            log.Info(String.Format("Getting key entry `{0}` metadatas...", keyEntry.Identifier.Id));
            SAM_SEDllObject? temp = SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.GetKey(keyEntry.Identifier.Id!);
            if (temp is SAM_SEDllDESFire desfire && keyEntry is SAM_SESymmetricKeyEntryDESFire des)
            {
                des.SAM_SEDESFireProperties!.ReadingMode = desfire.GetReadingMode();
                if (des.SAM_SEDESFireProperties!.ReadingMode == SAM_SEDESFireMode.IDP)
                {
                    des.SAM_SEDESFireProperties!.Msb = desfire.GetMsb();
                    des.SAM_SEDESFireProperties!.Ev0 = desfire.GetEv0();
                    des.SAM_SEDESFireProperties!.Ev1 = desfire.GetEv1();
                    des.SAM_SEDESFireProperties!.Ev2 = desfire.GetEv2();
                    des.SAM_SEDESFireProperties!.Ev3 = desfire.GetEv3();
                    des.SAM_SEDESFireProperties!.Jcop = desfire.GetJcop();
                    des.SAM_SEDESFireProperties!.Jcop = desfire.GetJcopEv3();
                    des.SAM_SEDESFireProperties!.AuthEv2 = desfire.GetAuthEv2();
                    des.SAM_SEDESFireProperties!.ProximityCheck = desfire.GetProximityCheck();
                    des.SAM_SEDESFireProperties!.AidString = BitConverter.ToString(desfire.GetAid()).Replace("-", "");
                    des.SAM_SEDESFireProperties!.KeyNum = desfire.GetKeyNumber();
                    des.SAM_SEDESFireProperties!.FileNum = desfire.GetFileNumber();
                    des.SAM_SEDESFireProperties!.Offset = desfire.GetOffset();
                    des.SAM_SEDESFireProperties!.Size = desfire.GetSize();
                    des.SAM_SEDESFireProperties!.EncryptType = desfire.GetEncryption();
                    des.SAM_SEDESFireProperties!.Communication = desfire.GetCommunication();
                    des.SAM_SEDESFireProperties!.Div.Enable = desfire.GetDivEnable();
                    if (des.SAM_SEDESFireProperties!.Div.Enable == true)
                    {
                        des.SAM_SEDESFireProperties!.Div.AidInverted = desfire.GetDivAidInv();
                        des.SAM_SEDESFireProperties!.Div.KeyInc = desfire.GetDivKeyEnable();
                        des.SAM_SEDESFireProperties!.Div.SiString = BitConverter.ToString(desfire.GetDivSi()).Replace("-", "");
                    }
                }
            }
            else if (temp is SAM_SEDllDESFireUid desfireUID && keyEntry is SAM_SESymmetricKeyEntryDESFireUID desUid)
            {
                desUid.SAM_SEDESFireUIDProperties!.Enable = desfireUID.GetUidEnable();
                if (desUid.SAM_SEDESFireUIDProperties!.Enable == true)
                {
                    desUid.SAM_SEDESFireUIDProperties!.KeyNum = desfireUID.GetUidKeyNumber();
                }
            }
            else
            {
                //Nothing to do !
            }
            log.Info(String.Format("Key entry `{0}` metadatas retrieved.", keyEntry.Identifier.Id));
        }

        //Method to regroup all the call of metadatas setters methods inside only method
        private int SetMetadataKeyEntry(SAM_SESymmetricKeyEntry keyEntry)
        {
            log.Info(String.Format("Setting key entry `{0}` metadatas...",keyEntry.Identifier.Id));

            //Récupération de l'objet, de type objet car on ne sait pas encore son type
            SAM_SEDllObject? temp = SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.GetKey(keyEntry.Identifier.Id!);
            if (temp is SAM_SEDllDESFire desfire && keyEntry is SAM_SESymmetricKeyEntryDESFire des)
            {
                //On retire les warnings, car on part du principe qu'ils ne seront plus là
                des.SAM_SEDESFireProperties!.CommunicationWarning = false;
                des.SAM_SEDESFireProperties!.EncryptTypeWarning = false;
                des.SAM_SEDESFireProperties!.ReadingModeWarning = false;
                if (des.SAM_SEDESFireProperties!.Aid == null)
                    throw new KeyStoreException(Resources.AidNotValid);
                if (des.SAM_SEDESFireProperties!.Div.SiString.Length %2 != 0)
                    throw new KeyStoreException(Resources.SiNotValid);
                desfire.SetMsb(des.SAM_SEDESFireProperties!.Msb);
                desfire.SetEv0(des.SAM_SEDESFireProperties!.Ev0);
                desfire.SetEv1(des.SAM_SEDESFireProperties!.Ev1);
                desfire.SetEv2(des.SAM_SEDESFireProperties!.Ev2);
                desfire.SetEv3(des.SAM_SEDESFireProperties!.Ev3);
                desfire.SetJcop(des.SAM_SEDESFireProperties!.Jcop);
                desfire.SetJcopEv3(des.SAM_SEDESFireProperties!.JcopEv3);
                desfire.SetAuthEv2(des.SAM_SEDESFireProperties!.AuthEv2);
                desfire.SetProximityCheck(des.SAM_SEDESFireProperties!.ProximityCheck);
                desfire.SetAid(des.SAM_SEDESFireProperties!.Aid!);
                desfire.SetKeyNumber(des.SAM_SEDESFireProperties!.KeyNum);
                desfire.SetFileNumber(des.SAM_SEDESFireProperties!.FileNum);
                desfire.SetOffset(des.SAM_SEDESFireProperties!.Offset);
                desfire.SetSize(des.SAM_SEDESFireProperties!.Size);
                desfire.SetEncryption(des.SAM_SEDESFireProperties!.EncryptType);
                desfire.SetCommunication(des.SAM_SEDESFireProperties!.Communication);
                desfire.SetReadingMode(des.SAM_SEDESFireProperties!.ReadingMode);
                desfire.SetDivEnable(des.SAM_SEDESFireProperties!.Div.Enable);
                if(des.SAM_SEDESFireProperties!.Div.Enable)
                {
                    desfire.SetDivAidInv(des.SAM_SEDESFireProperties!.Div.AidInverted);
                    desfire.SetDivSi(des.SAM_SEDESFireProperties!.Div.Si, des.SAM_SEDESFireProperties!.Div.KeyInc, des.SAM_SEDESFireProperties!.KeyNum);
                }
            }
            else if (temp is SAM_SEDllDESFireUid desfireUID && keyEntry is SAM_SESymmetricKeyEntryDESFireUID desUid)
            {
                desfireUID.SetUidEnable(desUid.SAM_SEDESFireUIDProperties!.Enable);
                desfireUID.SetUidKeyNumber(desUid.SAM_SEDESFireUIDProperties!.KeyNum);
            }
            else
            {
                //Nothing to do
            }
            log.Info(String.Format("Key entry `{0}` metadatas updated.", keyEntry.Identifier.Id));
            return SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.UploadMetadata();
        }

        //Method to regroup all the call of policies getters methods inside only method
        private void GetPolitics(SAM_SESymmetricKeyEntry keyEntry)
        {
            log.Info(String.Format("Getting key entry `{0}` policy...", keyEntry.Identifier.Id));
            SAM_SEDllObject? temp = SAM_SEDll.GetCurrentProgrammingStation()!.SAM_SE.GetKey(keyEntry.Identifier.Id!);
            if (temp != null)
            {
                keyEntry.SAM_SEProperties!.Politics.Read = temp.GetPolicyReadable();
                keyEntry.SAM_SEProperties!.Politics.Write = temp.GetPolicyWritable();
                keyEntry.SAM_SEProperties!.Politics.Import = temp.GetPolicyImpExport();
                keyEntry.SAM_SEProperties!.Politics.Wrap = temp.GetPolicyWrapable();
                keyEntry.SAM_SEProperties!.Politics.Encrypt = temp.GetPolicyEncypher();
                keyEntry.SAM_SEProperties!.Politics.Decrypt = temp.GetPolicyDecypher();
                keyEntry.SAM_SEProperties!.Politics.AuthDESFire = temp.GetPolicyAuthDes();
                keyEntry.SAM_SEProperties!.Politics.SessionKeyDESFire = temp.GetPolicySessionDesDump();
            }
            
            log.Info(String.Format("Key entry `{0}` policy updated.", keyEntry.Identifier.Id));
        }
    }
}
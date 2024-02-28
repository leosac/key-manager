using Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.Properties;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SEKeyStore : KeyStore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IList<SAM_SESymmetricKeyEntry> _keyEntries = new List<SAM_SESymmetricKeyEntry>();

        public SAM_SEDllProgrammingStation SAM_SEDll = new();

        public SAM_SEKeyStoreProperties GetFileProperties()
        {
            var p = Properties as SAM_SEKeyStoreProperties;
            if (p == null)
                throw new KeyStoreException(Resources.KeyStorePropertiesMissing);
            return p;
        }

        public SAM_SEDllProgrammingStation? GetSAM_SEDll()
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
            return CheckKeyEntryExists(identifier, keClass, out _);
        }

        protected Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass, out KeyEntry? keyEntry)
        {
            keyEntry = _keyEntries.Where(k => k.Identifier == identifier && k.KClass == keClass).SingleOrDefault();
            return Task.FromResult(keyEntry != null);
        }

        public override Task Close()
        {
            int index = SAM_SEDll.FindSAM_SEByMac(GetFileProperties().SAM_SEMac);
            if (index != -1)
                SAM_SEDll.DeselectSAM_SE((uint)index);
            return Task.CompletedTask;
        }

        public override Task Open()
        {
            log.Info("Opening SAM-SE KeyStore ...");

            SAM_SEDll.DetectSAM_SEBlocking();

            int index = SAM_SEDll.FindSAM_SEByMac(GetFileProperties().SAM_SEMac);

            if (index == -1)
            {
                throw new KeyStoreException(Resources.ProgrammingStationMissing);
            }

            //Opening DLL
            SAM_SEDll.SelectSAM_SE((uint)index);

            int ret;

            //Depending on GUI, we connect with default secret or not
            if (GetFileProperties().DefaultKey == true && GetFileProperties().Secret!.Length == 0)
            {
                ret = SAM_SEDll.GetCurrentSAM_SE()!.ConnectToSAM_SE(null, 0);
            }
            else
            {
                ret = SAM_SEDll.GetCurrentSAM_SE()!.ConnectToSAM_SE(GetFileProperties().Secret!, Convert.ToUInt16(GetFileProperties().Secret!.Length));
            }

            if(ret != 0)
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
            if (!await CheckKeyEntryExists(identifier, keClass, out KeyEntry? keyEntry))
            {
                throw new KeyStoreException(Resources.KeyStoreKeyEntryMissing);
            }
            return keyEntry;
        }

        public override Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass = null)
        {
            log.Info(String.Format("Getting all key entries (class: `{0}`)...", keClass));
            IList<KeyEntryId> entries = new List<KeyEntryId>();

            //Extracting informations from the configuration file inside the SAM-SE
            SAM_SEDll.GetCurrentSAM_SE()!.DownloadMetadatas();

            //Getting the lock level of the SAM-SE
            GetFileProperties().LockedLevel = SAM_SEDll.GetCurrentSAM_SE()!.GetLockLevel();
            log.Info(String.Format("Lock Level : {0}", GetFileProperties().LockedLevel));

            //Getting list Id from DLL, and clean the old one
            SAM_SEDll.GetCurrentSAM_SE()!.GetKeysList(entries);
            _keyEntries.Clear();

            if (keClass == null || keClass == KeyEntryClass.Symmetric)
            {
                //We check every key that is symmetric
                foreach (KeyEntryId ele in entries)
                {
                    //For each key inside the SAM-SE, we create an object
                    SAM_SEDllObject? temp = SAM_SEDll.GetCurrentSAM_SE()!.GetKey(ele.Id!);
                    //At this point it's a generic object
                    SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType type = SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.Default;
                    if (temp != null)
                    {
                        type = temp.GetKeyType();
                        //Adding keyEntries inside key list
                        _keyEntries.Add(new(ele, type));
                        //Getting link between DESFire and DESFire UID ; if it exists
                        if (temp.LinkedObject != null)
                            _keyEntries.Last().SAM_SEProperties!.DESFire.Div.UidLinkId = temp.LinkedObject.StringId;
                        else
                            _keyEntries.Last().SAM_SEProperties!.DESFire.Div.UidLinkId = null;
                    }
                }
                foreach (SAM_SESymmetricKeyEntry keyEntry in _keyEntries)
                {
                    //Getting metadatas of every object
                    GetMetadataKeyEntry(keyEntry);
                    GetPolitics(keyEntry);
                }
                foreach (SAM_SESymmetricKeyEntry keyEntry in _keyEntries)
                {
                    //If there is a link between DESFire and DESFire UID
                    if (keyEntry.SAM_SEProperties!.DESFire.Div.UidLinkId != null)
                    {
                        //We get the linked key
                        SAM_SESymmetricKeyEntry? keyEntryUID = GetKeyEntry(keyEntry.SAM_SEProperties!.DESFire.Div.UidLinkId, KeyEntryClass.Symmetric);
                        if (keyEntryUID != null)
                        {
                            //And we assign the values from DESFire UID to DESFire so we can display them
                            keyEntry.SAM_SEProperties!.DESFire.Div.UidLinkKeyNum = keyEntryUID.SAM_SEProperties!.DESFireUID.KeyNum;
                            keyEntry.SAM_SEProperties!.DESFire.Div.UidLinkEnable = keyEntryUID.SAM_SEProperties!.DESFireUID.Enable;
                        }
                    }
                }
                //Update enabled conf depending on the linked objects
                UpdateConfEnabled();
            }

            log.Info(String.Format("{0} key entries returned.", entries.Count));
            return Task.FromResult(entries);
        }

        public override Task Store(IList<IChangeKeyEntry> changes)
        {
            log.Info(String.Format("Storing `{0}` key entries...", changes.Count));
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
            int ret = 0;

            //Getting object using informations from arguments
            SAM_SESymmetricKeyEntry? keyEntry = GetKeyEntry(change.Identifier.Id!, change.KClass) ?? throw new KeyStoreException(Resources.KeyStoreKeyEntryMissing);

            //We need to update the Identifier, cause the Label may have changed
            keyEntry.Identifier = change.Identifier;

            //Updating differently depending on the key type
            switch(keyEntry.SAM_SEProperties!.KeyEntryType)
            {
                case SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.Authenticate:
                    //Authenticate Key, not much to do
                    UpdatePassword(keyEntry);
                    break;
                case SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFireUID:
                    //Getting the DESFire UID key
                    SAM_SEDllObject? temp = SAM_SEDll.GetCurrentSAM_SE()!.GetKey(keyEntry.Identifier.Id!) ?? throw new KeyStoreException(Resources.KeyStoreKeyEntryTypeMissing);
                    SAM_SESymmetricKeyEntry? keyEntryDESFire = GetKeyEntry(temp.StringId, keyEntry.KClass);
                    if (keyEntryDESFire != null)
                    {
                        //Updating values inside the DESFire object
                        keyEntryDESFire.SAM_SEProperties!.DESFire.Div.UidLinkKeyNum = keyEntry.SAM_SEProperties!.DESFireUID.KeyNum;
                        keyEntryDESFire.SAM_SEProperties!.DESFire.Div.UidLinkEnable = keyEntry.SAM_SEProperties!.DESFireUID.Enable;
                    }
                    //Updating the object
                    ret = UpdateObject(keyEntry);
                    if(ret != 0)
                    {
                        log.Error(String.Format("Error while updating the secret of key entry `{0}`.", change.Identifier));
                    }
                    //Updating the metadatas of the object
                    ret = SetMetadataKeyEntry(keyEntry);
                    if (ret != 0)
                    {
                        log.Error(String.Format("Error while updating DESFire metadata of key entry `{0}`.", change.Identifier));
                    }
                    break;
                case SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFire:
                    //Updating the object
                    ret = UpdateObject(keyEntry);
                    if (ret != 0)
                    {
                        log.Error(String.Format("Error while updating the secret of key entry `{0}`.", change.Identifier));
                    }
                    //Updating its metadatas
                    ret = SetMetadataKeyEntry(keyEntry);
                    if (ret != 0)
                    {
                        log.Error(String.Format("Error while updating DESFire metadata of key entry `{0}`.", change.Identifier));
                    }
                    break;
                case SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.Default:
                default:
                    //That's not supposed to happen
                    throw new KeyStoreException(Resources.KeyStoreKeyEntryTypeMissing);
            }

            //Update enabled conf depending on the linked objects
            UpdateConfEnabled();

            //Notifying the KML that we updated a Key Entry
            OnKeyEntryUpdated(change);
            log.Info(String.Format("Key entry `{0}` updated.", change.Identifier));

            //Clear the password string so nothing is left in the RAM
            ClearPassword(keyEntry);

            return Task.CompletedTask;
        }

        //Method to get a key entry from its id
        private SAM_SESymmetricKeyEntry? GetKeyEntry (string id, KeyEntryClass kClass)
        {
            SAM_SESymmetricKeyEntry? keyEntry = _keyEntries.Where(k => k.Identifier.Id == id && k.KClass == kClass).SingleOrDefault();
            return keyEntry;
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
            ret = SAM_SEDll.GetCurrentSAM_SE()!.GetKey(keyEntry.Identifier.Id!)!.UploadKey(key);
            if (ret != 0)
            {
                log.Error(String.Format("Error {0} modifying object.", ret));
            }
            return ret;
        }

        //Method to update the password used to connect to the SAM-SE
        private void UpdatePassword(SAM_SESymmetricKeyEntry keyEntry)
        {
            //No password in GUI, so no need to change the password
            if (keyEntry.SAM_SEProperties!.Authenticate.Password == string.Empty)
            {
                log.Info("There is no password to modify.");
                return;
            }
            //If we're here, then there is a password to update
            log.Info("Modifying password...");

            //Checking the validity of the new password
            if (!keyEntry.SAM_SEProperties!.Authenticate.PasswordValid)
                throw new KeyStoreException(Resources.KeyStorePasswordInvalid);
            if(!keyEntry.SAM_SEProperties!.Authenticate.PasswordMatch)
                throw new KeyStoreException(Resources.KeyStorePasswordNotMatching);

            //Here, the password is valid, so we can update it
            //First we make sure the object is the right type
            if(SAM_SEDll.GetCurrentSAM_SE()!.GetKey("SCP3") is SAM_SEDllAuthenticate temp)
            {
                //Then we call the DLL method to update the password
                int ret = temp.ChangePassword(keyEntry.SAM_SEProperties!.Authenticate.Password, Convert.ToUInt16(keyEntry.SAM_SEProperties.Authenticate.Password.Length));
                if (ret != 0)
                {
                    log.Error(String.Format("Error {0} modifying password.", ret));
                }
                log.Info("Password modified.");
                return;
            }
            log.Info("The key is not an Authenticate type.");
            return;
        }

        //Method used to clear the informations from the password
        private static void ClearPassword(SAM_SESymmetricKeyEntry keyEntry)
        {
            //On efface le MDP modifié une fois le changement effectué ou pas
            keyEntry.SAM_SEProperties!.Authenticate.Password = string.Empty;
            keyEntry.SAM_SEProperties!.Authenticate.PasswordConfirmation = string.Empty;
        }

        //Method to update DESFire Configuration depending on its N-1 enabled or not
        private void UpdateConfEnabled()
        {
            //We start at "True" cause the first configuration will always be active
            bool previousConfEnabled = true;
            foreach (SAM_SESymmetricKeyEntry keyEntry in _keyEntries)
            {
                //Getting only key that stats with "DF" --> DESFire object
                if (keyEntry.Identifier.Id!.StartsWith("DF"))
                {
                    //Storing information of enabled from the previous conf in the actual one
                    keyEntry.SAM_SEProperties!.DESFire.PreviousConfEnable = previousConfEnabled;
                    //Then, we get the information if this conf is active or not
                    previousConfEnabled = keyEntry.SAM_SEProperties!.DESFire.ReadingMode != SAM_SESymmetricKeyEntryPropertiesDESFire.SAM_SEDESFireMode.Disable;
                }
            }
        }

        //Method to regroup all the call of metadatas getters methods inside only method
        private void GetMetadataKeyEntry(SAM_SESymmetricKeyEntry keyEntry)
        {
            log.Info(String.Format("Getting key entry `{0}` metadatas...", keyEntry.Identifier.Id));
            SAM_SEDllObject? temp = SAM_SEDll.GetCurrentSAM_SE()!.GetKey(keyEntry.Identifier.Id!);
            if (temp is SAM_SEDllDESFire desfire)
            {
                keyEntry.SAM_SEProperties!.DESFire.ReadingMode = desfire.GetReadingMode();
                keyEntry.SAM_SEProperties!.DESFire.Msb = desfire.GetMsb();
                keyEntry.SAM_SEProperties!.DESFire.Ev0 = desfire.GetEv0();
                keyEntry.SAM_SEProperties!.DESFire.Ev1 = desfire.GetEv1();
                keyEntry.SAM_SEProperties!.DESFire.Ev2 = desfire.GetEv2();
                keyEntry.SAM_SEProperties!.DESFire.Ev3 = desfire.GetEv3();
                keyEntry.SAM_SEProperties!.DESFire.Jcop = desfire.GetJcop();
                keyEntry.SAM_SEProperties!.DESFire.AuthEv2 = desfire.GetAuthEv2();
                keyEntry.SAM_SEProperties!.DESFire.ProximityCheck = desfire.GetProximityCheck();
                if (keyEntry.SAM_SEProperties!.DESFire.ReadingMode == SAM_SESymmetricKeyEntryPropertiesDESFire.SAM_SEDESFireMode.IDP)
                {
                    keyEntry.SAM_SEProperties!.DESFire.Aid = desfire.GetAid();
                    keyEntry.SAM_SEProperties!.DESFire.KeyNum = desfire.GetKeyNumber();
                    keyEntry.SAM_SEProperties!.DESFire.FileNum = desfire.GetFileNumber();
                    keyEntry.SAM_SEProperties!.DESFire.Offset = desfire.GetOffset();
                    keyEntry.SAM_SEProperties!.DESFire.Size = desfire.GetSize();
                    keyEntry.SAM_SEProperties!.DESFire.EncryptType = desfire.GetEncryption();
                    keyEntry.SAM_SEProperties!.DESFire.Communication = desfire.GetCommunication();
                    keyEntry.SAM_SEProperties!.DESFire.Div.Enable = desfire.GetDivEnable();
                    if (keyEntry.SAM_SEProperties!.DESFire.Div.Enable == true)
                    {
                        keyEntry.SAM_SEProperties!.DESFire.Div.AidInverted = desfire.GetDivAidInv();
                        keyEntry.SAM_SEProperties!.DESFire.Div.KeyInc = desfire.GetDivKeyEnable();
                        keyEntry.SAM_SEProperties!.DESFire.Div.Si = desfire.GetDivSi();
                    }
                }
            }
            else if (temp is SAM_SEDllDESFireUid desfireUID)
            {
                keyEntry.SAM_SEProperties!.DESFireUID.Enable = desfireUID.GetUidActive();
                keyEntry.SAM_SEProperties!.DESFireUID.KeyNum = desfireUID.GetUidKeyNumber();
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
            //On retire les warnings, car on part du principe qu'ils ne seront plus là
            keyEntry.SAM_SEProperties!.DESFire.CommunicationWarning = false;
            keyEntry.SAM_SEProperties!.DESFire.EncryptTypeWarning = false;
            keyEntry.SAM_SEProperties!.DESFire.ReadingModeWarning = false;

            //Récupération de l'objet, de type objet car on ne sait pas encore son type
            SAM_SEDllObject? temp = SAM_SEDll.GetCurrentSAM_SE()!.GetKey(keyEntry.Identifier.Id!);
            if (temp is SAM_SEDllDESFire desfire)
            {
                if (keyEntry.SAM_SEProperties!.DESFire.Aid == null)
                    throw new KeyStoreException(Resources.AidNotValid);
                if (keyEntry.SAM_SEProperties!.DESFire.Div.SiString.Length %2 != 0)
                    throw new KeyStoreException(Resources.SiNotValid);
                desfire.SetMsb(keyEntry.SAM_SEProperties!.DESFire.Msb);
                desfire.SetEv0(keyEntry.SAM_SEProperties!.DESFire.Ev0);
                desfire.SetEv1(keyEntry.SAM_SEProperties!.DESFire.Ev1);
                desfire.SetEv2(keyEntry.SAM_SEProperties!.DESFire.Ev2);
                desfire.SetEv3(keyEntry.SAM_SEProperties!.DESFire.Ev3);
                desfire.SetJcop(keyEntry.SAM_SEProperties!.DESFire.Jcop);
                desfire.SetAuthEv2(keyEntry.SAM_SEProperties!.DESFire.AuthEv2);
                desfire.SetProximityCheck(keyEntry.SAM_SEProperties!.DESFire.ProximityCheck);
                desfire.SetAid(keyEntry.SAM_SEProperties!.DESFire.Aid!);
                desfire.SetKeyNumber(keyEntry.SAM_SEProperties!.DESFire.KeyNum);
                desfire.SetFileNumber(keyEntry.SAM_SEProperties!.DESFire.FileNum);
                desfire.SetOffset(keyEntry.SAM_SEProperties!.DESFire.Offset);
                desfire.SetSize(keyEntry.SAM_SEProperties!.DESFire.Size);
                desfire.SetEncryption(keyEntry.SAM_SEProperties!.DESFire.EncryptType);
                desfire.SetCommunication(keyEntry.SAM_SEProperties!.DESFire.Communication);
                desfire.SetReadingMode(keyEntry.SAM_SEProperties!.DESFire.ReadingMode);
                desfire.SetDivEnable(keyEntry.SAM_SEProperties!.DESFire.Div.Enable);
                if(keyEntry.SAM_SEProperties!.DESFire.Div.Enable)
                {
                    desfire.SetDivAidInv(keyEntry.SAM_SEProperties!.DESFire.Div.AidInverted);
                    desfire.SetDivSi(keyEntry.SAM_SEProperties!.DESFire.Div.Si, keyEntry.SAM_SEProperties!.DESFire.Div.KeyInc, keyEntry.SAM_SEProperties!.DESFire.KeyNum);
                }
            }
            else if (temp is SAM_SEDllDESFireUid desfireUID)
            {
                desfireUID.SetUidEnable(keyEntry.SAM_SEProperties!.DESFireUID.Enable);
                desfireUID.SetUidKeyNumber(keyEntry.SAM_SEProperties!.DESFireUID.KeyNum);
            }
            else
            {
                //Nothing to do
            }
            log.Info(String.Format("Key entry `{0}` metadatas updated.", keyEntry.Identifier.Id));
            return SAM_SEDll.GetCurrentSAM_SE()!.UploadMetadata();
        }

        //Method to regroup all the call of policies getters methods inside only method
        private void GetPolitics(SAM_SESymmetricKeyEntry keyEntry)
        {
            log.Info(String.Format("Getting key entry `{0}` policy...", keyEntry.Identifier.Id));
            SAM_SEDllObject? temp = SAM_SEDll.GetCurrentSAM_SE()!.GetKey(keyEntry.Identifier.Id!);
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
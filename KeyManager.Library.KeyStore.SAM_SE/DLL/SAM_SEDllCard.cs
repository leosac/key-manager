using System.Runtime.InteropServices;
using System.Text;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL
{
    public class SAM_SEDllCard
    {
        //Pointer to the SAM-SE context to be given to functions working on a SAM-SE
        private UIntPtr context;
        //Object used to manage DLL errors
        private SAM_SEDllErrorHandler errorHandler;
        //List of objects in the SAM-SE
        private readonly static List<SAM_SEDllObject> keys = [];
        //String with MAC of SAM-SE
        public string Mac;
        //String with type of SAM-SE
        public string Type;
        //String with version of SAM-SE
        public string Version;

        //Enum to list the differents lock level of SAM-SE
        //This enum comes from the DLL, do not modify without any knowledge of the DLL
        public enum SAM_SELockLevel
        {
            LOCK_LVL_KEYS = 0,      /*<! Minimal lock, only secrets keys are locked (only for Synchronic software not KML) */
            LOCK_LVL_FILE,          /*<! Intermediate lock, keys and files are locked for Synchronic Hardware */
            LOCK_LVL_READONLY,      /*<! Advanced lock, keys and files are locked for Synchronic Hardware, configuration file in read only mode for Synchronic software */
            //Add enum above this comment ; LOCK_LVL_NB_MAX is used as the maximum value not assignable
            LOCK_LVL_NB_MAX,
        }

        public SAM_SEDllCard(UIntPtr ctx, string mac, string type, string version, SAM_SEDllErrorHandler errorHandler)
        {
            context = ctx;
            Mac = mac;
            Type = type;
            Version = version;
            this.errorHandler = errorHandler;
        }

        //Method to set a context to the object, used for favourites
        public void SetContext(UIntPtr ctx)
        { 
            context = ctx;
        }

        //Method to set an error context to the object, used for favourites
        public void SetErrorHandler(SAM_SEDllErrorHandler errorHandler)
        {
            this.errorHandler = errorHandler;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_blinkLed(UIntPtr currentSAM_SE, uint state);
        public int BlinkLed(uint state)
        {
            int ret = spse_blinkLed(context, state);
            ret = errorHandler.HandlingError(ret);
            return ret;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_connectToSamSe(UIntPtr currentSAM_SE, string? password, uint pswdLen);
        public int ConnectToSAM_SE(string? password, uint pswdLen)
        {
            int ret = spse_connectToSamSe(context, password, pswdLen);
            ret = errorHandler.HandlingError(ret);
            BlinkLed(1);
            return ret;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_uploadMetadatas(UIntPtr currentSAM_SE);
        public int UploadMetadata()
        {
            int ret = spse_uploadMetadatas(context);
            ret = errorHandler.HandlingError(ret);
            BlinkLed(1);
            return ret;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void spse_getKeysList(ref IntPtr listKeys, ref uint nbKeys);
        public void GetKeysList(IList<KeyEntryId> keyId)
        {
            //Clean lsit of keys
            keyId.Clear();
            //Get data from DLL
            IntPtr listKeys = IntPtr.Zero;
            uint nbKeys = 0;
            spse_getKeysList(ref listKeys, ref nbKeys);
            int[] result = new int[nbKeys];
            Marshal.Copy(listKeys, result, 0, Convert.ToInt16(nbKeys));
            keys.Clear();
            //Here data is casted into an array of int, we get rid of the pointer
            foreach (uint ele in result.Select(v => (uint)v))
            {
                //Cast of int into string to have a friendly usable ID
                byte[] byteArray = BitConverter.GetBytes(ele);
                Array.Reverse(byteArray, 0, 4);
                string str = Encoding.ASCII.GetString(byteArray, 0, 4);
                //Adding label
                keyId.Add(new(str) { Label = AddKeyLabel(str) });
                //Check that the key doesn't already exists
                if (GetKey(str) == null)
                {
                    //If not, we create the key
                    CreateKeys(str, ele);
                    
                }
            }
            BlinkLed(1);
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void spse_freeSpse(UIntPtr currentSAM_SE);
        public void Deinit()
        {
            spse_freeSpse(context);
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint spse_getLockLvl(UIntPtr currentSAM_SE);
        public SAM_SELockLevel GetLockLevel()
        {
            SAM_SELockLevel ret = (SAM_SELockLevel)spse_getLockLvl(context);
            BlinkLed(1);
            return ret;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_setLockLvl(UIntPtr currentSAM_SE, uint lvl);
        public int SetLockLevel(SAM_SELockLevel lvl)
        {
            int ret = spse_setLockLvl(context, (uint)lvl);
            ret = errorHandler.HandlingError(ret);
            BlinkLed(1);
            return ret;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_writeDefFile(UIntPtr currentSAM_SE);
        public int SetDefaultConfigurationFile()
        {
            int ret = spse_writeDefFile(context);
            ret = errorHandler.HandlingError(ret);
            BlinkLed(1);
            return ret;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_downloadMetadatas(UIntPtr currentSAM_SE);
        public int DownloadMetadatas()
        {
            int ret = spse_downloadMetadatas(context);
            ret = errorHandler.HandlingError(ret);
            BlinkLed(1);
            return ret;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern byte spse_getKeyType(uint keyID);
        private SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType GetKeyType(uint id)
        {
            SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType ret = (SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType)spse_getKeyType(id);
            BlinkLed(1);
            return ret;
        }

        //Method used to format Label of each key depending on its ID
        private static string AddKeyLabel(string keyId)
        {
            if (keyId.Equals("SCP3"))
            {
                return Properties.Resources.LabelScp3;
            }
            else if (keyId.StartsWith("DF"))
            {
                return Properties.Resources.LabelDfxa + string.Format(" {0}", keyId.Substring(2, 1));
            }
            else if (keyId.StartsWith("UID"))
            {
                return Properties.Resources.LabelUidx + string.Format(" {0}", keyId.Substring(3, 1));
            }
            else if (keyId.StartsWith("BIO"))
            {
                return Properties.Resources.LabelBioa;
            }
            else
            {
                return Properties.Resources.LabelUnknown;
            }
        }

        //Method that create the right type of object depending on the informations from the DLL
        public void CreateKeys(string stringId, uint id)
        {
            SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType type = GetKeyType(id);
            SAM_SEDllObject temp;
            switch (type)
            {
                case SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.Authenticate:
                    temp = new SAM_SEDllAuthenticate(context, stringId, id, type, errorHandler);
                    break;
                case SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFire:
                    temp = new SAM_SEDllDESFire(context, stringId, id, type, errorHandler);
                    break;
                case SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFireUID:
                    temp = new SAM_SEDllDESFireUid(context, stringId, id, type, errorHandler);
                    break;
                default:
                case SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.Default:
                    temp = new(context, stringId, id, SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.Default, errorHandler);
                    break;
            }
            keys.Add(temp);
        }

        //Method that returns the key in the list if the id matches
        public SAM_SEDllObject? GetKey(string id)
        {
            return keys.FirstOrDefault(obj => obj.StringId == id);
        }

        //Method used to get a specific objet inside a SAM-SE from its id as uint
        public static SAM_SEDllObject? GetSAM_SEObject(uint id)
        {
            foreach (SAM_SEDllObject key in keys)
            {
                if (key.Id == id)
                    return key;
            }
            return null;
        }

        //Method used to get a specific objet inside a SAM-SE from its id as string
        public static SAM_SEDllObject? GetSAM_SEObject(string id)
        {
            foreach (SAM_SEDllObject key in keys)
            {
                if (key.StringId == id)
                    return key;
            }
            return null;
        }
    }
}

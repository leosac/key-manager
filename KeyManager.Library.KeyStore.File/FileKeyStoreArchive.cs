using System.IO.Compression;

namespace Leosac.KeyManager.Library.KeyStore.File
{
    public static class FileKeyStoreArchive
    {
        public static void Import(string fileName, FileKeyStore keyStore)
        {
            if (System.IO.File.Exists(fileName))
            {
                ZipFile.ExtractToDirectory(fileName, keyStore.GetFileProperties().Fullpath, true);
            }
        }

        public static void Export(string fileName, FileKeyStore keyStore)
        {
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }

            ZipFile.CreateFromDirectory(keyStore.GetFileProperties().Fullpath, fileName);
        }
    }
}

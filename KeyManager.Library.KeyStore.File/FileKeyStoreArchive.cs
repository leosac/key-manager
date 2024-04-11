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

            var filesToAdd = Directory.GetFiles(keyStore.GetFileProperties().Fullpath, "*.leok", SearchOption.TopDirectoryOnly);
            using var zipFileStream = new FileStream(fileName, FileMode.Create);
            using var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create);
            for (int i = 0; i < filesToAdd.Length; i++)
            {
                archive.CreateEntryFromFile(filesToAdd[i], Path.GetFileName(filesToAdd[i]));
            }
        }
    }
}

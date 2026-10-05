using Microsoft.Win32;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Leosac.KeyManager.Library.UI.Helpers
{
    public static class KeyFileDialogHelper
    {
        private const string FileFilter = "Binary Files (*.bin)|*.bin|Text Files (*.txt)|*.txt|PEM Files (*.pem)|*.pem|PKCS#12 Files (*.p12;*.pfx)|*.p12;*.pfx";
        private const string ImportFileFilter = FileFilter + "|PKCS#12 Files (*.p12;*.pfx)|*.p12;*.pfx";

        public static void Import(Action<byte[]> importBinary, Action<string, KeyValueStringFormat?> importText, Action<string?, string?> importPkcs12)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = ImportFileFilter
            };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            if (dialog.FilterIndex == 1)
            {
                importBinary(System.IO.File.ReadAllBytes(dialog.FileName));
            }
            else if (dialog.FilterIndex == 4)
            {
                var password = PasswordPrompt.Show("Password for the PKCS#12 file");
                if (password == null)
                {
                    return;
                }

                var (privateKey, publicKey) = ImportPkcs12(System.IO.File.ReadAllBytes(dialog.FileName), password);
                importPkcs12(privateKey, publicKey);
            }
            else
            {
                importText(System.IO.File.ReadAllText(dialog.FileName), dialog.FilterIndex == 3 ? KeyValueStringFormat.Pem : null);
            }
        }

        public static void Export(Func<byte[]> exportBinary, Func<KeyValueStringFormat?, string> exportText)
        {
            var dialog = new SaveFileDialog
            {
                Filter = FileFilter
            };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            if (dialog.FilterIndex == 1)
            {
                System.IO.File.WriteAllBytes(dialog.FileName, exportBinary());
            }
            else
            {
                System.IO.File.WriteAllText(dialog.FileName, exportText(dialog.FilterIndex == 3 ? KeyValueStringFormat.Pem : null));
            }
        }

        private static (string? PrivateKey, string? PublicKey) ImportPkcs12(byte[] data, string password)
        {
            var certificates = new X509Certificate2Collection();
            certificates.Import(data, password, X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
            var certificate = certificates.Cast<X509Certificate2>().FirstOrDefault(c => c.HasPrivateKey)
                ?? certificates.Cast<X509Certificate2>().FirstOrDefault()
                ?? throw new CryptographicException("The PKCS#12 file does not contain a certificate.");

            var privateKey = certificate.GetRSAPrivateKey();
            var publicKey = certificate.GetRSAPublicKey();
            if (publicKey != null)
            {
                using (publicKey)
                {
                    var privateKeyPem = privateKey?.ExportPkcs8PrivateKeyPem();
                    privateKey?.Dispose();
                    return (privateKeyPem, publicKey.ExportSubjectPublicKeyInfoPem());
                }
            }

            var privateEcdsa = certificate.GetECDsaPrivateKey();
            var publicEcdsa = certificate.GetECDsaPublicKey();
            if (publicEcdsa != null)
            {
                using (publicEcdsa)
                {
                    var privateKeyPem = privateEcdsa?.ExportPkcs8PrivateKeyPem();
                    privateEcdsa?.Dispose();
                    return (privateKeyPem, publicEcdsa.ExportSubjectPublicKeyInfoPem());
                }
            }

            throw new CryptographicException("The PKCS#12 certificate does not contain a supported public or private key.");
        }
    }
}

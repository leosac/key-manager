using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library
{
    public static class KCV
    {
        /*static public byte[] ComputeKCV(KeyHelper.KeyType keytype, byte[] key)
        {
            byte[] data, result ;
            byte[] iv = new byte[KeyHelper.GetIVSize(keytype)];            
            MemoryStream ms = new MemoryStream();
            CryptoStream cs;


            if (keytype == KeyHelper.KeyType.AES128 || keytype == KeyHelper.KeyType.AES192 || keytype == KeyHelper.KeyType.AES256)
            {
                data = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                RijndaelManaged rijndael = new RijndaelManaged();
                rijndael.Mode = CipherMode.ECB;

                ICryptoTransform aesEncryptor = rijndael.CreateEncryptor(key, iv);

                cs = new CryptoStream(ms, aesEncryptor, CryptoStreamMode.Write);
            }
            else if (keytype == KeyHelper.KeyType.T2KDES || keytype == KeyHelper.KeyType.T3KDES)
            {
                data = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };


                TripleDES tripleDESalg = TripleDES.Create();
                TripleDESCryptoServiceProvider provider = tripleDESalg as TripleDESCryptoServiceProvider;
                MethodInfo mi = provider.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance);
                provider.Mode = CipherMode.ECB;
                object[] Par = { key, provider.Mode, iv, provider.FeedbackSize, 0 };
                ICryptoTransform desEncryptor = mi.Invoke(provider, Par) as ICryptoTransform;

                cs = new CryptoStream(ms, desEncryptor, CryptoStreamMode.Write);
            }
            else if (keytype == KeyHelper.KeyType.DES)
            {
                data = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };

                DES DESalg = DES.Create();
                DESCryptoServiceProvider provider = DESalg as DESCryptoServiceProvider;
                MethodInfo mi = provider.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance);
                provider.Mode = CipherMode.ECB;
                object[] Par = { key, provider.Mode, iv, provider.FeedbackSize, 0 };
                ICryptoTransform desEncryptor = mi.Invoke(provider, Par) as ICryptoTransform;

                cs = new CryptoStream(ms, desEncryptor, CryptoStreamMode.Write);
            }
            else
                throw new Exception("No supported KeyType for KCV calcul.");

            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();

            result = ms.ToArray();

            ms.Close();
            cs.Close();

            Array.Resize(ref result, 3);
            return result;
        }*/
    }
}

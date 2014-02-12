using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ClientStream
{
    internal static class Security
    {
        private const string Key = "hPP7d6R4[53-4#-";
        private const string Salt = "w9=z4]0h";

        public static byte[] EncryptBytes(byte[] bytes)
        {
            var rdb = new Rfc2898DeriveBytes(Key, Encoding.UTF8.GetBytes(Salt));
            var ms = new MemoryStream();

            Aes aes = new AesManaged();
            aes.Key = rdb.GetBytes(aes.KeySize/8);
            aes.IV = rdb.GetBytes(aes.BlockSize/8);

            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(bytes, 0, bytes.Length);
                cs.FlushFinalBlock();
                cs.Close();
            }

            ms.Close();
            return ms.ToArray();
        }

        public static byte[] DecryptBytes(byte[] bytes, int length)
        {
            var rdb = new Rfc2898DeriveBytes(Key, Encoding.UTF8.GetBytes(Salt));
            var ms = new MemoryStream();

            Aes aes = new AesManaged();
            aes.Key = rdb.GetBytes(aes.KeySize/8);
            aes.IV = rdb.GetBytes(aes.BlockSize/8);

            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(bytes, 0, length);
                cs.FlushFinalBlock();
                cs.Close();
            }

            ms.Close();
            return ms.ToArray();
        }
    }
}
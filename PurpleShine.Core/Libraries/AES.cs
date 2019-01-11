using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PurpleShine.Core.Libraries
{

    public class GamingAes
    {
        private bool Enable { get; set; }
        /// <summary>
        /// 如果EncryptEnable是true 則啟用加/解密
        /// </summary>
        /// <param name="EncryptEnable"></param>
        public GamingAes(bool EncryptEnable)
        {
            Enable = EncryptEnable;
        }
        #region var
        private int KeySize = 256;
        private int BlockSize = 256;
        private int Iterations = 1;
        public string saltText = "i know what i am doing";
        //saltBytes 依照 saltText 做設定
        #endregion

        #region Encrypt
        public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            if (!Enable) return bytesToBeEncrypted;
            byte[] encryptedBytes = null;
            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[Encoding.Default.GetBytes(saltText).Length];
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = KeySize;
                    AES.BlockSize = BlockSize;
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, Iterations);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;
                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return encryptedBytes;
        }
        public string EncryptText(string input, string password)
        {
            if (!Enable) return input;
            // Get the bytes of the string
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);
            string result = Convert.ToBase64String(bytesEncrypted);
            return result;
        }
        #endregion

        #region Decrypt
        public byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            if (!Enable) return bytesToBeDecrypted;
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[Encoding.Default.GetBytes(saltText).Length];

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = KeySize;
                    AES.BlockSize = BlockSize;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, Iterations);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }
        public string Decrypt(string decryptedText, string pwd)
        {
            if (!Enable) return decryptedText;
            byte[] bytesToBeDecrypted = Convert.FromBase64String(decryptedText);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(pwd);
            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            byte[] decryptedBytes = AES_Decrypt(bytesToBeDecrypted, passwordBytes);
            // Getting the size of salt
            int _saltSize = 0;
            // Removing salt bytes, retrieving original bytes
            byte[] originalBytes = new byte[decryptedBytes.Length - _saltSize];
            for (int i = _saltSize; i < decryptedBytes.Length; i++)
            {
                originalBytes[i - _saltSize] = decryptedBytes[i];
            }
            return Encoding.UTF8.GetString(originalBytes);
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Security;
using System.Security.Cryptography;

namespace _1C
{

    public enum EnumVersion1C
    {
        NA=0,
        v7=1,
        v8=2,
        v81=3,
        v82=4
    }

    public enum EnumTypeConnection{
    	OLE=1,
    	COM=2
    }

    //[DataContract]
    [Serializable]
    public class BaseInfo
    {
      
    	/// <summary>
    	/// Имя настройки
    	/// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Путь к исполняемому файлу
        /// </summary>
        public string PathToExe { get; set; }

        /// <summary>
        /// Версия 1с: 7.7 или 8.х
        /// </summary>
        public EnumVersion1C Version1C { get; set; }
        
        /// <summary>
        /// Строка соединения
        /// </summary>
        public string ConnectionString { get; set; }

        public bool IsFileDB
        {
            get
            {
                return Regex.IsMatch(ConnectionString, "^File=.+", RegexOptions.Compiled);
            }
        }
        
        /// <summary>
        /// Строка соединения для пакетного режима
        /// </summary>
        public string ConnectionStringConsole
        {
            get
            {
                StringBuilder result = new StringBuilder();

                if (IsFileDB)
                {
                    Regex rx = new Regex("^File=\"(?<patch>.+)\"", RegexOptions.Compiled);
                    if (rx.IsMatch(ConnectionString))
                        result.Append(rx.Match(ConnectionString).Result("/F\"${patch}\""));
                }
                else
                {
                    Regex rx = new Regex("^Srvr=\"(?<server>.+)\";Ref=\"(?<base>.+)\";", RegexOptions.Compiled);
                    if (rx.IsMatch(ConnectionString))
                        result.Append(rx.Match(ConnectionString).Result("/S\"${server}\\${base}\""));
                }

                result.Append((User.Length == 0 ? @"" : @" /N""" + User + @""" "));
                result.Append((Password.Length == 0 ? @"" : @" /P""" + GetPassword() + @""" "));

                return result.ToString();
            }
        }

        /// <summary>
        /// Имя сервера SQL
        /// </summary>
        public string ServerName
        {
            get
            {
                string serverName = String.Empty;
                Regex rx = new Regex("Srvr=\"(?<server>[^\"]*)\"", RegexOptions.Compiled);
                if (rx.IsMatch(ConnectionString))
                    serverName = rx.Match(ConnectionString).Result("${server}");

                return serverName;
            }
        }

        /// <summary>
        /// Имя базы SQL
        /// </summary>
        public string BaseName
        {
            get
            {
                string baseName = String.Empty;
                Regex rx = new Regex("Ref=\"(?<base>[^\"]*)\"", RegexOptions.Compiled);
                if (rx.IsMatch(ConnectionString))
                    baseName = rx.Match(ConnectionString).Result("${base}");
                
                return baseName;
            }
        }

        public string PathToDB
        {
            get
            {
                string pathToDB = String.Empty;
                Regex rx = new Regex("^File=\"(?<patch>.+)\"", RegexOptions.Compiled);
                if(rx.IsMatch(ConnectionString))
                    pathToDB = rx.Match(ConnectionString).Result("${patch}");
                
                return pathToDB;
            }
        }
        /// <summary>
        /// Пользователь 
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public string Password { get; set;}

        public void SetPassword(string password)
        {
            //byte[] b = System.Text.ASCIIEncoding.ASCII.GetBytes(password);
            //string encryptedConnectionString = Convert.ToBase64String(b);
            //Password = encryptedConnectionString;
            Password = Crypto.EncryptStringAES(password, "78h342nb");
        }

        public string GetPassword()
        {
            //byte[] b = Convert.FromBase64String(Password);
            //string decryptedConnectionString = System.Text.ASCIIEncoding.ASCII.GetString(b);
            //return decryptedConnectionString;
            return Crypto.DecryptStringAES(Password, "78h342nb");
          }

        /// <summary>
        /// Только для 1с 8.х
        /// Тип доступа к 1с: OLE или COM
        /// </summary>
        public EnumTypeConnection TypeConnection { get; set; }
        

        public BaseInfo()
        {
            Version1C = EnumVersion1C.NA;
            TypeConnection = EnumTypeConnection.COM;
            User = "";
            Password = "";
            ConnectionString = "";
            Name = "";
        }


        
    }

    public class Crypto
    {
        private static byte[] _salt = Encoding.ASCII.GetBytes("o6806642kbM7c5");

        /// <summary>
        /// Encrypt the given string using AES.  The string can be decrypted using 
        /// DecryptStringAES().  The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        public static string EncryptStringAES(string plainText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            string outStr = null;                       // Encrypted string to return
            RijndaelManaged aesAlg = null;              // RijndaelManaged object used to encrypt the data.

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return outStr;
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using 
        /// EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        public static string DecryptStringAES(string cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String(cipherText);
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NamedPipeWrapper
{
    public class Crypto
    {
        //https://stackoverflow.com/questions/8041451/good-aes-initialization-vector-practice
        public static string EncryptString(string message, byte[] key)
        {
            var aes = new AesCryptoServiceProvider();
            var iv = aes.IV;

            using (var memStream = new System.IO.MemoryStream())
            {
                //memStream.Write(iv, 0, iv.Length);  // Add the IV to the first 16 bytes of the encrypted value
                using (var cryptStream = new CryptoStream(memStream, aes.CreateEncryptor(key, aes.IV), CryptoStreamMode.Write))
                {
                    using (var writer = new System.IO.StreamWriter(cryptStream))
                    {
                        writer.Write(message);
                    }
                }
                var buf = memStream.ToArray();
                
                //We describe the algorithm and provide the IV seperately for flexibility and transparency.
                return $"aes:{Convert.ToBase64String(iv)}:{Convert.ToBase64String(buf, 0, buf.Length)}";
            }
        }

        public static string DecryptString(string encryptedValue, byte[] key)
        {
            var splits = encryptedValue.Split(':');

            if (splits.Length != 3) throw new ArgumentException("Unknown format. Expected aes:[iv]:[bytes].", "encryptedValue");
            if (splits[0] != "aes") throw new ArgumentException("Algorithm not supported", "encryptedValue");

            var iv = Convert.FromBase64String(splits[1]);
            var bytes = Convert.FromBase64String(splits[2]);

            var aes = new AesCryptoServiceProvider();

            using (var memStream = new System.IO.MemoryStream(bytes))
            {
                //var iv = new byte[16];
                //memStream.Read(iv, 0, 16);  // Pull the IV from the first 16 bytes of the encrypted value 

                using (var cryptStream = new CryptoStream(memStream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                {
                    using (var reader = new System.IO.StreamReader(cryptStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}

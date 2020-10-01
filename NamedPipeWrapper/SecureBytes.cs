using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecOne.NamedPipeWrapper
{
    public static class SecureBytes
    {
        private static byte[] _additionalEntropy;

        //https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.protecteddata
        public static byte[] Protect(byte[] data)
        {
            return ProtectedData.Protect(data, GetEntropy(), DataProtectionScope.CurrentUser);
        }

        public static byte[] Unprotect(byte[] data)
        {
            return ProtectedData.Unprotect(data, GetEntropy(), DataProtectionScope.CurrentUser);
        }

        private static byte[] GetEntropy()
        {
            //Generate some entropy using a CSPRNG
            if (_additionalEntropy == null)
            {
                using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
                {
                    _additionalEntropy = new byte[5];
                    rng.GetBytes(_additionalEntropy);
                }
            }

            return _additionalEntropy;
        }

        public static string PrintValues(byte[] values)
        {
            var builder = new StringBuilder();

            foreach (Byte i in values)
            {
                builder.Append($"0x{i} ");
            }
            return builder.ToString();
        }


    }
}

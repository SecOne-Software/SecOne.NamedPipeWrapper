using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SecOne.NamedPipeWrapper
{
    //https://codereview.stackexchange.com/questions/107860/converting-a-securestring-to-a-byte-array
    public sealed class SecureStringWrapper : IDisposable
    {
        private readonly Encoding _encoding;
        private readonly SecureString _secureString;

        private byte[] _bytes = null;
        private bool _disposed = false;

        public SecureStringWrapper(SecureString secureString) : this(secureString, Encoding.UTF8)
        { }

        public SecureStringWrapper(SecureString secureString, Encoding encoding)
        {
            if (secureString == null) throw new ArgumentNullException(nameof(secureString));

            _encoding = encoding ?? Encoding.UTF8;
            _secureString = secureString;
        }

        public unsafe byte[] ToByteArray()
        {
            int maxLength = _encoding.GetMaxByteCount(_secureString.Length);

            IntPtr bytes = IntPtr.Zero;
            IntPtr str = IntPtr.Zero;

            try
            {
                bytes = Marshal.AllocHGlobal(maxLength);
                str = Marshal.SecureStringToBSTR(_secureString);

                char* chars = (char*)str.ToPointer();
                byte* bptr = (byte*)bytes.ToPointer();
                int len = _encoding.GetBytes(chars, _secureString.Length, bptr, maxLength);

                _bytes = new byte[len];
                for (int i = 0; i < len; ++i)
                {
                    _bytes[i] = *bptr;
                    bptr++;
                }

                return _bytes;
            }
            finally
            {
                if (bytes != IntPtr.Zero) Marshal.FreeHGlobal(bytes);
                if (str != IntPtr.Zero) Marshal.ZeroFreeBSTR(str);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Destroy();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        private void Destroy()
        {
            if (_bytes == null) return;
            Array.Clear(_bytes, 0, _bytes.Length);
            _bytes = null;
        }

        ~SecureStringWrapper()
        {
            Dispose();
        }
    }
}

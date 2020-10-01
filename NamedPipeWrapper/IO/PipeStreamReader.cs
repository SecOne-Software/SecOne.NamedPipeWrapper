using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace SecOne.NamedPipeWrapper.IO
{
    /// <summary>
    /// Wraps a <see cref="PipeStream"/> object and reads from it.  Deserializes binary data sent by a <see cref="PipeStreamWriter{T}"/>
    /// into a .NET CLR object specified by <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Reference type to deserialize data to</typeparam>
    public class PipeStreamReader<T> where T : class
    {
        /// <summary>
        /// Gets the underlying <c>PipeStream</c> object.
        /// </summary>
        public PipeStream BaseStream { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the pipe is connected or not.
        /// </summary>
        public bool IsConnected { get; private set; }

        public byte[] ProtectedEncryptionKey { get; set; }

        private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        /// <summary>
        /// Constructs a new <c>PipeStreamReader</c> object that reads data from the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Pipe to read from</param>
        public PipeStreamReader(PipeStream stream)
        {
            BaseStream = stream;
            IsConnected = stream.IsConnected;
        }

        #region Private stream readers

        /// <summary>
        /// Reads the length of the next message (in bytes) from the client.
        /// </summary>
        /// <returns>Number of bytes of data the client will be sending.</returns>
        /// <exception cref="InvalidOperationException">The pipe is disconnected, waiting to connect, or the handle has not been set.</exception>
        /// <exception cref="IOException">Any I/O error occurred.</exception>
        private int ReadLength()
        {
            const int lensize = sizeof (int);
            var lenbuf = new byte[lensize];
            var bytesRead = BaseStream.Read(lenbuf, 0, lensize);
            if (bytesRead == 0)
            {
                IsConnected = false;
                return 0;
            }
            if (bytesRead != lensize)
                throw new IOException(string.Format("Expected {0} bytes but read {1}", lensize, bytesRead));
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenbuf, 0));
        }

        /// <exception cref="SerializationException">An object in the graph of type parameter <typeparamref name="T"/> is not marked as serializable.</exception>
        private T ReadObject(int len)
        {
            var data = new byte[len];
            BaseStream.Read(data, 0, len);

            //Check if we have to decrypt this data first
            if (ProtectedEncryptionKey != null)
            {
                var key = SecureBytes.Unprotect(ProtectedEncryptionKey);
                
                try
                {
                    var encryptedData = DecryptBytes(data, key);
                    Array.Clear(data, 0, data.Length);

                    data = encryptedData;
                }
                finally
                {
                    Array.Clear(key, 0, key.Length);
                }
            }

            using (var memoryStream = new MemoryStream(data))
            {
                var result = (T) _binaryFormatter.Deserialize(memoryStream);
                Array.Clear(data, 0, data.Length);

                return result;
            }
        }

        private byte[] DecryptBytes(byte[] bytes, byte[] key)
        {
            using (var hashAlgorithm = new SHA256Managed())
            {
                //We derive a two keys, using sha256 hash, one Ke for encryption, the other Km for the mac
                var keyMaterialBytes = hashAlgorithm.ComputeHash(key);
                var ke = keyMaterialBytes.Take(16).ToArray();
                var km = keyMaterialBytes.Skip(16).ToArray();

                var mac = bytes.Skip(bytes.Length - 32).ToArray();
                var cypher = bytes.Take(bytes.Length - 32).ToArray();

                //Verify the mac
                var hash = new HMACSHA256(km);
                var verify = hash.ComputeHash(cypher);

                if (!verify.SequenceEqual(mac)) throw new Exception("MAC supplied does not match.");

                //Decrypt the remaining bytes
                //Note this uses BC block mode by default
                using (var aes = new AesCryptoServiceProvider())
                {
                    using (var outputStream = new MemoryStream())
                    {
                        using (var ms = new MemoryStream(cypher))
                        {
                            var iv = new byte[16];
                            ms.Read(iv, 0, 16);  // Pull the IV from the first 16 bytes of the encrypted value

                            using (var cs = new CryptoStream(outputStream, aes.CreateDecryptor(ke, iv), CryptoStreamMode.Write))
                            {
                                cs.Write(cypher, 16, cypher.Length - 16);
                                cs.Close();
                            }

                            return outputStream.ToArray();
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Reads the next object from the pipe.  This method blocks until an object is sent
        /// or the pipe is disconnected.
        /// </summary>
        /// <returns>The next object read from the pipe, or <c>null</c> if the pipe disconnected.</returns>
        /// <exception cref="SerializationException">An object in the graph of type parameter <typeparamref name="T"/> is not marked as serializable.</exception>
         public T ReadObject()
        {
            var len = ReadLength();
            return len == 0 ? default(T) : ReadObject(len);
        }
    }
}

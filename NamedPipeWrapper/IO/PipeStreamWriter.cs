using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace SecOne.NamedPipeWrapper.IO
{
    /// <summary>
    /// Wraps a <see cref="PipeStream"/> object and writes to it.  Serializes .NET CLR objects specified by <typeparamref name="T"/>
    /// into binary form and sends them over the named pipe for a <see cref="PipeStreamWriter{T}"/> to read and deserialize.
    /// </summary>
    /// <typeparam name="T">Reference type to serialize</typeparam>
    public class PipeStreamWriter<T> where T : class
    {
        /// <summary>
        /// Gets the underlying <c>PipeStream</c> object.
        /// </summary>
        public PipeStream BaseStream { get; private set; }

        public byte[] ProtectedEncryptionKey { get; set; }

        private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        /// <summary>
        /// Constructs a new <c>PipeStreamWriter</c> object that writes to given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Pipe to write to</param>
        public PipeStreamWriter(PipeStream stream)
        {
            BaseStream = stream;
        }

        #region Private stream writers

        /// <exception cref="SerializationException">An object in the graph of type parameter <typeparamref name="T"/> is not marked as serializable.</exception>
        private byte[] Serialize(T obj)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    _binaryFormatter.Serialize(memoryStream, obj);

                    //Check if we should return an encrypted array of data instead
                    if (ProtectedEncryptionKey != null)
                    {
                        var key = SecureBytes.Unprotect(ProtectedEncryptionKey);

                        try
                        {
                            return EncryptStream(memoryStream, key);
                        }
                        finally
                        {
                            Array.Clear(key, 0, key.Length);
                        }
                    }

                    return memoryStream.ToArray();
                }
            }
            catch
            {
                //if any exception in the serialize, it will stop named pipe wrapper, so there will ignore any exception.
                return null;
            }
        }

        private void WriteLength(int len)
        {
            var lenbuf = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(len));
            BaseStream.Write(lenbuf, 0, lenbuf.Length);
        }

        private void WriteObject(byte[] data)
        {
            BaseStream.Write(data, 0, data.Length);
        }

        private void Flush()
        {
            BaseStream.Flush();
        }

        private byte[] EncryptStream(MemoryStream message, byte[] key)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                var iv = aes.IV;

                using (var hashAlgorithm = new SHA256Managed())
                {
                    //We derive a two keys, using sha256 hash, one Ke for encryption, the other Km for the mac
                    var keyMaterialBytes = hashAlgorithm.ComputeHash(key);
                    var ke = keyMaterialBytes.Take(16).ToArray();
                    var km = keyMaterialBytes.Skip(16).ToArray();

                    //Encrypt the data
                    using (var ms = new MemoryStream())
                    {
                        ms.Write(iv, 0, iv.Length);  // Add the IV to the first 16 bytes of the encrypted value

                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(ke, aes.IV), CryptoStreamMode.Write))
                        {
                            var messageBytes = message.ToArray();

                            cs.Write(messageBytes, 0, messageBytes.Length);
                            cs.Close();

                            Array.Clear(messageBytes, 0, messageBytes.Length);
                        }

                        var cypher = ms.ToArray();
                        var hash = new HMACSHA256(km);
                        var mac = hash.ComputeHash(cypher);

                        //Append the MAC to the end of the array of bytes
                        byte[] output = new byte[cypher.Length + mac.Length];
                        Buffer.BlockCopy(cypher, 0, output, 0, cypher.Length);
                        Buffer.BlockCopy(mac, 0, output, cypher.Length, mac.Length);

                        return output;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Writes an object to the pipe.  This method blocks until all data is sent.
        /// </summary>
        /// <param name="obj">Object to write to the pipe</param>
        /// <exception cref="SerializationException">An object in the graph of type parameter <typeparamref name="T"/> is not marked as serializable.</exception>
        public void WriteObject(T obj)
        {
            var data = Serialize(obj);

            WriteLength(data.Length);
            WriteObject(data);
            Flush();

            //Now clear the data array
            Array.Clear(data, 0, data.Length);
        }

        /// <summary>
        ///     Waits for the other end of the pipe to read all sent bytes.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The pipe is closed.</exception>
        /// <exception cref="NotSupportedException">The pipe does not support write operations.</exception>
        /// <exception cref="IOException">The pipe is broken or another I/O error occurred.</exception>
        public void WaitForPipeDrain()
        {
            BaseStream.WaitForPipeDrain();
        }
    }
}
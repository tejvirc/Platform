namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    ///     A set of storage related utility methods
    /// </summary>
    public class StorageUtilities
    {
        /// <summary>
        ///     Helper method for storing a list in the persistent storage layer
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="list">The collection to store</param>
        /// <returns>A byte array</returns>
        public static byte[] ToByteArray<T>(IEnumerable<T> list)
        {
            if (list == null)
            {
                return null;
            }

            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
                formatter.Serialize(stream, list);
#pragma warning restore SYSLIB0011
                return stream.ToArray();
            }
        }

        /// <summary>
        ///     Gets a typed list from a byte array
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="data">A serialized list</param>
        /// <returns>The typed collection</returns>
        public static IEnumerable<T> GetListFromByteArray<T>(byte[] data)
        {
            if (data?.Length > 2)
            {
                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();

                    stream.Write(data, 0, data.Length);
                    stream.Position = 0;
#pragma warning disable SYSLIB0011
                    if (formatter.Deserialize(stream) is List<T> list)
                    {
                        return list;
                    }
#pragma warning restore SYSLIB0011
                }
            }

            return Enumerable.Empty<T>().ToList();
        }
    }
}

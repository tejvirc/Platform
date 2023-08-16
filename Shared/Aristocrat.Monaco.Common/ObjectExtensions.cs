namespace Aristocrat.Monaco.Common
{
    using System.IO;
    using ServiceStack.Text;

    /// <summary>
    ///     Extenstion methods for objects.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        ///     Performs a deep copy of an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepClone<T>(this T obj) where T : class
        {
            if (obj == null)
            {
                return default;
            }

            using (var memoryStream = new MemoryStream())
            {
                JsonSerializer.SerializeToStream(obj, memoryStream);
                memoryStream.Seek(0L, SeekOrigin.Begin);
                return JsonSerializer.DeserializeFromStream<T>(memoryStream);
            }
        }

        /// <summary>
        /// Returns true if the given objects are referentially equal, semantically equal, or both null.
        /// </summary>
        public static bool AreEqualOrNull(this object o1, object o2)
        {
            return o1?.Equals(o2) ?? o2 == null;
        }
    }
}

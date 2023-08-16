namespace Aristocrat.Monaco.Sas.Storage
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    ///     Helper functions for interacting with persistence storage
    /// </summary>
    public static class StorageHelpers
    {
        /// <summary>
        ///     Serializes an option for storage
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="data">The data to serialize</param>
        /// <returns>The serialized data</returns>
        public static string Serialize<T>(T data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings()
            {
                ReferenceLoopHandling= ReferenceLoopHandling.Ignore,
            });
        }

        /// <summary>
        ///     Deserializes the data from storage
        /// </summary>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <param name="data">The data from the persistence storage</param>
        /// <param name="defaultValue">The default value provider</param>
        /// <returns>The deserialized data</returns>
        public static T Deserialize<T>(string data, Func<T> defaultValue)
        {
            return string.IsNullOrEmpty(data) ? defaultValue.Invoke() : JsonConvert.DeserializeObject<T>(data);
        }
    }
}
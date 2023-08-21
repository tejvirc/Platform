namespace Aristocrat.Monaco.Protocol.Common.Logging
{
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    /// <summary>
    ///     Helper class to print the state of an object
    /// </summary>
    public static class ObjectPrinter
    {
#if RETAIL
        private const int MaxLoggingLength = 20480;
#else
        private const int MaxLoggingLength = 20480000;
#endif

        /// <summary>
        ///     Converts the object to Json representation
        /// </summary>
        /// <param name="obj"> The object </param>
        /// <param name="maxLength"> A maximum length for the output </param>
        /// <param name="formatting"> Formatting to be applied </param>
        public static string ToJson<T>(this T obj, int maxLength = MaxLoggingLength, Formatting formatting = Formatting.None)
            where T : class
        {
            if (obj == null) return "[NULL]";
            var jsonString = JsonConvert.SerializeObject(obj, formatting);
            if (jsonString.Length > maxLength) jsonString = jsonString.Substring(0, maxLength) + "...";
            return obj.GetType().Name + ":" + jsonString;
        }

        /// <summary>
        ///     Converts the struct object to Json representation
        /// </summary>
        /// <param name="obj"> The object </param>
        /// <param name="maxLength"> A maximum length for the output </param>
        /// <param name="formatting"> Formatting to be applied </param>
        public static string ToJson2<T>(this T obj, int maxLength = MaxLoggingLength, Formatting formatting = Formatting.None)
            where T : struct
        {
            var jsonString = JsonConvert.SerializeObject(obj, formatting);
            if (jsonString.Length > maxLength) jsonString = jsonString.Substring(0, maxLength) + "...";
            return obj.GetType().Name + ":" + jsonString;
        }
    }
}
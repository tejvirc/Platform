namespace Aristocrat.Monaco.Protocol.Common.Logging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    /// <summary>
    ///     Helper class to print the state of an object
    /// </summary>
    public static class ObjectPrinter
    {
        /// <summary>
        ///     Converts the object to Json representation
        /// </summary>
        /// <param name="obj"> The object </param>
        /// <param name="maxLength"> A maximum length for the output </param>
        /// <param name="formatting"> Formatting to be applied </param>
        public static string ToJson<T>(this T obj, int maxLength = 100000, Formatting formatting = Formatting.None)
            where T : class
        {
            if (obj == null) return "[NULL]";
            var jsonString = JsonConvert.SerializeObject(obj, formatting);
            if (jsonString.Length > maxLength) jsonString = jsonString.Substring(0, maxLength) + "...";
            return obj.GetType().Name + ":"+ jsonString;
        }

        /// <summary>
        ///     Converts the struct object to Json representation
        /// </summary>
        /// <param name="obj"> The object </param>
        /// <param name="maxLength"> A maximum length for the output </param>
        /// <param name="formatting"> Formatting to be applied </param>
        public static string ToJson2<T>(this T obj, int maxLength = 600, Formatting formatting = Formatting.None)
            where T : struct
        {
            var jsonString = JsonConvert.SerializeObject(obj, formatting);
            if (jsonString.Length > maxLength) jsonString = jsonString.Substring(0, maxLength) + "...";
            return obj.GetType().Name + ":" + jsonString;
        }

        /// <summary>
        ///     Converts the object to Xml representation
        /// </summary>
        /// <param name="obj"> The object </param>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static string ToXml<T>(this T obj)
            where T : class
        {
            if (obj == null)
            {
                return string.Empty;
            }

            try
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlSerializer.Serialize(writer, obj);
                    return  stringWriter.ToString();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
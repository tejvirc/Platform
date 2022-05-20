namespace Aristocrat.G2S.Emdi.Extensions
{
    using System;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Extension method helpers for XML
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Formats an XML string
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string FormatXml(this string source)
        {
            if (source == null)
                return null;

            var builder = new StringBuilder();

            try
            {
                var element = XElement.Parse(source);

                var settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = true,
                    NewLineOnAttributes = true
                };

                using (var xmlWriter = XmlWriter.Create(builder, settings))
                {
                    element.Save(xmlWriter);
                }
            }
            catch (Exception)
            {
                builder.Clear();
                builder.Append(source);
            }

            return builder.ToString();
        }
    }
}
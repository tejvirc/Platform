namespace Aristocrat.Monaco.Hardware.StorageSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using log4net;

    /// <summary>
    ///     BlockManager manages the persistent storage.  It is the arbitor and the
    ///     go between for users and storage.
    /// </summary>
    /// <remarks>
    ///     Currently persistent storage is setup as follows:
    ///     Bytes (0-3): Integer representing the number of bytes reserved before the start of blocks included these four
    ///     bytes,
    ///     right now that value would be 16. This is primarily of use to external tools that will no longer have
    ///     to be modified if additional "reserved" values are created.
    ///     Bytes (4-7) Persistent Storage Version Number (integer)
    ///     Bytes (8-11) Reserved Bytes CRC (unsigned integer)
    ///     <para>
    ///         After that starting at n (where n = integer from bytes 0-3) you have two blocks each with a header and each is
    ///         41
    ///         bytes in size.
    ///         These blocks are for external tools to use. After that, on a initial nvram clear the remaining will
    ///         be a solid free block used by other components at runtime.
    ///     </para>
    /// </remarks>
    public static class BlockManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly List<string> DefaultPath = new List<string>();
        private static readonly IList<string> SchemaPaths = new List<string>();

        /// <summary>
        ///     Gets the list of schema files.
        /// </summary>
        /// <remarks>
        ///     If directories other than the current directory are to be searched for schema files,
        ///     the caller needs to make calls to AddPath prior to requesting the list.
        /// </remarks>
        public static ReadOnlyCollection<string> SchemaFiles
        {
            get
            {
                if (SchemaPaths.Count == 0)
                {
                    SearchForSchemaFiles();
                }

                return new ReadOnlyCollection<string>(SchemaPaths);
            }
        }

        private static void SearchForSchemaFiles()
        {
            if (DefaultPath.Count == 0)
            {
                DefaultPath.Add(".");
            }

            foreach (var path in DefaultPath)
            {
                if (!Directory.Exists(path))
                {
                    continue;
                }

                var fileNames = Directory.GetFiles(path, "*.xml");
                var addinNames = Directory.GetFiles(path, "*.addin.xml");

                var xmlFilenames = fileNames.Except(addinNames);

                foreach (var fileName in xmlFilenames)
                {
                    try
                    {
                        using (var reader = new XmlTextReader(new StreamReader(fileName)))
                        {
                            var done = false;
                            while (reader.Read() && !done)
                            {
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        if (IsBlockFormat(reader.Name))
                                        {
                                            SchemaPaths.Add(fileName);
                                            done = true;
                                        }

                                        break;
                                    case XmlNodeType.Text:
                                        break;
                                    case XmlNodeType.EndElement:
                                        done = true;
                                        break;
                                }
                            }
                        }
                    }
                    catch (XmlException ex)
                    {
                        Logger.Error(ex.ToString());
                    }
                }
            }
        }

        private static bool IsBlockFormat(string s)
        {
            return s.Equals("BlockFormat", StringComparison.Ordinal) ||
                   s.Equals("ArrayOfBlockFormat", StringComparison.Ordinal) ||
                   s.Equals("BlockFormats", StringComparison.Ordinal);
        }
    }
}
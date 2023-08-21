namespace Aristocrat.Mgam.Client.Routing
{
    /// <summary>
    ///     Describes the format of the payload or content of the message.
    /// </summary>
    /// <remarks>
    ///     Used in the message header.
    /// </remarks>
    internal enum XmlDataSetFormat
    {
        /// <summary>Uncompressed XML payload.</summary>
        XmlDataSet,

        /// <summary>Compressed XML payload.</summary>
        XmlCompressedDataSet
    }
}

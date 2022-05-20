namespace Aristocrat.Sas.Client
{
    using System.Collections.Generic;

    /// <inheritdoc />
    public class SasEmptyMessage : ISasMessage
    {
        /// <inheritdoc />
        public IReadOnlyCollection<byte> MessageData => new List<byte>();
    }
}
namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using Monaco.Common.Storage;

    public class Host : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        ///   Gets or sets the HostName
        /// </summary>
        public string HostName { get; set; }
    }
}
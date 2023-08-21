namespace Aristocrat.Monaco.Hardware.Contracts.CardReader
{
    using System.Text;

    /// <summary>
    ///     The collection of data for each track on a magnetic card.
    /// </summary>
    public class TrackData
    {
        private const string Separator = "//";

        /// <summary>
        ///     Gets IdNumber.
        /// </summary>
        public string IdNumber => ToString();

        /// <summary>
        ///     Gets or sets Track1.
        /// </summary>
        public string Track1 { get; set; }

        /// <summary>
        ///     Gets or sets Track2.
        /// </summary>
        public string Track2 { get; set; }

        /// <summary>
        ///     Gets or sets Track2.
        /// </summary>
        public string Track3 { get; set; }

        /// <summary>
        ///     Converts the class to a string representing its data.
        /// </summary>
        /// <returns>A string representation of the class</returns>
        public override string ToString()
        {
            StringBuilder id = new StringBuilder();

            if (!string.IsNullOrEmpty(Track1))
            {
                id.Append(Track1);
            }

            if (!string.IsNullOrEmpty(Track2))
            {
                AppendSeparator();
                id.Append(Track2);
            }

            if (!string.IsNullOrEmpty(Track3))
            {
                AppendSeparator();
                id.Append(Track3);
            }

            return id.ToString();

            void AppendSeparator()
            {
                if (id.Length > 0)
                {
                    id.Append(Separator);
                }
            }
        }
    }
}

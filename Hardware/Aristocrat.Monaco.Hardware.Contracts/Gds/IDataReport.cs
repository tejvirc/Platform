namespace Aristocrat.Monaco.Hardware.Contracts.Gds
{
    /// <summary>Interface for data report.</summary>
    public interface IDataReport
    {
        /// <summary>Gets or sets the zero-based index of this report.</summary>
        /// <value>The index.</value>
        int Index { get; set; }

        /// <summary>Gets or sets the length.</summary>
        /// <value>The length.</value>
        int Length { get; set; }

        /// <summary>Gets or sets the data.</summary>
        /// <value>The data.</value>
        string Data { get; set; }
    }
}

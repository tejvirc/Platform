namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>Per port Sas host configuration</summary>
    public class SasHostConfiguration
    {
        /// <summary>Gets or sets the COM port for this host.</summary>
        public int ComPort { get; set; }

        /// <summary>Gets the port name for the host</summary>
        public string PortName => $"com{ComPort}";

        /// <summary>Gets or sets the account denom value for this host</summary>
        public long AccountingDenom { get; set; }

        /// <summary>Gets or sets the sas address used for this host</summary>
        public byte SasAddress { get; set; }

        /// <summary>Gets or sets the overflow behavior for the host</summary>
        public ExceptionOverflowBehavior OverflowBehavior { get; set; }
    }
}

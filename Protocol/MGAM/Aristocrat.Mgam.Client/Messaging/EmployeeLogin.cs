namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message must be sent after an Employee card (see GetCardType) is inserted to get
    ///     information about the employee.
    /// </summary>
    /// <remarks>
    ///     Implementing <see cref="IInstanceId"/> interface with tell <see cref="T:Aristocrat.Mgam.Client.Services.Identification.IdentificationService"/> to add the
    ///     InstanceId based on the registered VLT Service being targeted.
    /// </remarks>
    public class EmployeeLogin : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets or sets the CardString.
        /// </summary>
        public string CardString { get; set; }

        /// <summary>
        ///     Gets or sets the Pin.
        /// </summary>
        public string Pin { get; set; }
    }
}

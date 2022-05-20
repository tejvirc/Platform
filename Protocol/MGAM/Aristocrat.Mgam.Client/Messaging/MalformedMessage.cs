namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Indicates to the VLT that a message could not
    ///     be parsed or processed successfully.If any
    ///     information about the exact error could be
    ///     discerned, it will be returned in the parameter.
    /// </summary>
    public class MalformedMessage : Command
    {
        /// <summary>
        ///     Gets or sets the error description.
        /// </summary>
        public string ErrorDescription { get; set; }
    }
}

namespace Aristocrat.Monaco.Mgam.Services.SoftwareValidation
{
    /// <summary>
    ///     Used to Validate and update VLT software.
    /// </summary>
    public interface ISoftwareValidator
    {
        /// <summary>
        ///     Validate the software with the site server.
        /// </summary>
        void Validate();
    }
}

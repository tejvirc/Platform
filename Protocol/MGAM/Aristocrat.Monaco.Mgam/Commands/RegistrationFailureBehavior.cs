namespace Aristocrat.Monaco.Mgam.Commands
{
    /// <summary>
    ///     Instructions the registration service how to handle registration failures.
    /// </summary>
    public enum RegistrationFailureBehavior
    {
        /// <summary>Disconnect from VLT Service and relocate the service.</summary>
        Relocate,

        /// <summary>Lock and wait for employee to resolve the lock.</summary>
        Lock,
    }
}

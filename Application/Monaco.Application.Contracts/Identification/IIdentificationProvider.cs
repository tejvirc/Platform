namespace Aristocrat.Monaco.Application.Contracts.Identification
{
    using System;
    using Common;

    /// <summary>
    ///     An interface to interact with an identification provider
    /// </summary>
    [CLSCompliant(false)]
    public interface IIdentificationProvider : IHandlerConnector<IIdentificationValidator>
    {
    }
}

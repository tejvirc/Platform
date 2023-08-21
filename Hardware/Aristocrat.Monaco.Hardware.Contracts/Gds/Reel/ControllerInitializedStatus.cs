namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;

    /// <summary>
    ///     The controller initialized status
    /// </summary>
    [Serializable]
    public class ControllerInitializedStatus : GdsSerializableMessage
    {
        /// <summary>
        ///     Creates an instance of <see cref="ControllerInitializedStatus"/>
        /// </summary>
        public ControllerInitializedStatus()
            : base(GdsConstants.ReportId.ReelControllerInitialized)
        {
        }
    }
}

﻿namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to notify that the hard tilt condition related to the disk space has been removed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The system will go into the hard tilt condition when the cabinet is
    ///         disabled because of low disk space.
    ///     </para>
    /// </remarks>
    [Serializable]
    public class DiskSpaceClearEvent : BaseEvent
    {
    }
}
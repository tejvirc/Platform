namespace Aristocrat.Monaco.Hardware.EdgeLight.Contracts
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Event args holding physical strips for the simulated edge lighting
    /// </summary>
    public class StripsRenderedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StripsRenderedEventArgs" /> class.
        /// </summary>
        /// <param name="strips">The physical strips</param>
        public StripsRenderedEventArgs(IReadOnlyList<IStrip> strips)
        {
            Strips = strips;
        }

        /// <summary>
        ///     Gets the changed amount.
        /// </summary>
        public IReadOnlyList<IStrip> Strips { get; }
    }
}

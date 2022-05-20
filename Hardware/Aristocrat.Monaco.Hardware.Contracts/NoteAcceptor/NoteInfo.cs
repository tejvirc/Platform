namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;

    /// <summary>
    ///     Note info.
    /// </summary>
    [Serializable]
    public class NoteInfo
    {
        /// <summary>
        ///     A tuple for notes.
        /// </summary>
        public (int Denom, string IsoCode)[] Notes;
    }
}
namespace Aristocrat.Monaco.Hhr.Client.WorkFlow
{
    using System;

    /// <summary>
    ///     Interface to manage sequence id.
    /// </summary>
    public interface ISequenceIdManager
    {
        /// <summary>
        ///     Next Sequence Id
        /// </summary>
        uint NextSequenceId { get; }

        /// <summary>
        ///     Sequence Id observable
        /// </summary>
        IObservable<uint> SequenceIdObservable { get; }
    }
}
using static System.FormattableString;

namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using Kernel;

    /// <summary>
    ///     Class to handle fake note acceptor events
    /// </summary>
    public class FakeNoteAcceptorEvent : BaseEvent
    {
        /// <summary>BNA Id</summary>
        public int Id;

        /// Paper Jam
        public bool Jam;

        /// Cheat notice
        public bool Cheat;

        /// BNA path is clear
        public bool PathClear;

        /// Note removed
        public bool Removed;

        /// Note rejected
        public bool Rejected;

        /// Note returned
        public bool Returned;

        /// Note Accepted
        public bool Accepted;

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{GetType().Name} [Id={Id}]");
        }
    }
}
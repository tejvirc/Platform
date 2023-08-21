namespace Aristocrat.Monaco.Bingo
{
    using System;
    using Kernel;

    public class BingoDisableProvider : IBingoDisableProvider
    {
        private static readonly Guid BingoDisableGuid = new Guid("{6DFA1C82-FCAB-4B31-85FA-36D6BF216AEC}");

        private readonly ISystemDisableManager _systemDisableManager;

        /// <summary>
        ///     Creates the BingoDisableProvider Instance
        /// </summary>
        /// <param name="systemDisableManager">the system disable manager</param>
        public BingoDisableProvider(
            ISystemDisableManager systemDisableManager)
        {
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
        }

        public void Disable(string reason)
        {
            _systemDisableManager.Disable(BingoDisableGuid, SystemDisablePriority.Normal, () => reason);
        }

        public void Enable()
        {
            if (_systemDisableManager.IsDisabled)
            {
                _systemDisableManager.Enable(BingoDisableGuid);
            }
        }
    }
}
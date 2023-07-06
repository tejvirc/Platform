namespace Aristocrat.Monaco.Gaming
{
    using System;
    using Contracts;
    using Runtime;

    public class BalanceUpdateService : IBalanceUpdateService
    {
        private readonly IRuntime _runtime;
        private readonly IPlayerBank _bank;
        private readonly IMaxWinOverlayService _maxWinOverlayService;

        public BalanceUpdateService(IRuntime runtime, IPlayerBank bank, IMaxWinOverlayService maxWinOverlayService)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _maxWinOverlayService =
                maxWinOverlayService ?? throw new ArgumentNullException(nameof(maxWinOverlayService));
        }

        public void UpdateBalance()
        {
            if (!_maxWinOverlayService.ShowingMaxWinWarning)
            {
                _runtime.UpdateBalance(_bank.Credits);
            }
        }
    }
}
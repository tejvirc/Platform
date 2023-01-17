namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Contracts;
    using Kernel;

    /// <summary>
    ///     This class determines when we can do funds transfers based on the state of
    ///     the gaming machine.
    /// </summary>
    /// <remarks>
    ///     Transfers out are allowed only if we are not playing a game and there are
    ///     no tilts present that prevent a cashout from occurring.
    ///     Transfers on are allowed only if we are not playing a game or
    ///     the game has allowed for money to be inserted in a game round and
    ///     there are no tilts present.
    /// </remarks>
    public class FundsTransferDisable : IFundsTransferDisable, IService
    {
        private static readonly IReadOnlyList<Guid> AllowedDisableItems = new List<Guid>
        {
            ApplicationConstants.LiveAuthenticationDisableKey,
            GamingConstants.ReelsNeedHomingGuid
        };

        private readonly IGamePlayState _gameState;
        private readonly ISystemDisableManager _disableManager;

        public FundsTransferDisable(IGamePlayState gameState, ISystemDisableManager disableManager, IEventBus eventBus)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));

            if (eventBus == null) throw new ArgumentNullException(nameof(eventBus));

            eventBus.Subscribe<OverlayMenuEnteredEvent>(this, _ => TransferOnDisabledOverlay = true);
            eventBus.Subscribe<OverlayMenuExitedEvent>(this, _ =>
            {
                TransferOnDisabledOverlay = false;
                eventBus.Publish(new TransferEnableOnOverlayEvent());
            });
        }

        public bool TransferOnDisabledInGame => !IdleOrPresentationIdle;

        public bool TransferOnDisabledTilt => _disableManager.IsDisabled;

        public bool TransferOffDisabled => !IdleOrPresentationIdle ||
                                           _disableManager.CurrentImmediateDisableKeys.Except(AllowedDisableItems).Any();

        public bool TransferOnDisabledOverlay { get; private set; }

        public string Name => nameof(FundsTransferDisable);

        public ICollection<Type> ServiceTypes => new[] { typeof(IFundsTransferDisable) };

        private bool IdleOrPresentationIdle => _gameState.UncommittedState is PlayState.Idle or PlayState.PresentationIdle;

        public void Initialize()
        {
        }
    }
}
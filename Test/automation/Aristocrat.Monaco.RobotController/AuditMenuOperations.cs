﻿namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;

    internal sealed class AuditMenuOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly Automation _automator;
        private readonly RobotLogger _logger;
        private readonly StateChecker _sc;
        private readonly RobotController _robotController;
        private Timer _loadAuditMenuTimer;
        private Timer _exitAuditMenuTimer;
        private bool _disposed;

        public AuditMenuOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, RobotController robotController)
        {
            _sc = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _robotController = robotController;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_loadAuditMenuTimer != null)
            {
                _loadAuditMenuTimer.Dispose();
                _loadAuditMenuTimer = null;
            }

            if (_exitAuditMenuTimer != null)
            {
                _exitAuditMenuTimer.Dispose();
                _exitAuditMenuTimer = null;
            }

            _eventBus.UnsubscribeAll(this);
            _disposed = true;
        }

        public void Reset()
        {
            _automator.ExitAuditMenu();
            _robotController.UnBlockOtherOperations(RobotStateAndOperations.AuditMenuOperation);
        }

        public void Execute()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AuditMenuOperations));
            }

            _logger.Info("AuditMenuOperations Has Been Initiated!", GetType().Name);
            SubscribeToEvents();
            _loadAuditMenuTimer = new Timer(
                _ => RequestAuditMenu(),
                null,
                _robotController.Config.Active.IntervalLoadAuditMenu,
                _robotController.Config.Active.IntervalLoadAuditMenu);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _eventBus.UnsubscribeAll(this);

            _loadAuditMenuTimer?.Halt();

            _automator.ExitAuditMenu();
        }

        private void RequestAuditMenu()
        {
            if (!IsValid())
            {
                return;
            }

            _logger.Info("Requesting Audit Menu", GetType().Name);
            _automator.LoadAuditMenu();

            _exitAuditMenuTimer = new Timer(
                _ =>
                {
                    _automator.ExitAuditMenu();
                    _exitAuditMenuTimer.Dispose();
                },
                null,
                Constants.AuditMenuDuration,
                Timeout.Infinite);
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(
                this,
                _ =>
                {
                    _robotController.BlockOtherOperations(RobotStateAndOperations.AuditMenuOperation);
                    _logger.Info($"OperatorMenuEnteredEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                });

            _eventBus.Subscribe<OperatorMenuExitedEvent>(
                this,
                _ =>
                {
                    _robotController.UnBlockOtherOperations(RobotStateAndOperations.AuditMenuOperation);
                    _logger.Info($"OperatorMenuExitedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                });
        }

        private bool IsValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            return !isBlocked && _sc.AuditMenuOperationValid;
        }
    }
}

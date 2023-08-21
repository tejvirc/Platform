using Aristocrat.Monaco.Application.UI.OperatorMenu;
using Aristocrat.Monaco.Gaming.Contracts;
using Aristocrat.Monaco.Kernel;
using Vgt.Client12.Application.OperatorMenu;

namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    public abstract class OperatorMenuDiagnosticPageViewModelBase : OperatorMenuPageViewModelBase
    {
        private readonly IOperatorMenuLauncher _operatorMenuLauncher;

        protected OperatorMenuDiagnosticPageViewModelBase(bool defaultPrintButtonEnabled = false) : base(defaultPrintButtonEnabled)
        {
            _operatorMenuLauncher = ServiceManager.GetInstance().GetService<IOperatorMenuLauncher>();
        }

        protected void PreventOperatorMenuExit()
        {
            _operatorMenuLauncher.PreventExit();
        }

        protected virtual void GameDiagnosticsComplete()
        {    
        }

        protected override void OnLoaded()
        {
            EventBus.Subscribe<GameDiagnosticsCompletedEvent>(this, HandleEvent);
        }

        private void HandleEvent(GameDiagnosticsCompletedEvent @event)
        {
            GameDiagnosticsComplete();
            _operatorMenuLauncher.AllowExit();
        }

        protected override void OnUnloaded()
        {
            // need to do this in case menu is exited mid-diagnostics.
            _operatorMenuLauncher.AllowExit();
        }
    }
}

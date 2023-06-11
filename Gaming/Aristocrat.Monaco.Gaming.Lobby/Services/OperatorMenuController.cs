namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using Aristocrat.Monaco.Application.Contracts;
using Vgt.Client12.Application.OperatorMenu;

public class OperatorMenuController : IOperatorMenuController
{
    private readonly IOperatorMenuLauncher _operatorMenuLauncher;

    public OperatorMenuController(IOperatorMenuLauncher operatorMenuLauncher)
    {
        _operatorMenuLauncher = operatorMenuLauncher;
    }

    public void Enable()
    {
        _operatorMenuLauncher.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);
    }
}

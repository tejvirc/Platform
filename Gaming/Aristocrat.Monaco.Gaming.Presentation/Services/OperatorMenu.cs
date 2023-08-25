﻿namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using Aristocrat.Monaco.Application.Contracts;
using Contracts;
using Vgt.Client12.Application.OperatorMenu;

public class OperatorMenu : IOperatorMenu
{
    private readonly IOperatorMenuLauncher _operatorMenuLauncher;

    public OperatorMenu(IOperatorMenuLauncher operatorMenuLauncher)
    {
        _operatorMenuLauncher = operatorMenuLauncher;
    }

    public void Initialize()
    {
        _operatorMenuLauncher.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);
    }

    public void Disable()
    {
        _operatorMenuLauncher.DisableKey(GamingConstants.OperatorMenuDisableKey);
    }

    public void Enable()
    {
        _operatorMenuLauncher.EnableKey(GamingConstants.OperatorMenuDisableKey);
    }
}

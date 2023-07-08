﻿namespace Aristocrat.Extensions.Fluxor;

using System;
using global::Fluxor.DependencyInjection;
using global::Fluxor.Extensions;
using Microsoft.Extensions.DependencyInjection;

public static class FluxorOptionsExtensions
{
    /// <summary>
    /// Enables interaction with remote Redux Dev Tools over a SignalR connection
    /// </summary>
    /// <param name="options">The current options</param>
    /// <param name="updateReduxOptions">An action to update the options</param>
    /// <returns></returns>
    public static FluxorOptions UseRemoteReduxDevTools(
        this FluxorOptions options,
        Action<RemoteReduxDevToolsMiddlewareOptions>? updateReduxOptions = null)
    {
        var reduxOptions = new RemoteReduxDevToolsMiddlewareOptions(options);
        updateReduxOptions?.Invoke(reduxOptions);

        options.AddMiddleware<RemoteReduxDevToolsMiddleware>();
        options.Services.Add(_ => reduxOptions, options);

        return options;
    }

    public static FluxorOptions UseSelectors(this FluxorOptions options)
    {
        options.Services
            .AddSingleton<ISelector, StoreSelector>()
            .AddSingleton(typeof(IStateSelector<>), typeof(StateSelector<>));

        return options;
    }
}

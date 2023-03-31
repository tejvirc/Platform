namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System;
using System.Windows;
using System.Windows.Controls;

internal interface IViewCollection
{
    void RegisterViewType<TView>(string viewName) where TView : UserControl;

    void RegisterWindowViewType<TWindow>(string viewName) where TWindow : Window;

    Type GetViewType(string viewName);

    Type GetWindowViewType(string viewName);
}

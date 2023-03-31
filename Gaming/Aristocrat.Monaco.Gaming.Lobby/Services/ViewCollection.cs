namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

public class ViewCollection : IViewCollection
{
    private readonly Dictionary<string, Type> _views = new();
    private readonly Dictionary<string, Type> _windows = new();

    public void RegisterViewType<TView>(string viewName) where TView : UserControl
    {
        if (_views.ContainsKey(viewName))
        {
            throw new ArgumentException($@"View with key {viewName} is already registered", nameof(viewName));
        }

        _views.Add(viewName, typeof(TView));
    }

    public void RegisterWindowViewType<TWindow>(string viewName) where TWindow : Window
    {
        if (_windows.ContainsKey(viewName))
        {
            throw new ArgumentException($@"View with key {viewName} is already registered", nameof(viewName));
        }

        _windows.Add(viewName, typeof(TWindow));
    }

    public Type GetViewType(string viewName)
    {
        if (!_views.TryGetValue(viewName, out var view))
        {
            throw new ArgumentException($@"View with key {viewName} not found", nameof(viewName));
        }

        return view;
    }

    public Type GetWindowViewType(string viewName)
    {
        if (!_windows.TryGetValue(viewName, out var view))
        {
            throw new ArgumentException($@"View with key {viewName} not found", nameof(viewName));
        }

        return view;
    }
}

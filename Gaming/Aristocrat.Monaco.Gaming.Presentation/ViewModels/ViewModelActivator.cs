namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Windows.Threading;

public class ViewModelActivator
{
    private readonly CompositeDisposable _disposables = new();
    private readonly List<Action<ICollection<IDisposable>>> _blocks = new();

    public ViewModelActivator()
    {
        Dispatcher.CurrentDispatcher.ShutdownStarted += OnShutdownStarted;
    }

    private void OnShutdownStarted(object? sender, EventArgs e)
    {
        _disposables.Dispose();
    }

    public void Activate(Action<ICollection<IDisposable>> block)
    {
        if (_disposables.IsDisposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }

        _blocks.Add(block);

        block(_disposables);
    }
}

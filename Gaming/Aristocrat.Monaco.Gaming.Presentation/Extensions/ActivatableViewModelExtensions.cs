namespace Aristocrat.Monaco.Gaming.Presentation;

using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using ViewModels;

public static class ActivatableViewModelExtensions
{
    public static void WhenActivated(this IActivatableViewModel viewModel, Action<ICollection<IDisposable>> block)
    {
        viewModel.Activator.Activate(block);
    }

    public static T DisposeWith<T>(this T disposable, ICollection<IDisposable> disposables)
        where T : IDisposable
    {
        if (disposables is null)
        {
            throw new ArgumentNullException(nameof(disposables));
        }

        disposables.Add(disposable);

        return disposable;
    }
}

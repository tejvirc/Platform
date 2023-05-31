namespace Aristocrat.Monaco.Gaming.Lobby.Redux;

using System;

public interface ISelector<TState, out TResult> : IObservable<TResult> where TState : class
{
}

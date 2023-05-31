namespace Aristocrat.Monaco.Gaming.Lobby.Redux;

using System;

[AttributeUsage(AttributeTargets.Class)]
public class SelectorsAttribute : Attribute
{
    public SelectorsAttribute(Type stateType)
    {
        StateType = stateType;
    }

    public Type StateType { get; }
}

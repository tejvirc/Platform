namespace Aristocrat.Monaco.Gaming.Commands.RuntimeEvents
{
    public interface IRuntimeEventHandler
    {
        void HandleEvent(GameRoundEvent gameRoundEvent);
    }
}

namespace Aristocrat.Monaco.Test.KeyConverter
{
    public class ToggleInputEvent
    {
        public ToggleInputEvent(int id, bool springLoaded)
        {
            Id = id;
            SpringLoaded = springLoaded;
        }

        public int Id { get; }

        public bool SpringLoaded { get; }
    }
}
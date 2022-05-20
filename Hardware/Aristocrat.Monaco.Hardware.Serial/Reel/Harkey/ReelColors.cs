namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey
{
    public class ReelColors
    {
        public ReelColors()
        {
            TopColor = 0;
            MiddleColor = 0;
            BottomColor = 0;
        }

        public ReelColors(int top, int middle, int bottom)
        {
            TopColor = top;
            MiddleColor = middle;
            BottomColor = bottom;
        }

        public int TopColor { get; }

        public int MiddleColor { get; }

        public int BottomColor { get; }
    };
}

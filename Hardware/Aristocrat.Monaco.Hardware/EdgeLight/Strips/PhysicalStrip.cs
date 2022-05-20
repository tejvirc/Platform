namespace Aristocrat.Monaco.Hardware.EdgeLight.Strips
{
    using System;
    using Contracts;

    public class PhysicalStrip : IStrip
    {
        private int _brightness;

        public PhysicalStrip(
            int stripId = 0,
            int ledCount = 0,
            BoardIds boardId = BoardIds.InvalidBoardId,
            byte brightness = 100)
        {
            StripId = stripId;
            LedCount = ledCount;
            Brightness = brightness;
            ColorBuffer = new LedColorBuffer(ledCount);
            BoardId = boardId;
        }

        public BoardIds BoardId { get; }

        public int StripId { get; }

        public int Brightness
        {
            get => _brightness;
            set
            {
                if (value == _brightness)
                {
                    return;
                }

                _brightness = value;
                BrightnessChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int LedCount { get; set; }
        public LedColorBuffer ColorBuffer { get; }

        public void SetColors(LedColorBuffer colorBuffer, int sourceColorIndex, int ledCount, int destinationLedIndex)
        {
            ColorBuffer.SetColors(colorBuffer, sourceColorIndex, ledCount, destinationLedIndex);
        }

        public event EventHandler<EventArgs> BrightnessChanged;
    }
}
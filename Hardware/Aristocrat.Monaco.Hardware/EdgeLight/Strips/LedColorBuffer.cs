namespace Aristocrat.Monaco.Hardware.EdgeLight.Strips
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    /// <summary>
    ///     A utility class to encapsulate Led color buffer.
    /// </summary>
    public class LedColorBuffer : IEnumerable<Color>
    {
        public const int BytesPerLed = 4;

        private byte[] _argbBytes;

        /// <summary>
        ///     Initializes color buffer with given number of led count.
        /// </summary>
        /// <param name="ledCount">Number of led this buffer need to hold.</param>
        public LedColorBuffer(int ledCount = 0)
        {
            _argbBytes = new byte[ledCount * BytesPerLed];
        }

        /// <summary>
        ///     Initializes color buffer with given led colors. Led count is set to the number of colors provided.
        /// </summary>
        /// <param name="colors">An enumeration of colors.</param>
        public LedColorBuffer(IEnumerable<Color> colors)
        {
            _argbBytes = colors.SelectMany(x => new[] { x.A, x.R, x.G, x.B }).ToArray();
        }

        /// <summary>
        ///     Initializes color buffer with given ARGB color data. Led count is set to the number bytes / 4.
        /// </summary>
        /// <param name="argbBytes">Enumeration of ARGB color buffer data. Must be divisible by four.</param>
        public LedColorBuffer(IEnumerable<byte> argbBytes)
        {
            var argbArray = argbBytes as byte[] ?? argbBytes.ToArray();
            if (argbArray.Length % BytesPerLed != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(argbBytes));
            }

            _argbBytes = argbArray;
        }

        public byte[] ArgbBytes => _argbBytes;

        public byte[] RgbBytes => _argbBytes.Where((_, i) => i % 4 != 0).ToArray();

        public int Count => ArgbBytes.Length / BytesPerLed;

        public Color this[int index]
        {
            get => Color.FromArgb(
                _argbBytes[index * BytesPerLed],
                _argbBytes[index * BytesPerLed + 1],
                _argbBytes[index * BytesPerLed + 2],
                _argbBytes[index * BytesPerLed + 3]);
            set
            {
                _argbBytes[index * BytesPerLed] = value.A;
                _argbBytes[index * BytesPerLed + 1] = value.R;
                _argbBytes[index * BytesPerLed + 2] = value.G;
                _argbBytes[index * BytesPerLed + 3] = value.B;
            }
        }

        public IEnumerator<Color> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Returns the color buffer segment as another LedColorBuffer.
        /// </summary>
        /// <param name="sourceLedIndex">Index of led from which to create new segment.</param>
        /// <param name="ledCount"></param>
        /// <param name="reversed">Should the segment be reversed.</param>
        /// <returns></returns>
        public LedColorBuffer GetSegment(int sourceLedIndex, int ledCount, bool reversed = false)
        {
            CheckBounds(this, sourceLedIndex, ledCount);

            var segmentColors = this.Skip(sourceLedIndex).Take(ledCount);
            if (reversed)
            {
                segmentColors = segmentColors.Reverse();
            }

            var segmentBuffer = new LedColorBuffer(segmentColors);
            return segmentBuffer;
        }

        /// <summary>
        ///     Appends another color buffer. This operation increases the led count of the buffer.
        /// </summary>
        /// <param name="ledSegmentColorBuffer">Buffer to append.</param>
        /// <returns></returns>
        public LedColorBuffer Append(LedColorBuffer ledSegmentColorBuffer)
        {
            var beforeSize = _argbBytes.Length;
            Array.Resize(ref _argbBytes, beforeSize + ledSegmentColorBuffer.ArgbBytes.Length);
            Array.Copy(
                ledSegmentColorBuffer.ArgbBytes,
                0,
                _argbBytes,
                beforeSize,
                ledSegmentColorBuffer.ArgbBytes.Length);
            return this;
        }

        /// <summary>
        ///     Sets buffer color from another color buffer.
        /// </summary>
        /// <param name="colorBuffer"></param>
        /// <param name="sourceColorIndex"></param>
        /// <param name="count"></param>
        /// <param name="destinationColorIndex"></param>
        public void SetColors(LedColorBuffer colorBuffer, int sourceColorIndex, int count, int destinationColorIndex)
        {
            CheckBounds(colorBuffer, sourceColorIndex, count);
            CheckBounds(this, destinationColorIndex, count);
            Array.Copy(
                colorBuffer._argbBytes,
                sourceColorIndex * BytesPerLed,
                _argbBytes,
                destinationColorIndex * BytesPerLed,
                count * BytesPerLed);
        }

        /// <summary>
        ///     Sets the color from BGRA data.
        /// </summary>
        /// <param name="setLedData"></param>
        public void SetBgraData(byte[] setLedData)
        {
            if (setLedData.Length % BytesPerLed != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(setLedData));
            }

            var count = Math.Min(_argbBytes.Length, setLedData.Length);
            for (var i = 0; i < count; i += 4)
            {
                _argbBytes[i] = setLedData[i + 3];
                _argbBytes[i + 1] = setLedData[i + 2];
                _argbBytes[i + 2] = setLedData[i + 1];
                _argbBytes[i + 3] = setLedData[i];
            }
        }

        /// <summary>
        ///     Sets a specified number of LED to a single given color.
        /// </summary>
        /// <param name="destinationLedIndex"></param>
        /// <param name="color"></param>
        /// <param name="ledCount"></param>
        public void SetColor(int destinationLedIndex, Color color, int ledCount)
        {
            CheckBounds(this, destinationLedIndex, ledCount);
            var count = Math.Min(ledCount, Count);
            for (var i = 0; i < count; i++)
            {
                this[i + destinationLedIndex] = color;
            }
        }

        /// <summary>
        ///     Overlays new color buffer on top of itself based on alpha. For alpha only two values (fully opaque or fully
        ///     transparent) are supported.
        /// </summary>
        /// <param name="ledColorBuffer"></param>
        public void Draw(LedColorBuffer ledColorBuffer)
        {
            if (Count != ledColorBuffer.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(ledColorBuffer));
            }

            for (var i = 0; i < Count; i++)
            {
                this[i] = Blend(this[i], ledColorBuffer[i]);
            }
        }

        public void DrawScaled(
            LedColorBuffer sourceData,
            int sourceLedStart,
            int sourceLedCount,
            int destinationLedStart,
            int destinationLedCount)
        {
            if (sourceData == null || sourceLedStart + sourceLedCount > sourceData.Count || sourceLedCount <= 0 ||
                sourceLedStart < 0)
            {
                throw new ArgumentException(nameof(sourceData));
            }

            if (destinationLedStart + destinationLedCount > Count || destinationLedCount <= 0)
            {
                throw new ArgumentException(nameof(ArgbBytes));
            }

            var scalingFactor = (double)sourceLedCount / destinationLedCount;

            for (var i = 0; i < destinationLedCount; i++)
            {
                var index = sourceLedStart +
                            (sourceLedCount != destinationLedCount ? (int)Math.Floor(i * scalingFactor) : i);
                this[destinationLedStart + i] = sourceData[index];
            }
        }

        private static Color Blend(Color backgroundColor, Color foreground)
        {
            return foreground.A > 0 ? foreground : backgroundColor;
        }

        private static void CheckBounds(LedColorBuffer buffer, int index, int count)
        {
            if (index < 0 || index > buffer.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0 || count + index > buffer.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }
    }
}
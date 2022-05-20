namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using System;

    /// <summary>
    ///     A set of extension methods for the mechanical reels />
    /// </summary>
    public static class ReelExtensions
    {
        /// <summary>
        ///     Gets a stop position for a specified step position on a reel.
        /// </summary>
        /// <param name="step">A step position on a reel</param>
        /// <param name="numberOfSteps">The maximum number of steps on a reel</param>
        /// <param name="numberOfStops">The maximum number of stops on a reel</param>
        /// <returns>A stop position on a reel</returns>
        public static int GetStopFromStep(int step, int numberOfSteps, int numberOfStops)
        {
            if (step < 0 || step > numberOfSteps)
            {
                throw new ArgumentException(nameof(step));
            }

            if (numberOfStops < 1)
            {
                throw new ArgumentException(nameof(numberOfStops));
            }

            var numStepsPerStop = (double)numberOfSteps / numberOfStops;
            var stop = (int)(step / numStepsPerStop) + 1;

            return stop;
        }
    }
}

namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// TouchDeviceEmulator
    /// </summary>
    public class TouchDeviceEmulator : TouchDevice
    {
        /// <summary>
        ///     The Hardware source pointer
        /// </summary>
        public IntPtr HSource { get; }

        /// <summary>
        /// Position
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <param name="hSource">The Hardware source pointer</param>
        public TouchDeviceEmulator(int deviceId, IntPtr hSource)
            : base(deviceId)
        {
            HSource = hSource;
        }

        /// <summary>
        /// GetTouchPoint
        /// </summary>
        /// <param name="relativeTo"></param>
        /// <returns></returns>
        public override TouchPoint GetTouchPoint(IInputElement relativeTo)
        {
            var pt = Position;
            if (relativeTo != null)
            {
                pt = ActiveSource?.RootVisual?.TransformToDescendant((Visual)relativeTo).Transform(Position) ?? Position;
            }

            var rect = new Rect(pt, new Size(1.0, 1.0));
            return new TouchPoint(this, pt, rect, TouchAction.Move);
        }

        /// <summary>
        /// GetIntermediateTouchPoints
        /// </summary>
        /// <param name="relativeTo"></param>
        /// <returns></returns>
        public override TouchPointCollection GetIntermediateTouchPoints(IInputElement relativeTo)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// SetActiveSource
        /// </summary>
        /// <param name="activeSource"></param>
        public new void SetActiveSource(PresentationSource activeSource)
        {
            base.SetActiveSource(activeSource);
        }

        /// <summary>
        /// Activate
        /// </summary>
        public new void Activate()
        {
            base.Activate();
        }

        /// <summary>
        /// ReportUp
        /// </summary>
        public new void ReportUp()
        {
            base.ReportUp();
        }

        /// <summary>
        /// ReportDown
        /// </summary>
        public new void ReportDown()
        {
            base.ReportDown();
        }

        /// <summary>
        /// ReportMove
        /// </summary>
        public new void ReportMove()
        {
            base.ReportMove();
        }

        /// <summary>
        /// Deactivate
        /// </summary>
        public new void Deactivate()
        {
            base.Deactivate();
        }
    }
}
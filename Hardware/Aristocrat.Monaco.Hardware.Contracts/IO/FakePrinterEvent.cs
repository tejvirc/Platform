using static System.FormattableString;

namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using Kernel;

    /// <summary>
    ///     Used for printer emulation
    /// </summary>
    public class FakePrinterEvent : BaseEvent
    {
        /// <summary>
        ///     If paper is in the chute.  Nice.
        /// </summary>
        public bool PaperInChute;

        /// <summary>
        ///     If the paper tray is empty.
        /// </summary>
        public bool PaperEmpty;

        /// <summary>
        ///     If the paper is low.
        /// </summary>
        public bool PaperLow;

        /// <summary>
        ///     If there is a paper jam.
        /// </summary>
        public bool PaperJam;

        /// <summary>
        ///     True if top of form.
        /// </summary>
        public bool TopOfForm;

        /// <summary>
        ///     If the printer head is open.
        /// </summary>
        public bool PrintHeadOpen;

        /// <summary>
        ///     If the chassis is open.
        /// </summary>
        public bool ChassisOpen;

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant(
                $"{GetType().Name} [PaperInChute={PaperInChute}] [PaperEmpty={PaperEmpty}] [PaperLow={PaperLow}] [PaperJam={PaperJam}] [TopOfForm={TopOfForm}] [PrintHeadOpen={PrintHeadOpen}] [ChassisOpen={ChassisOpen}]");
        }
    }
}
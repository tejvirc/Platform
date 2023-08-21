namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using Properties;
    using System.Collections.Generic;

    /// <summary>
    ///     Events descriptor
    /// </summary>
    public static class NoteAcceptorEventsDescriptor
    {
        /// <summary>
        ///     The fault event strings
        /// </summary>
        public static readonly Dictionary<NoteAcceptorFaultTypes, string> FaultTexts =
            new Dictionary<NoteAcceptorFaultTypes, string>
            {
                //{ NoteAcceptorFaultTypes.Eaten, Resources.DocumentEatenText },
                { NoteAcceptorFaultTypes.FirmwareFault, Resources.NoteAcceptorFaultTypes_FirmwareFault },
                { NoteAcceptorFaultTypes.MechanicalFault, Resources.NoteAcceptorFaultTypes_MechanicalFault },
                { NoteAcceptorFaultTypes.OpticalFault, Resources.NoteAcceptorFaultTypes_OpticalFault },
                { NoteAcceptorFaultTypes.ComponentFault, Resources.NoteAcceptorFaultTypes_ComponentFault },
                { NoteAcceptorFaultTypes.NvmFault, Resources.NoteAcceptorFaultTypes_NvmFault },
                { NoteAcceptorFaultTypes.OtherFault, Resources.NoteAcceptorFaultTypes_OtherFault },
                { NoteAcceptorFaultTypes.StackerDisconnected, Resources.NoteAcceptorFaultTypes_StackerDisconnected },
                { NoteAcceptorFaultTypes.StackerFull, Resources.NoteAcceptorFaultTypes_StackerFull },
                { NoteAcceptorFaultTypes.StackerJammed, Resources.NoteAcceptorFaultTypes_StackerJammed },
                { NoteAcceptorFaultTypes.StackerFault, Resources.NoteAcceptorFaultTypes_StackerFault },
                { NoteAcceptorFaultTypes.NoteJammed, Resources.NoteAcceptorFaultTypes_NoteJammed },
                { NoteAcceptorFaultTypes.CheatDetected, Resources.NoteAcceptorFaultTypes_CheatDetected }
            };
    }
}

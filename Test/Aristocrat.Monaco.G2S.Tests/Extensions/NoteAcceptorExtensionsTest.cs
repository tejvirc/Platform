namespace Aristocrat.Monaco.G2S.Tests.Extensions
{
    using System;
    using G2S.Handlers.NoteAcceptor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NoteAcceptorExtensionsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenNoteAcceptorIsNullExpectException()
        {
            NoteAcceptorExtensions.GetNoteAcceptorData(null, "USD");
        }
    }
}
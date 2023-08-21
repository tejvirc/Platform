//******************************************************************************************
// This is prototype code that is still being actively worked on.
// Please do not modify it unless you notify the Core Technologies team of your intentions.
//******************************************************************************************
namespace Aristocrat.Sas.Client
{
    using System.Collections.Generic;

    public interface ISasCommandHandler
    {
        /// <summary>
        /// Gets the SAS command this handler handles
        /// </summary>
        /// <returns>The SAS command this handler handles</returns>
        byte Command();

        /// <summary>
        /// Handler for the command
        /// </summary>
        /// <param name="command"></param>
        void Handle(ICollection<byte> command);
    }
}

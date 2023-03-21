namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Kernel;

    /// <summary>
    ///     Definition of the ITransferOutExtension interface.
    /// </summary>
    public interface ITransferOutExtension:IService
    {
        /// <summary>
        ///     Action before transfer out
        /// </summary>
        long PreProcessor(long amount);

        /// <summary>
        ///     Action after transfer out
        /// </summary>
        void PosProcessor(long amount);
    }
}

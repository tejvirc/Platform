namespace Aristocrat.Monaco.Hhr.Services
{
    using System.Threading.Tasks;

    /// <summary>
    /// Service responsible for verifying the games selections made are valid
    /// </summary>
    public interface IGameSelectionVerificationService
    {
        /// <summary>
        /// Will verify the selections made by the operator are valid against what the determinant server has sent
        /// </summary>
        /// <returns></returns>
        Task Verify();
    }
}

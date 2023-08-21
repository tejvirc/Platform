namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     This interface defines method that allows to execute actions and conditions prior to loading the parameter fields.
    ///     It can be expanded if in future we need to execute certain actions and conditions after loading the parameter fields.
    /// </summary>
    public interface IParameterLoadActions
    {
        /// <summary>
        ///     Method to be executed prior to loading parameter fields
        /// </summary>
        void PreLoad();
    }
}
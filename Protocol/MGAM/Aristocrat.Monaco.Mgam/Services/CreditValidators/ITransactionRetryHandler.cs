namespace Aristocrat.Monaco.Mgam.Services.CreditValidators
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Messaging;

    public interface ITransactionRetryHandler
    {
        void Add(IRequest message);

        void Remove(IRequest message);

        void RegisterCommand(Type commandType, Func<object, Task<IResponse>> commandRetry);

        void RegisterRetryAction(Type commandType, Action commandAction);
    }
}
namespace Aristocrat.Monaco.Hhr.Client.WorkFlow
{
    using System.Threading.Tasks;

    internal class MessageWrapper<TInput, TOutput>
    {
        public MessageWrapper(TInput input, TaskCompletionSource<TOutput> taskCompletionSource)
        {
            Value = input;
            TaskCompletionSource = taskCompletionSource;
        }

        public TInput Value { get; set; }

        public TaskCompletionSource<TOutput> TaskCompletionSource { get; set; }
    }
}
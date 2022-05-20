namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    public interface IMask
    {
        MaskOperation MaskOperation { get; }
        int Value { get; }
        string TrueText { get; }
        string FalseText { get; }
        string DataMemberName { get; }
    }
}
namespace Aristocrat.Sas.Client
{
    public static class SasExtensions
    {
        public static string SasPollTupleToString(this (byte poll, decimal timeTaken) pollDataTuple)
        {
            return $"Poll {pollDataTuple.poll:X2}, {pollDataTuple.timeTaken:0.00}ms";
        }
    }
}

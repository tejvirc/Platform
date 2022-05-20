namespace Aristocrat.Monaco.Asp.Extensions
{
    using Client.Contracts;
    using System.Collections.Generic;
    
    public static class DataSourceExtensions
    {
        public static Dictionary<string, object> GetMemberSnapshot(this IDataSource dataSource, string memberName)
        {
            return new Dictionary<string, object> {{memberName, dataSource.GetMemberValue(memberName)}};
        }

        public static Dictionary<string, object> GetMemberSnapshot(this IDataSource dataSource, IEnumerable<string> memberNames)
        {
            var snapshot = new Dictionary<string, object>();

            foreach (var memberName in memberNames)
            {
                snapshot[memberName] = dataSource.GetMemberValue(memberName);
            }

            return snapshot;
        }
    }
}

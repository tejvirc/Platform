namespace Aristocrat.Monaco.Application.UI.Models
{
    using System.Text;
    using Contracts.Localization;

    public class IOPageStatus
    {
        public string ResourceKey { get; set; }
        public string[] ResourceObjects { get; set; }

        /// <summary>
        /// Instantiate the Status object with the required fields
        /// </summary>
        /// <param name="resourceKey">The resource key of the string to lookup for this status</param>
        /// <param name="resourceObjects">Optional parameter of values to pass in to be added to the resource string</param>
        public IOPageStatus(string resourceKey, params string[] resourceObjects)
        {
            ResourceKey = resourceKey;
            ResourceObjects = resourceObjects;
        }

        public string FormattedStatus
        {
            get
            {
                var sb = new StringBuilder();
                var resource = Localizer.For(CultureFor.Operator).GetString(ResourceKey);
                if (!string.IsNullOrWhiteSpace(resource))
                {
                    sb.Append(resource);
                    sb.Append(" ");
                }
                foreach (var obj in ResourceObjects)
                {
                    string objResource;
                    try
                    {
                        objResource = Localizer.For(CultureFor.Operator).GetString(obj);
                    }
                    catch
                    {
                        //Not a resource
                        objResource = string.Empty;
                    }
                    if (!string.IsNullOrWhiteSpace(objResource))
                    {
                        sb.Append(objResource);
                    }
                    else
                    {
                        sb.Append(obj);
                    }
                    sb.Append(" ");
                }
                return sb.ToString();
            }
        }
    }
}

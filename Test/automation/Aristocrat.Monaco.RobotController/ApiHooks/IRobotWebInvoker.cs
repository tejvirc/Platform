namespace Aristocrat.Monaco.RobotController.ApiHooks
{
    using Aristocrat.Monaco.TestController.DataModel;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    public interface IRobotWebInvoker
    {
        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/ToggleRobotMode",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult ToggleRobotMode();
    }
}

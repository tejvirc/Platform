namespace Aristocrat.Monaco.Mgam.UI.ViewModels
{
    using System;
    using System.Diagnostics;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Routing;
    using Common;

    public partial class HostTranscriptsViewModel
    {
        [Conditional("DESIGN")]
        private void WireDesignerData()
        {
            if (InDesigner)
            {
                RegisteredInstances.Add(
                    new RegisteredInstance(
                        new InstanceInfo
                        {
                            TimeStamp = DateTime.UtcNow,
                            ConnectionString = "0.0.0.0:6602",
                            DeviceId = 99,
                            InstanceId = 1001,
                            SiteId = 1,
                            Description = "MGAM Design Mode"
                        }));

                Messages.Add(new HostTranscript(new RoutedMessage
                {
                    Timestamp = DateTime.UtcNow,
                    Name = "RegisterInstance",
                    Source = "0.0.0.0",
                    Destination = "0.0.0.0",
                    ResponseCode = null,
                    IsCommand = false,
                    IsNotification = false,
                    IsRequest = true,
                    IsResponse = false,
                    IsHeartbeat = false
                }));

                Messages.Add(new HostTranscript(new RoutedMessage
                {
                    Timestamp = DateTime.UtcNow,
                    Name = "RegisterInstanceResponse",
                    Source = "0.0.0.0",
                    Destination = "0.0.0.0",
                    ResponseCode = 0,
                    IsCommand = false,
                    IsNotification = false,
                    IsRequest = false,
                    IsResponse = true,
                    IsHeartbeat = false
                }));

                Messages.Add(new HostTranscript(new RoutedMessage
                {
                    Timestamp = DateTime.UtcNow,
                    Name = "KeepAlive",
                    Source = "0.0.0.0",
                    Destination = "0.0.0.0",
                    ResponseCode = null,
                    IsCommand = false,
                    IsNotification = false,
                    IsRequest = true,
                    IsResponse = false,
                    IsHeartbeat = true
                }));

                Messages.Add(new HostTranscript(new RoutedMessage
                {
                    Timestamp = DateTime.UtcNow,
                    Name = "KeepAliveResponse",
                    Source = "0.0.0.0",
                    Destination = "0.0.0.0",
                    ResponseCode = 0,
                    IsCommand = false,
                    IsNotification = false,
                    IsRequest = false,
                    IsResponse = true,
                    IsHeartbeat = true
                }));

                Messages.Add(new HostTranscript(new RoutedMessage
                {
                    Timestamp = DateTime.UtcNow,
                    Name = "Command",
                    Source = "0.0.0.0",
                    Destination = "0.0.0.0",
                    ResponseCode = null,
                    IsCommand = true,
                    IsNotification = false,
                    IsRequest = true,
                    IsResponse = false,
                    IsHeartbeat = false
                }));

                Messages.Add(new HostTranscript(new RoutedMessage
                {
                    Timestamp = DateTime.UtcNow,
                    Name = "KeepAliveResponse",
                    Source = "0.0.0.0",
                    Destination = "0.0.0.0",
                    ResponseCode = 22,
                    IsCommand = true,
                    IsNotification = false,
                    IsRequest = false,
                    IsResponse = true,
                    IsHeartbeat = false
                }));

                Messages.Add(new HostTranscript(new RoutedMessage
                {
                    Timestamp = DateTime.UtcNow,
                    Name = "Notification",
                    Source = "0.0.0.0",
                    Destination = "0.0.0.0",
                    ResponseCode = null,
                    IsCommand = false,
                    IsNotification = true,
                    IsRequest = true,
                    IsResponse = false,
                    IsHeartbeat = false
                }));

                Messages.Add(new HostTranscript(new RoutedMessage
                {
                    Timestamp = DateTime.UtcNow,
                    Name = "NotificationResponse",
                    Source = "0.0.0.0",
                    Destination = "0.0.0.0",
                    ResponseCode = 0,
                    IsCommand = false,
                    IsNotification = true,
                    IsRequest = false,
                    IsResponse = true,
                    IsHeartbeat = false
                }));

                Messages.Add(new HostTranscript(new RoutedMessage
                {
                    Timestamp = DateTime.UtcNow,
                    Name = "ReadyToPlay",
                    Source = "0.0.0.0",
                    Destination = "0.0.0.0",
                    ResponseCode = null,
                    IsCommand = false,
                    IsNotification = false,
                    IsRequest = true,
                    IsResponse = false,
                    IsHeartbeat = false
                }));

                Messages.Add(new HostTranscript(new RoutedMessage
                {
                    Timestamp = DateTime.UtcNow,
                    Name = "ReadyToPlayResponse",
                    Source = "0.0.0.0",
                    Destination = "0.0.0.0",
                    ResponseCode = 0,
                    IsCommand = false,
                    IsNotification = false,
                    IsRequest = false,
                    IsResponse = true,
                    IsHeartbeat = false
                }));
            } 
        }
    }
}

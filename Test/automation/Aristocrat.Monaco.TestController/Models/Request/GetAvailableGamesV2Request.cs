namespace Aristocrat.Monaco.TestController.Models.Request
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using TestController.DataModel;

    public class GetAvailableGamesV2Request
    {
        [JsonPropertyName("test_name")]
        public string TestName { get; set; }
    }

    public class TryLoadGameV2Request
    {
        [JsonPropertyName("gameName")]
        public string GameName { get; set; }

        [JsonPropertyName("denomination")]
        public string Denomination { get; set; }

        [JsonPropertyName("test_name")]
        public string TestName { get; set; }
    }

    public class RequestSpinV2Request
    {
        [JsonPropertyName("test_name")]
        public string TestName { get; set; }
    }

    public class InsertCreditsV2Request
    {
        [JsonPropertyName("bill_value")]
        public int BillValue { get; set; }

        [JsonPropertyName("test_name")]
        public string TestName { get; set; }
    }

    public class RequestCashOutV2
    {
        [JsonPropertyName("test_name")]
        public string TestName { get; set; }
    }

    public class RequestBNAStatusV2
    {
        [JsonPropertyName("test_name")]
        public string TestName { get; set; }
    }

    public class AuditMenuRequest
    {
        [JsonPropertyName("open")]
        public bool Open { get; set; }
    }

    public class SelectMenuTabRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class RequestGameRequest
    {
        public string GameName { get; set; }
        public long Denomination { get; set; }
    }

    public class EnableCashOutRequest
    {
        [JsonPropertyName("enable")]
        public bool Enable { get; set; }
    }

    public class HandleRGRequest
    {
        [JsonPropertyName("enable")]
        public bool Enable { get; set; }
    }

    public class SetRgDialogOptionsRequest
    {
        [JsonPropertyName("buttonNames")]
        public string[] ButtonNames { get; set; }
    }

    public class SetBetLevelRequest
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }
    }

    public class SetLineLevelRequest
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }
    }

    public class SendInputRequest
    {
        [JsonPropertyName("input")]
        public int Input { get; set; }
    }

    public class SendTouchGameRequest
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    public class SendTouchVBDRequest
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    public class InsertCreditsRequest
    {
        [JsonPropertyName("bill_value")]
        public int BillValue { get; set; }
    }

    public class ServiceButtonRequest
    {
        [JsonPropertyName("pressed")]
        public bool Pressed { get; set; }
    }

    public class LockupRequest
    {
        [JsonPropertyName("type")]
        public LockupTypeEnum Type { get; set; }

        [JsonPropertyName("clear")]
        public bool Clear { get; set; }
    }

    public class SetMaxWinLimitOverrideRequest
    {
        [JsonPropertyName("maxWinLimitOverrideMillicents")]
        public int MaxWinLimitOverrideMillicents { get; set; }
    }

    public class RequestHandPayRequest
    {
        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("type")]
        public TransferOutType Type { get; set; }

        [JsonPropertyName("accountType")]
        public Account AccountType { get; set; }
    }

    public class GetInfoRequest
    {
        [JsonPropertyName("info")]
        public List<PlatformInfoEnum> Info { get; set; }
    }

    public class GetProgressivesRequest
    {
        [JsonPropertyName("gameName")]
        public string GameName { get; set; }
    }

    public class GetConfigOptionRequest
    {
        [JsonPropertyName("option")]
        public string Option { get; set; }
    }

    public class SetConfigOptionRequest
    {
        [JsonPropertyName("option")]
        public string Option { get; set; }

        [JsonPropertyName("value")]
        public object Value { get; set; }
    }

    public class WaitRequest
    {
        [JsonPropertyName("evtType")]
        public string EventType { get; set; }

        [JsonPropertyName("timeout")]
        public int Timeout { get; set; }
    }

    public class CancelWaitRequest
    {
        [JsonPropertyName("evtType")]
        public string EventType { get; set; }
    }

    public class WaitAllRequest
    {
        [JsonPropertyName("evtType")]
        public string[] EventType { get; set; }

        [JsonPropertyName("timeout")]
        public int Timeout { get; set; }
    }

    public class WaitAnyRequest
    {
        [JsonPropertyName("evtType")]
        public string[] EventType { get; set; }

        [JsonPropertyName("timeout")]
        public int Timeout { get; set; }
    }

    public class GetPropertyRequest
    {
        [JsonPropertyName("property")]
        public string Property { get; set; }
    }

    public class SetPropertyRequest
    {
        [JsonPropertyName("property")]
        public string Property { get; set; }

        [JsonPropertyName("value")]
        public object Value { get; set; }

        [JsonPropertyName("isConfig")]
        public bool IsConfig { get; set; }
    }

    public class SetIoRequest
    {
        [JsonPropertyName("index")]
        public string Index { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class NoteAcceptorSetMaskRequest
    {
        [JsonPropertyName("mask")]
        public string Mask { get; set; }
    }

    public class NoteAcceptorSetFirmwareRequest
    {
        [JsonPropertyName("contents")]
        public string Contents { get; set; }
    }

    public class InsertTicketRequest
    {
        [JsonPropertyName("validation_id")]
        public string ValidationId { get; set; }
    }

    public class PlayerCardEventRequest
    {
        [JsonPropertyName("inserted")]
        public bool Inserted { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }
    }

    public class InsertCardRequest
    {
        public string CardName { get; set; }
    }
}

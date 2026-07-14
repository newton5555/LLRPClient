using System.Buffers.Binary;
using System.Reflection;
using System.Xml.Linq;
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;

namespace LLRP.Cli.Delivery;

public enum FrameDirection { Tx, Rx }

public sealed record LlrpSemanticNode(string Name, string? Value = null, IReadOnlyList<LlrpSemanticNode>? Children = null)
{
    public IReadOnlyList<LlrpSemanticNode> Items { get; } = Children ?? [];
}

public sealed record LlrpFrame(
    DateTimeOffset Timestamp,
    FrameDirection Direction,
    byte[] Bytes,
    string MessageType,
    uint? MessageId,
    string Summary,
    string Xml,
    string? DecodeError,
    Message? DecodedMessage = null,
    byte? ProtocolVersion = null,
    uint? DeclaredLength = null,
    string? StatusCode = null,
    string? StatusDescription = null,
    LlrpSemanticNode? SemanticTree = null)
{
    public string Hex => Convert.ToHexString(Bytes);
    public bool IsResponse => MessageType.EndsWith("_RESPONSE", StringComparison.Ordinal) || MessageType == "ERROR_MESSAGE";
    public bool? IsSuccess => StatusCode is null ? null : StatusCode == "M_Success";

    public static LlrpFrame Decode(FrameDirection direction, byte[] raw)
    {
        var bytes = raw.ToArray();
        var header = ReadHeader(bytes);
        if (bytes.Length < 10)
            return new(DateTimeOffset.UtcNow, direction, bytes, header.Type, header.MessageId,
                FallbackSummary(header.Type, bytes), string.Empty, "Frame is shorter than the 10-byte LLRP header.",
                ProtocolVersion: header.Version, DeclaredLength: header.DeclaredLength,
                SemanticTree: BuildFallbackTree(header, bytes));

        try
        {
            var decodeBuffer = bytes.ToArray();
            LLRPBinaryDecoder.Decode(ref decodeBuffer, out Message? message);
            if (message == null)
                return new(DateTimeOffset.UtcNow, direction, bytes, header.Type, header.MessageId,
                    FallbackSummary(header.Type, bytes), string.Empty, "No message returned by decoder.",
                    ProtocolVersion: header.Version, DeclaredLength: header.DeclaredLength,
                    SemanticTree: BuildFallbackTree(header, bytes));

            var xml = message.ToString() ?? string.Empty;
            var status = ReadStatus(message);
            return new(DateTimeOffset.UtcNow, direction, bytes, message.GetType().Name.Replace("MSG_", string.Empty), message.MSG_ID,
                Summarize(message, status), PrettyXml(xml), ValidateHeader(header, bytes), message, header.Version, header.DeclaredLength,
                status?.StatusCode.ToString(), EmptyToNull(status?.ErrorDescription), BuildSemanticTree(message, xml, header));
        }
        catch (Exception ex)
        {
            var validation = ValidateHeader(header, bytes);
            var error = $"{ex.GetType().Name}: {ex.Message}" + (validation is null ? string.Empty : $"; {validation}");
            return new(DateTimeOffset.UtcNow, direction, bytes, header.Type, header.MessageId,
                FallbackSummary(header.Type, bytes), string.Empty, error,
                ProtocolVersion: header.Version, DeclaredLength: header.DeclaredLength,
                SemanticTree: BuildFallbackTree(header, bytes));
        }
    }

    public static string MessageTypeName(ushort type) => type switch
    {
        1 => "GET_READER_CAPABILITIES",
        2 => "GET_READER_CONFIG",
        3 => "SET_READER_CONFIG",
        4 => "CLOSE_CONNECTION_RESPONSE",
        11 => "GET_READER_CAPABILITIES_RESPONSE",
        12 => "GET_READER_CONFIG_RESPONSE",
        13 => "SET_READER_CONFIG_RESPONSE",
        14 => "CLOSE_CONNECTION",
        20 => "ADD_ROSPEC",
        21 => "DELETE_ROSPEC",
        22 => "START_ROSPEC",
        23 => "STOP_ROSPEC",
        24 => "ENABLE_ROSPEC",
        25 => "DISABLE_ROSPEC",
        26 => "GET_ROSPECS",
        30 => "ADD_ROSPEC_RESPONSE",
        31 => "DELETE_ROSPEC_RESPONSE",
        32 => "START_ROSPEC_RESPONSE",
        33 => "STOP_ROSPEC_RESPONSE",
        34 => "ENABLE_ROSPEC_RESPONSE",
        35 => "DISABLE_ROSPEC_RESPONSE",
        36 => "GET_ROSPECS_RESPONSE",
        40 => "ADD_ACCESSSPEC",
        41 => "DELETE_ACCESSSPEC",
        42 => "ENABLE_ACCESSSPEC",
        43 => "DISABLE_ACCESSSPEC",
        44 => "GET_ACCESSSPECS",
        45 => "CLIENT_REQUEST_OP",
        50 => "ADD_ACCESSSPEC_RESPONSE",
        51 => "DELETE_ACCESSSPEC_RESPONSE",
        52 => "ENABLE_ACCESSSPEC_RESPONSE",
        53 => "DISABLE_ACCESSSPEC_RESPONSE",
        54 => "GET_ACCESSSPECS_RESPONSE",
        55 => "CLIENT_REQUEST_OP_RESPONSE",
        60 => "GET_REPORT",
        61 => "RO_ACCESS_REPORT",
        62 => "KEEPALIVE",
        63 => "READER_EVENT_NOTIFICATION",
        64 => "ENABLE_EVENTS_AND_REPORTS",
        72 => "KEEPALIVE_ACK",
        100 => "ERROR_MESSAGE",
        1023 => "CUSTOM_MESSAGE",
        _ => $"TYPE_{type}"
    };

    private static Header ReadHeader(byte[] bytes)
    {
        if (bytes.Length < 2)
            return new(null, 0, 0, "TRUNCATED", null, null);
        var word = BinaryPrimitives.ReadUInt16BigEndian(bytes);
        var reserved = (byte)(word >> 13);
        var version = (byte)((word >> 10) & 0x07);
        var typeId = (ushort)(word & 0x03ff);
        uint? length = bytes.Length >= 6 ? BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(2, 4)) : null;
        uint? id = bytes.Length >= 10 ? BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(6, 4)) : null;
        return new(version, reserved, typeId, MessageTypeName(typeId), length, id);
    }

    private static PARAM_LLRPStatus? ReadStatus(Message message) =>
        message.GetType().GetField("LLRPStatus", BindingFlags.Public | BindingFlags.Instance)?.GetValue(message) as PARAM_LLRPStatus;

    private static string FallbackSummary(string type, byte[] bytes)
    {
        if (bytes.Length < 10) return "Truncated LLRP frame (header requires 10 bytes).";
        var declaredLength = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(2, 4));
        var suffix = $"declared length {declaredLength}, captured {bytes.Length}";
        if (bytes.Length >= 14 && type is "ENABLE_ROSPEC" or "DISABLE_ROSPEC" or "START_ROSPEC" or "STOP_ROSPEC" or "DELETE_ROSPEC")
            return $"{type}: ROSpecID {BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(10, 4))}; {suffix}";
        if (bytes.Length >= 11 && type == "GET_READER_CAPABILITIES")
            return $"GET_READER_CAPABILITIES: requested-data {bytes[10]}; {suffix}";
        return $"{type}; {suffix}";
    }

    private static string Summarize(Message message, PARAM_LLRPStatus? status)
    {
        var detail = message switch
        {
            MSG_RO_ACCESS_REPORT report => $"{report.TagReportData?.Length ?? 0} tag report(s)",
            MSG_GET_READER_CAPABILITIES request => $"requested data: {request.RequestedData}",
            MSG_GET_READER_CONFIG request => $"requested data: {request.RequestedData}, antenna: {request.AntennaID}",
            MSG_ENABLE_ROSPEC request => $"ROSpecID {request.ROSpecID}",
            MSG_DISABLE_ROSPEC request => $"ROSpecID {request.ROSpecID}",
            MSG_START_ROSPEC request => $"ROSpecID {request.ROSpecID}",
            MSG_STOP_ROSPEC request => $"ROSpecID {request.ROSpecID}",
            MSG_DELETE_ROSPEC request => $"ROSpecID {request.ROSpecID}",
            _ => string.Empty
        };
        if (status is not null)
        {
            var description = EmptyToNull(status.ErrorDescription);
            detail = $"status: {status.StatusCode}" + (description is null ? string.Empty : $" — {description}");
        }
        var name = message.GetType().Name.Replace("MSG_", string.Empty);
        return detail.Length == 0 ? name : $"{name}: {detail}";
    }

    private static LlrpSemanticNode BuildSemanticTree(Message message, string xml, Header header)
    {
        var children = new List<LlrpSemanticNode> { HeaderNode(header) };
        try
        {
            var root = XDocument.Parse(xml).Root;
            if (root is not null)
                children.AddRange(ConvertElements(root.Elements()));
        }
        catch
        {
            children.Add(new("Decoded type", message.GetType().Name));
        }
        return new(message.GetType().Name.Replace("MSG_", string.Empty), Children: children);
    }

    private static LlrpSemanticNode BuildFallbackTree(Header header, byte[] bytes)
    {
        var children = new List<LlrpSemanticNode> { HeaderNode(header) };
        if (bytes.Length >= 14 && header.Type is "ENABLE_ROSPEC" or "DISABLE_ROSPEC" or "START_ROSPEC" or "STOP_ROSPEC" or "DELETE_ROSPEC")
            children.Add(new("ROSpecID", BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(10, 4)).ToString()));
        if (bytes.Length >= 11 && header.Type == "GET_READER_CAPABILITIES")
            children.Add(new("RequestedData", bytes[10].ToString()));
        return new(header.Type, Children: children);
    }

    private static LlrpSemanticNode HeaderNode(Header header) => new("Header", Children:
    [
        new("Version", header.Version?.ToString() ?? "unknown"),
        new("Reserved", header.Reserved.ToString()),
        new("MessageType", $"{header.Type} ({header.TypeId})"),
        new("Length", header.DeclaredLength?.ToString() ?? "unknown"),
        new("MessageID", header.MessageId?.ToString() ?? "unknown")
    ]);

    private static IEnumerable<LlrpSemanticNode> ConvertElements(IEnumerable<XElement> elements)
    {
        var groups = elements.GroupBy(element => element.Name.LocalName);
        foreach (var group in groups)
        {
            var values = group.Take(50).ToArray();
            var index = 0;
            foreach (var element in values)
            {
                index++;
                var name = values.Length == 1 ? group.Key : $"{group.Key} [{index}]";
                var childElements = element.Elements().ToArray();
                if (childElements.Length == 0)
                    yield return new(name, Truncate(element.Value.Trim(), 180));
                else
                    yield return new(name, Children: ConvertElements(childElements).ToArray());
            }
            var omitted = group.Skip(50).Count();
            if (omitted > 0) yield return new($"{group.Key} […]", $"{omitted} additional item(s) omitted");
        }
    }

    private static string PrettyXml(string xml)
    {
        try { return XDocument.Parse(xml).ToString(); }
        catch { return xml; }
    }

    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length] + "…";
    private static string? ValidateHeader(Header header, byte[] bytes)
    {
        var warnings = new List<string>();
        if (header.Reserved != 0) warnings.Add($"reserved header bits are {header.Reserved}, expected 0");
        if (header.Version != 1) warnings.Add($"protocol version is {header.Version}, expected 1");
        if (header.DeclaredLength != bytes.Length) warnings.Add($"declared length is {header.DeclaredLength}, captured {bytes.Length}");
        return warnings.Count == 0 ? null : string.Join("; ", warnings);
    }

    private sealed record Header(byte? Version, byte Reserved, ushort TypeId, string Type, uint? DeclaredLength, uint? MessageId);
}

public sealed record LlrpTransaction(LlrpFrame Request, LlrpFrame? Response, TimeSpan? Duration)
{
    public bool Completed => Response is not null;
    public bool Succeeded => Response?.IsSuccess is not false;
}

public static class LlrpTransactionAnalyzer
{
    public static IReadOnlyList<LlrpTransaction> Correlate(IEnumerable<LlrpFrame> source)
    {
        var frames = source.OrderBy(frame => frame.Timestamp).ToArray();
        var result = new List<LlrpTransaction>();
        for (var i = 0; i < frames.Length; i++)
        {
            var request = frames[i];
            var expectedResponse = ExpectedResponse(request.MessageType);
            if (request.Direction != FrameDirection.Tx || request.MessageId is null || expectedResponse is null)
                continue;
            var response = frames.Skip(i + 1).FirstOrDefault(candidate =>
                candidate.Direction == FrameDirection.Rx && candidate.MessageId == request.MessageId &&
                (candidate.MessageType == expectedResponse || candidate.MessageType == "ERROR_MESSAGE"));
            result.Add(new(request, response, response is null ? null : response.Timestamp - request.Timestamp));
        }
        return result;
    }

    private static string? ExpectedResponse(string requestType) => requestType switch
    {
        "GET_READER_CAPABILITIES" => "GET_READER_CAPABILITIES_RESPONSE",
        "GET_READER_CONFIG" => "GET_READER_CONFIG_RESPONSE",
        "SET_READER_CONFIG" => "SET_READER_CONFIG_RESPONSE",
        "CLOSE_CONNECTION" => "CLOSE_CONNECTION_RESPONSE",
        "ADD_ROSPEC" => "ADD_ROSPEC_RESPONSE",
        "DELETE_ROSPEC" => "DELETE_ROSPEC_RESPONSE",
        "START_ROSPEC" => "START_ROSPEC_RESPONSE",
        "STOP_ROSPEC" => "STOP_ROSPEC_RESPONSE",
        "ENABLE_ROSPEC" => "ENABLE_ROSPEC_RESPONSE",
        "DISABLE_ROSPEC" => "DISABLE_ROSPEC_RESPONSE",
        "GET_ROSPECS" => "GET_ROSPECS_RESPONSE",
        "ADD_ACCESSSPEC" => "ADD_ACCESSSPEC_RESPONSE",
        "DELETE_ACCESSSPEC" => "DELETE_ACCESSSPEC_RESPONSE",
        "ENABLE_ACCESSSPEC" => "ENABLE_ACCESSSPEC_RESPONSE",
        "DISABLE_ACCESSSPEC" => "DISABLE_ACCESSSPEC_RESPONSE",
        "GET_ACCESSSPECS" => "GET_ACCESSSPECS_RESPONSE",
        "CLIENT_REQUEST_OP" => "CLIENT_REQUEST_OP_RESPONSE",
        _ => null
    };
}

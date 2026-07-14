using LLRP.Cli.Delivery;

namespace LLRP.Cli.Tests;

public sealed class ProtocolTests
{
    [Theory]
    [InlineData(30, "ADD_ROSPEC_RESPONSE")]
    [InlineData(31, "DELETE_ROSPEC_RESPONSE")]
    [InlineData(32, "START_ROSPEC_RESPONSE")]
    [InlineData(61, "RO_ACCESS_REPORT")]
    [InlineData(62, "KEEPALIVE")]
    [InlineData(72, "KEEPALIVE_ACK")]
    public void StandardMessageTypesUseCorrectNames(ushort type, string expected) =>
        Assert.Equal(expected, LlrpFrame.MessageTypeName(type));

    [Fact]
    public void DecodesStandardRequestHeaderAndFixedField()
    {
        var frame = LlrpFrame.Decode(FrameDirection.Tx, Convert.FromHexString("04160000000E0000002A00000001"));

        Assert.Equal("START_ROSPEC", frame.MessageType);
        Assert.Equal((uint)42, frame.MessageId);
        Assert.Equal((byte)1, frame.ProtocolVersion);
        Assert.Equal((uint)14, frame.DeclaredLength);
        Assert.Contains("ROSpecID 1", frame.Summary);
        Assert.NotNull(frame.SemanticTree);
    }

    [Fact]
    public void DecodesSuccessfulLlrpStatus()
    {
        var frame = LlrpFrame.Decode(FrameDirection.Rx, Convert.FromHexString("0420000000120000002A011F000800000000"));

        Assert.Equal("START_ROSPEC_RESPONSE", frame.MessageType);
        Assert.Equal("M_Success", frame.StatusCode);
        Assert.True(frame.IsResponse);
        Assert.True(frame.IsSuccess);
        Assert.Null(frame.DecodeError);
    }

    [Fact]
    public void CorrelatesRequestAndResponseByMessageId()
    {
        var request = LlrpFrame.Decode(FrameDirection.Tx, Convert.FromHexString("04160000000E0000002A00000001")) with
        { Timestamp = DateTimeOffset.UnixEpoch };
        var response = LlrpFrame.Decode(FrameDirection.Rx, Convert.FromHexString("0420000000120000002A011F000800000000")) with
        { Timestamp = DateTimeOffset.UnixEpoch.AddMilliseconds(25) };

        var transaction = Assert.Single(LlrpTransactionAnalyzer.Correlate([request, response]));
        Assert.Same(request, transaction.Request);
        Assert.Same(response, transaction.Response);
        Assert.Equal(TimeSpan.FromMilliseconds(25), transaction.Duration);
        Assert.True(transaction.Succeeded);
    }

    [Fact]
    public void HexDumpIncludesOffsetsBytesAndAscii()
    {
        var dump = FrameOutput.HexDump([0x41, 0x42, 0x00]);

        Assert.Contains("0000", dump);
        Assert.Contains("41 42 00", dump);
        Assert.Contains("AB.", dump);
    }

    [Fact]
    public void ReportsDeclaredLengthMismatchWithoutDroppingRawFrame()
    {
        var frame = LlrpFrame.Decode(FrameDirection.Tx, Convert.FromHexString("04160000000F0000002A00000001"));

        Assert.Equal(14, frame.Bytes.Length);
        Assert.Contains("declared length", frame.DecodeError, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("04160000000F0000002A00000001", frame.Hex);
    }

    [Fact]
    public void DoesNotInventTransactionForKeepaliveAcknowledgement()
    {
        var acknowledgement = LlrpFrame.Decode(FrameDirection.Tx, Convert.FromHexString("04480000000A0000002A"));

        Assert.Equal("KEEPALIVE_ACK", acknowledgement.MessageType);
        Assert.Empty(LlrpTransactionAnalyzer.Correlate([acknowledgement]));
    }
}

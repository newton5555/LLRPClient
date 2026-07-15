using System.Xml;
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;

namespace LLRP.Cli.Delivery;

public sealed record RospecEditPatch(
    byte? Priority = null,
    ushort? Session = null,
    ushort? TagPopulation = null,
    uint? StopAfterMilliseconds = null,
    ushort? ReportEvery = null,
    bool? IncludeAntennaId = null,
    bool? IncludePeakRssi = null)
{
    public bool HasChanges => Priority is not null || Session is not null || TagPopulation is not null ||
                              StopAfterMilliseconds is not null || ReportEvery is not null ||
                              IncludeAntennaId is not null || IncludePeakRssi is not null;
}

public sealed record RospecEditableValues(
    byte Priority,
    ushort? Session,
    ushort? TagPopulation,
    ENUM_ROSpecStopTriggerType StopTrigger,
    uint? StopAfterMilliseconds,
    ENUM_ROReportTriggerType? ReportTrigger,
    ushort? ReportEvery,
    bool? IncludeAntennaId,
    bool? IncludePeakRssi);

public sealed record RospecEditResult(
    uint RospecId,
    ENUM_ROSpecState OriginalState,
    RospecEditableValues Before,
    RospecEditableValues After,
    bool Applied);

public static class RospecEditor
{
    public static RospecEditableValues Read(PARAM_ROSpec roSpec)
    {
        ArgumentNullException.ThrowIfNull(roSpec);
        var controls = GetSingulationControls(roSpec).ToArray();
        var selector = roSpec.ROReportSpec?.TagReportContentSelector;
        var stop = roSpec.ROBoundarySpec?.ROSpecStopTrigger;

        return new(
            roSpec.Priority,
            SingleValue(controls.Select(control => control.Session.ToInt())),
            SingleValue(controls.Select(control => control.TagPopulation)),
            stop?.ROSpecStopTriggerType ?? ENUM_ROSpecStopTriggerType.Null,
            stop?.ROSpecStopTriggerType == ENUM_ROSpecStopTriggerType.Duration
                ? stop.DurationTriggerValue
                : null,
            roSpec.ROReportSpec?.ROReportTrigger,
            roSpec.ROReportSpec?.N,
            selector?.EnableAntennaID,
            selector?.EnablePeakRSSI);
    }

    public static RospecEditableValues Apply(PARAM_ROSpec roSpec, RospecEditPatch patch)
    {
        ArgumentNullException.ThrowIfNull(roSpec);
        ArgumentNullException.ThrowIfNull(patch);

        if (patch.Priority is { } priority) roSpec.Priority = priority;

        if (patch.Session is not null || patch.TagPopulation is not null)
        {
            var controls = GetSingulationControls(roSpec).ToArray();
            if (controls.Length == 0)
                throw new InvalidOperationException("ROSpec does not contain a standard C1G2 singulation control parameter.");

            foreach (var control in controls)
            {
                if (patch.Session is { } session) control.Session = new TwoBits(session);
                if (patch.TagPopulation is { } population) control.TagPopulation = population;
            }
        }

        if (patch.StopAfterMilliseconds is { } stopAfter)
        {
            var stop = roSpec.ROBoundarySpec?.ROSpecStopTrigger ??
                       throw new InvalidOperationException("ROSpec does not contain an ROSpec stop trigger.");
            stop.ROSpecStopTriggerType = stopAfter == 0
                ? ENUM_ROSpecStopTriggerType.Null
                : ENUM_ROSpecStopTriggerType.Duration;
            stop.DurationTriggerValue = stopAfter;
            stop.GPITriggerValue = null;
        }

        if (patch.ReportEvery is not null || patch.IncludeAntennaId is not null || patch.IncludePeakRssi is not null)
        {
            var report = roSpec.ROReportSpec ??
                         throw new InvalidOperationException("ROSpec does not contain an ROReportSpec parameter.");
            if (patch.ReportEvery is { } reportEvery)
            {
                report.ROReportTrigger = ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_ROSpec;
                report.N = reportEvery;
            }

            if (patch.IncludeAntennaId is not null || patch.IncludePeakRssi is not null)
            {
                var selector = report.TagReportContentSelector ??
                               throw new InvalidOperationException("ROSpec does not contain a tag report content selector.");
                if (patch.IncludeAntennaId is { } antenna) selector.EnableAntennaID = antenna;
                if (patch.IncludePeakRssi is { } rssi) selector.EnablePeakRSSI = rssi;
            }
        }

        return Read(roSpec);
    }

    public static PARAM_ROSpec Clone(PARAM_ROSpec roSpec)
    {
        ArgumentNullException.ThrowIfNull(roSpec);
        var document = new XmlDocument();
        document.LoadXml(roSpec.ToString());
        return PARAM_ROSpec.FromXmlNode(document.DocumentElement!) ??
               throw new InvalidOperationException("Unable to clone the ROSpec returned by the reader.");
    }

    private static IEnumerable<PARAM_C1G2SingulationControl> GetSingulationControls(PARAM_ROSpec roSpec)
    {
        if (roSpec.SpecParameter is null) yield break;
        for (var specIndex = 0; specIndex < roSpec.SpecParameter.Count; specIndex++)
        {
            if (roSpec.SpecParameter[specIndex] is not PARAM_AISpec aiSpec || aiSpec.InventoryParameterSpec is null)
                continue;

            foreach (var inventory in aiSpec.InventoryParameterSpec)
            {
                if (inventory?.AntennaConfiguration is null) continue;
                foreach (var antenna in inventory.AntennaConfiguration)
                {
                    if (antenna?.AirProtocolInventoryCommandSettings is null) continue;
                    for (var commandIndex = 0; commandIndex < antenna.AirProtocolInventoryCommandSettings.Count; commandIndex++)
                    {
                        if (antenna.AirProtocolInventoryCommandSettings[commandIndex] is PARAM_C1G2InventoryCommand
                            {
                                C1G2SingulationControl: not null
                            } command)
                            yield return command.C1G2SingulationControl;
                    }
                }
            }
        }
    }

    private static ushort? SingleValue(IEnumerable<ushort> values)
    {
        var distinct = values.Distinct().Take(2).ToArray();
        return distinct.Length == 1 ? distinct[0] : null;
    }
}

using Spectre.Console;

namespace LLRP.Cli.Delivery;

/// <summary>Inline Spectre editor that leaves scrolling and input ownership to the terminal.</summary>
internal sealed class RospecEditScreen
{
    private readonly uint _rospecId;
    private readonly RospecEditableValues _original;
    private RospecEditPatch _patch = new();

    public RospecEditScreen(uint rospecId, RospecEditableValues original)
    {
        _rospecId = rospecId;
        _original = original;
    }

    public RospecEditPatch? Run(IAnsiConsole console)
    {
        while (true)
        {
            var choice = console.Prompt(new SelectionPrompt<EditChoice>()
                .Title($"[bold aqua]ROSpec {_rospecId} editor[/]  [grey]Select a field to edit[/]")
                .PageSize(12)
                .HighlightStyle(new Style(Color.Black, Color.Aqua, Decoration.Bold))
                .UseConverter(FormatChoice)
                .AddChoices(Choices));

            if (choice.Action == Action.Save) return _patch;
            if (choice.Action == Action.Discard) return null;
            Edit(console, choice.Field!.Value);
        }
    }

    private void Edit(IAnsiConsole console, Field field)
    {
        switch (field)
        {
            case Field.Session:
                var session = Select(console, field, ["S0", "S1", "S2", "S3"]);
                _patch = _patch with { Session = ushort.Parse(session[1..]) };
                break;
            case Field.IncludeAntennaId:
                _patch = _patch with { IncludeAntennaId = Select(console, field, ["On", "Off"]) == "On" };
                break;
            case Field.IncludePeakRssi:
                _patch = _patch with { IncludePeakRssi = Select(console, field, ["On", "Off"]) == "On" };
                break;
            default:
                EditNumber(console, field);
                break;
        }
    }

    private string Select(IAnsiConsole console, Field field, IReadOnlyList<string> values) =>
        console.Prompt(new SelectionPrompt<string>()
            .Title($"[bold aqua]{Markup.Escape(Label(field))}[/]  [grey]Current: {Markup.Escape(Value(field))}[/]")
            .HighlightStyle(new Style(Color.Black, Color.Aqua, Decoration.Bold))
            .AddChoices(values));

    private void EditNumber(IAnsiConsole console, Field field)
    {
        var prompt = new TextPrompt<string>($"[bold aqua]{Markup.Escape(Label(field))}[/] [grey]({Markup.Escape(Hint(field))})[/]")
            .ValidationErrorMessage($"[red]{Markup.Escape(Hint(field))}[/]")
            .Validate(value => IsValid(field, value)
                ? ValidationResult.Success()
                : ValidationResult.Error($"[red]{Markup.Escape(Hint(field))}[/]"));
        var current = NumericValue(field);
        if (current.Length > 0) prompt.DefaultValue(current);
        var value = console.Prompt(prompt);
        SetNumber(field, value);
    }

    private static bool IsValid(Field field, string value) => field switch
    {
        Field.Priority => byte.TryParse(value, out _),
        Field.TagPopulation => ushort.TryParse(value, out var population) && population > 0,
        Field.StopAfterMilliseconds => uint.TryParse(value, out _),
        Field.ReportEvery => ushort.TryParse(value, out _),
        _ => false
    };

    private void SetNumber(Field field, string value) => _patch = field switch
    {
        Field.Priority => _patch with { Priority = byte.Parse(value) },
        Field.TagPopulation => _patch with { TagPopulation = ushort.Parse(value) },
        Field.StopAfterMilliseconds => _patch with { StopAfterMilliseconds = uint.Parse(value) },
        Field.ReportEvery => _patch with { ReportEvery = ushort.Parse(value) },
        _ => _patch
    };

    private string FormatChoice(EditChoice choice)
    {
        if (choice.Field is { } field) return $"{Label(field),-24} {Value(field)}";
        return choice.Action == Action.Save
            ? _patch.HasChanges ? "Save changes" : "Finish without changes"
            : "Discard changes";
    }

    private string Value(Field field) => field switch
    {
        Field.Priority => (_patch.Priority ?? _original.Priority).ToString(),
        Field.Session => _patch.Session is { } session ? $"S{session}" : _original.Session is { } original ? $"S{original}" : "mixed / unavailable",
        Field.TagPopulation => (_patch.TagPopulation ?? _original.TagPopulation)?.ToString() ?? "mixed / unavailable",
        Field.StopAfterMilliseconds => (_patch.StopAfterMilliseconds ?? _original.StopAfterMilliseconds)?.ToString() ?? "disabled",
        Field.ReportEvery => (_patch.ReportEvery ?? _original.ReportEvery)?.ToString() ?? "unavailable",
        Field.IncludeAntennaId => OnOff(_patch.IncludeAntennaId ?? _original.IncludeAntennaId),
        Field.IncludePeakRssi => OnOff(_patch.IncludePeakRssi ?? _original.IncludePeakRssi),
        _ => string.Empty
    };

    private string NumericValue(Field field) => Value(field) is "disabled" or "unavailable" or "mixed / unavailable"
        ? string.Empty
        : Value(field);

    private static string OnOff(bool? value) => value is null ? "mixed / unavailable" : value.Value ? "On" : "Off";

    private static string Label(Field field) => field switch
    {
        Field.Priority => "Priority",
        Field.Session => "C1G2 session",
        Field.TagPopulation => "Tag population",
        Field.StopAfterMilliseconds => "Stop duration (ms)",
        Field.ReportEvery => "Report every (tags)",
        Field.IncludeAntennaId => "Include antenna ID",
        Field.IncludePeakRssi => "Include peak RSSI",
        _ => field.ToString()
    };

    private static string Hint(Field field) => field switch
    {
        Field.Priority => "range 0-255",
        Field.TagPopulation => "range 1-65535",
        Field.StopAfterMilliseconds => "range 0-4294967295; 0 disables the duration trigger",
        Field.ReportEvery => "range 0-65535; 0 reports at ROSpec end",
        _ => "enter a valid value"
    };

    private static readonly EditChoice[] Choices =
    [
        new(Field.Priority),
        new(Field.Session),
        new(Field.TagPopulation),
        new(Field.StopAfterMilliseconds),
        new(Field.ReportEvery),
        new(Field.IncludeAntennaId),
        new(Field.IncludePeakRssi),
        new(Action: Action.Save),
        new(Action: Action.Discard)
    ];

    private enum Field { Priority, Session, TagPopulation, StopAfterMilliseconds, ReportEvery, IncludeAntennaId, IncludePeakRssi }
    private enum Action { Save, Discard }
    private sealed record EditChoice(Field? Field = null, Action? Action = null);
}

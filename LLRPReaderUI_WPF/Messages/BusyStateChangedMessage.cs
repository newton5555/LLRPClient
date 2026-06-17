using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LLRPReaderUI_WPF.Messages;

public sealed class BusyStateChangedMessage(bool isBusy, string? text) : ValueChangedMessage<bool>(isBusy)
{
    public string? Text { get; } = text;
}

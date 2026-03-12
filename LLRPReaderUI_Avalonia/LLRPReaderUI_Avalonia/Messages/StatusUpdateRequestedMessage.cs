using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LLRPReaderUI_Avalonia.Messages;

/// <summary>
/// Message to request status update after inventory operations
/// </summary>
public sealed class StatusUpdateRequestedMessage : ValueChangedMessage<string>
{
    public StatusUpdateRequestedMessage(string reason) : base(reason)
    {
    }
}


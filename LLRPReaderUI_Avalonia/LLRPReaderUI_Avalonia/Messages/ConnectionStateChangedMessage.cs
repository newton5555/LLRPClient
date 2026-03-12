using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LLRPReaderUI_Avalonia.Messages;

public sealed class ConnectionStateChangedMessage(bool isConnected) : ValueChangedMessage<bool>(isConnected);


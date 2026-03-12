using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LLRPReaderUI_WPF.Messages;

public sealed class ConnectionStateChangedMessage(bool isConnected) : ValueChangedMessage<bool>(isConnected);

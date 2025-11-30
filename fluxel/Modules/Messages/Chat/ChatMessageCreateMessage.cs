using System;

namespace fluxel.Modules.Messages.Chat;

public class ChatMessageCreateMessage
{
    public Guid Message { get; }

    public ChatMessageCreateMessage(Guid message)
    {
        Message = message;
    }
}

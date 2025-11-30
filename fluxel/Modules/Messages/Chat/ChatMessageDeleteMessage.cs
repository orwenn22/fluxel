using System;

namespace fluxel.Modules.Messages.Chat;

public class ChatMessageDeleteMessage
{
    public Guid Message { get; }

    public ChatMessageDeleteMessage(Guid message)
    {
        Message = message;
    }
}

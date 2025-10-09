using backend.Models.Entities;
using backend.Models.Interfaces;

namespace backend.Services;

public class ChatMessageService
{
    private readonly IChatMessageRepository _chatMessageRepository;

    public ChatMessageService(IChatMessageRepository chatMessageRepository)
    {
        _chatMessageRepository = chatMessageRepository;
    }

    public async Task<ChatMessage> StoreMessage(Guid UserId, string message, string sender)
    {
        // Store message in database
        var messageObj = new ChatMessage()
        {
            UserId = UserId,
            Sender = sender,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _chatMessageRepository.AddAsync(messageObj);

        return result;
    }
}

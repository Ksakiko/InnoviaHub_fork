using System;
using backend.Models.Entities;

namespace backend.Models.Interfaces;

public interface IChatMessageRepository
{
    Task<IEnumerable<ChatMessage>> GetAllAsync();
    IEnumerable<ChatMessage> GetByUserId(Guid id);
    IEnumerable<ChatMessage> GetBySessionId(long id);
    Task<ChatMessage> AddAsync(ChatMessage message);
}

using System;
using backend.Models.Entities;
using backend.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Models.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly AppDbContext _context;

    public ChatMessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ChatMessage>> GetAllAsync()
    {
        return await _context.ChatMessages.ToListAsync();
    }
    public IEnumerable<ChatMessage> GetByUserId(Guid id)
    {
        var messages = _context.ChatMessages.Where(x => x.UserId == id);
        return messages;
    }
    // public IEnumerable<ChatMessage> GetBySessionId(long id)
    // {
    //     var messages = _context.ChatMessages.Where(x => x.SessionId == id);
    //     return messages;
    // }
    public async Task<ChatMessage> AddAsync(ChatMessage message)
    {
        await _context.ChatMessages.AddAsync(message);
        await _context.SaveChangesAsync();
        return message;
    }
}

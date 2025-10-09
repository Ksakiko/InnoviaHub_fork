using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities;

public class ChatMessage
{
    public long? Id { get; set; }
    [Required]
    public Guid UserId { get; set; }
    // public long SessionId { get; set; }
    [Required]
    public string Sender { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

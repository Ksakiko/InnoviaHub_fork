using System;

namespace backend.DTOs;

public class ChatRequestDTO
{
    public string UserInput { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}

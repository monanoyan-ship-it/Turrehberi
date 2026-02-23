namespace ErkanTatilPlani.Core.Entities;

public class Message : BaseEntity
{
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public int SenderTypeId { get; set; } // 0: visitor, 1: company
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
    public virtual Visitor Sender { get; set; } = null!;
}

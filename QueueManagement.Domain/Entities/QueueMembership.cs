namespace QueueManagement.Domain.Entities;

public class QueueMembership
{
    public int QueueId { get; set; }
    public Queue Queue { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
}
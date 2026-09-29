namespace QueueManagement.Domain.Entities;

public class Queue
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string AccessCode { get; set; } = "";
    public int? AdminUserId { get; set; }
    public User? AdminUser { get; set; }
    public ICollection<QueueMembership> Members { get; set; } = new List<QueueMembership>();
    public ICollection<QueueToken> Tokens { get; set; } = new List<QueueToken>();
}
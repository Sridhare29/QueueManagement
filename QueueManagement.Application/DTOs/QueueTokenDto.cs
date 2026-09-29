namespace QueueManagement.Application.DTOs
{
    public record QueueTokenDto(
        int Id,
        string TokenNo,
        string Status,
        string UserName,
        int? CounterId,
        string? CounterName,
        DateTime CreatedDate,
        DateTime? CalledTime,
        DateTime? CompletedTime,
        int? PositionInQueue);

    public class GenerateTokenRequest
    {
        public string Name { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
    }

    public record CreateQueueRequest(string Name);
    public record JoinQueueRequest(string AccessCode);
    public record CreatedQueueDto(int Id, string Name, string AccessCode);
    public record QueueSummaryDto(int Id, string Name, bool IsOwner, string? AccessCode);

    public record CallNextRequest(int CounterId);

    public record ErrorResponse(string Message);
}
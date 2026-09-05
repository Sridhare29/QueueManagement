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

    public record CallNextRequest(int CounterId);

    public record ErrorResponse(string Message);
}
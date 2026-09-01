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

    public record GenerateTokenRequest(int UserId);

    public record ErrorResponse(string Message);
}
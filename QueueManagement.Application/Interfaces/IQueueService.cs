using QueueManagement.Application.DTOs;

namespace QueueManagement.Application.Interfaces
{
    public interface IQueueService
    {
        Task<QueueTokenDto> GenerateToken(GenerateTokenRequest request);
        Task<QueueTokenDto?> CallNext(string tokenNo, int counterId);
        Task<QueueTokenDto> CompleteToken(string tokenNo);
        Task<List<QueueTokenDto>> GetWaitingQueue();
        Task<QueueTokenDto> GetTokenStatus(string tokenNo);
    }
}
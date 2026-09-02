using QueueManagement.Application.DTOs;

namespace QueueManagement.Application.Interfaces
{
    public interface IQueueService
    {
        Task<QueueTokenDto> GenerateToken(GenerateTokenRequest request);
        Task<QueueTokenDto?> CallNext(string tokenNo, int counterId);
        Task<QueueTokenDto> CompleteToken(int tokenId);
        Task<List<QueueTokenDto>> GetWaitingQueue();
        Task<QueueTokenDto> GetTokenStatus(int tokenId);
    }
}
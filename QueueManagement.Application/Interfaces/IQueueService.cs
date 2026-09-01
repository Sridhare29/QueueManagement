using QueueManagement.Application.DTOs;

namespace QueueManagement.Application.Interfaces
{
    public interface IQueueService
    {
        Task<QueueTokenDto> GenerateToken(int userId);
        Task<QueueTokenDto?> CallNext(int counterId);
        Task<QueueTokenDto> CompleteToken(int tokenId);
        Task<List<QueueTokenDto>> GetWaitingQueue();
        Task<QueueTokenDto> GetTokenStatus(int tokenId);
    }
}
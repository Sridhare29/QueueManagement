using QueueManagement.Application.DTOs;

namespace QueueManagement.Application.Interfaces
{
    public interface IQueueService
    {
        Task<CreatedQueueDto> CreateQueue(string name, int adminUserId);
        Task<QueueSummaryDto> JoinQueue(string accessCode, int userId);
        Task<List<QueueSummaryDto>> GetMyQueues(int userId);
        Task<QueueTokenDto> GenerateToken(int queueId, int userId);
        Task<QueueTokenDto?> CallNext(int queueId, string tokenNo, int counterId, int adminUserId);
        Task<QueueTokenDto> CompleteToken(int queueId, string tokenNo, int adminUserId);
        Task<List<QueueTokenDto>> GetWaitingQueue(int queueId, int adminUserId);
        Task<QueueTokenDto> GetTokenStatus(int queueId, string tokenNo, int userId);
    }
}
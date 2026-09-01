using QueueManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManagement.Application.Interfaces
{
    public interface IQueueService
    {
        Task<QueueToken> GenerateToken(int userId);

        Task<QueueToken?> CallNext(int counterId);

        Task CompleteToken(int tokenId);

        Task<List<QueueToken>> GetWaitingQueue();

    }
}

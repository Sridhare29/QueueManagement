using Microsoft.EntityFrameworkCore;
using QueueManagement.API.Data;
using QueueManagement.Application.DTOs;
using QueueManagement.Application.Interfaces;
using QueueManagement.Domain.Entities;
using QueueManagement.Domain.Enums;
using System.Security.Cryptography;

namespace QueueManagement.Application.Services
{
    public class QueueService : IQueueService
    {
        private readonly AppDbContext _context;

        public QueueService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CreatedQueueDto> CreateQueue(string name, int adminUserId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Queue name is required.");

            var queueName = name.Trim();

            for (var attempt = 0; attempt < 20; attempt++)
            {
                var accessCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                if (await _context.Queues.AnyAsync(x => x.AccessCode == accessCode))
                    continue;

                var queue = new Queue
                {
                    Name = queueName,
                    AccessCode = accessCode,
                    AdminUserId = adminUserId
                };
                _context.Queues.Add(queue);
                _context.QueueMemberships.Add(new QueueMembership
                {
                    Queue = queue,
                    UserId = adminUserId,
                    JoinedAt = DateTime.UtcNow
                });

                try
                {
                    await _context.SaveChangesAsync();
                    return new CreatedQueueDto(queue.Id, queue.Name, queue.AccessCode);
                }
                catch (DbUpdateException) when (attempt < 19)
                {
                    _context.Entry(queue).State = EntityState.Detached;
                    foreach (var membership in _context.ChangeTracker.Entries<QueueMembership>()
                                 .Where(x => x.Entity.Queue == queue))
                        membership.State = EntityState.Detached;
                }
            }

            throw new InvalidOperationException("Could not create a unique queue access code. Please retry.");
        }

        public async Task<QueueSummaryDto> JoinQueue(string accessCode, int userId)
        {
            if (string.IsNullOrWhiteSpace(accessCode))
                throw new KeyNotFoundException("No queue was found for that access code.");

            var normalizedCode = accessCode.Trim();
            var queue = await _context.Queues
                .FirstOrDefaultAsync(x => x.AccessCode == normalizedCode);
            if (queue == null)
                throw new KeyNotFoundException("No queue was found for that access code.");

            var alreadyJoined = await _context.QueueMemberships
                .AnyAsync(x => x.QueueId == queue.Id && x.UserId == userId);
            if (!alreadyJoined)
            {
                _context.QueueMemberships.Add(new QueueMembership
                {
                    QueueId = queue.Id,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            var isOwner = queue.AdminUserId == userId;
            return new QueueSummaryDto(queue.Id, queue.Name, isOwner, isOwner ? queue.AccessCode : null);
        }

        public async Task<List<QueueSummaryDto>> GetMyQueues(int userId)
        {
            return await _context.Queues
                .Where(queue => queue.AdminUserId == userId ||
                    queue.Members.Any(member => member.UserId == userId))
                .OrderBy(queue => queue.Name)
                .Select(queue => new QueueSummaryDto(
                    queue.Id,
                    queue.Name,
                    queue.AdminUserId == userId,
                    queue.AdminUserId == userId ? queue.AccessCode : null))
                .ToListAsync();
        }

        public async Task<QueueTokenDto> GenerateToken(int queueId, int userId)
        {
            await EnsureQueueMember(queueId, userId);

            const int maxAttempts = 5;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var tokenNumbers = await _context.QueueTokens
                    .Where(x => x.QueueId == queueId)
                    .Select(x => x.TokenNo)
                    .ToListAsync();

                var nextTokenNumber = tokenNumbers
                    .Select(tokenNo => int.TryParse(tokenNo.TrimStart('A'), out var number) ? number : 0)
                    .DefaultIfEmpty()
                    .Max() + 1;

                var token = new QueueToken
                {
                    QueueId = queueId,
                    UserId = userId,
                    TokenNo = $"A{nextTokenNumber:000}",
                    CreatedDate = DateTime.UtcNow,
                    Status = QueueStatus.Waiting
                };

                _context.QueueTokens.Add(token);

                try
                {
                    await _context.SaveChangesAsync();
                    return await MapToDto(token);
                }
                catch (DbUpdateException) when (attempt < maxAttempts)
                {
                    _context.Entry(token).State = EntityState.Detached;
                }
            }

            throw new InvalidOperationException("Could not generate a unique token number, please retry.");
        }

        public async Task<QueueTokenDto?> CallNext(int queueId, string tokenNo, int counterId, int adminUserId)
        {
            await EnsureQueueOwner(queueId, adminUserId);
            var counter = await _context.Counters.FirstOrDefaultAsync(x => x.Id == counterId);
            if (counter == null)
                throw new KeyNotFoundException($"Counter with Id {counterId} was not found.");

            if (!counter.IsActive)
                throw new InvalidOperationException($"Counter {counter.CounterName} is inactive.");

            // Serializable transaction so two counters can't grab the same
            // waiting token at the same time.
            await using var tx = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

            try
            {
                var token = await _context.QueueTokens
                    .Where(x => x.QueueId == queueId && x.TokenNo == tokenNo && x.Status == QueueStatus.Waiting)
                    .FirstOrDefaultAsync();

                if (token == null)
                    throw new KeyNotFoundException($"Waiting token '{tokenNo}' was not found.");

                token.Status = QueueStatus.Serving;
                token.CounterId = counterId;
                token.CalledTime = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return await MapToDto(token);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<QueueTokenDto> CompleteToken(int queueId, string tokenNo, int adminUserId)
        {
            await EnsureQueueOwner(queueId, adminUserId);
            var token = await _context.QueueTokens
                .FirstOrDefaultAsync(x => x.QueueId == queueId && x.TokenNo == tokenNo);
            if (token == null)
                throw new KeyNotFoundException($"Token '{tokenNo}' was not found.");

            if (token.Status != QueueStatus.Serving)
                throw new InvalidOperationException(
                    $"Token {token.TokenNo} cannot be completed from status '{token.Status}'. It must be Serving.");

            token.Status = QueueStatus.Completed;
            token.CompletedTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await MapToDto(token);
        }

        public async Task<List<QueueTokenDto>> GetWaitingQueue(int queueId, int adminUserId)
        {
            await EnsureQueueOwner(queueId, adminUserId);
            var tokens = await _context.QueueTokens
                .Include(x => x.User)
                .Where(x => x.QueueId == queueId && x.Status == QueueStatus.Waiting)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();

            var result = new List<QueueTokenDto>();
            for (int i = 0; i < tokens.Count; i++)
            {
                result.Add(MapToDtoSync(tokens[i], positionInQueue: i + 1));
            }
            return result;
        }

        public async Task<QueueTokenDto> GetTokenStatus(int queueId, string tokenNo, int userId)
        {
            await EnsureQueueMember(queueId, userId);
            var token = await _context.QueueTokens
                .Include(x => x.User)
                .Include(x => x.Counter)
                .FirstOrDefaultAsync(x => x.QueueId == queueId && x.TokenNo == tokenNo);

            if (token == null)
                throw new KeyNotFoundException($"Token '{tokenNo}' was not found.");

            int? position = null;
            if (token.Status == QueueStatus.Waiting)
            {
                position = await _context.QueueTokens
                    .Where(x => x.QueueId == queueId && x.Status == QueueStatus.Waiting && x.CreatedDate < token.CreatedDate)
                    .CountAsync() + 1;
            }

            return MapToDtoSync(token, position);
        }

        // --- helpers ---

        private async Task EnsureQueueMember(int queueId, int userId)
        {
            var queueExists = await _context.Queues.AnyAsync(x => x.Id == queueId);
            if (!queueExists)
                throw new KeyNotFoundException($"Queue with Id {queueId} was not found.");

            var isMember = await _context.QueueMemberships
                .AnyAsync(x => x.QueueId == queueId && x.UserId == userId);
            if (!isMember)
                throw new UnauthorizedAccessException("Join this queue before accessing it.");
        }

        private async Task EnsureQueueOwner(int queueId, int adminUserId)
        {
            var queue = await _context.Queues
                .Where(x => x.Id == queueId)
                .Select(x => new { x.AdminUserId })
                .FirstOrDefaultAsync();
            if (queue == null)
                throw new KeyNotFoundException($"Queue with Id {queueId} was not found.");
            if (queue.AdminUserId != adminUserId)
                throw new UnauthorizedAccessException("Only the queue owner can manage this queue.");
        }

        private async Task<QueueTokenDto> MapToDto(QueueToken token)
        {
            await _context.Entry(token).Reference(t => t.User).LoadAsync();
            if (token.CounterId.HasValue)
                await _context.Entry(token).Reference(t => t.Counter).LoadAsync();

            return MapToDtoSync(token, null);
        }

        private static QueueTokenDto MapToDtoSync(QueueToken token, int? positionInQueue)
        {
            return new QueueTokenDto(
                token.Id,
                token.TokenNo,
                token.Status.ToString(),
                token.User?.Name ?? "",
                token.CounterId,
                token.Counter?.CounterName,
                token.CreatedDate,
                token.CalledTime,
                token.CompletedTime,
                positionInQueue);
        }
    }
}
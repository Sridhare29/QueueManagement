using Microsoft.EntityFrameworkCore;
using QueueManagement.API.Data;
using QueueManagement.Application.DTOs;
using QueueManagement.Application.Interfaces;
using QueueManagement.Domain.Entities;
using QueueManagement.Domain.Enums;

namespace QueueManagement.Application.Services
{
    public class QueueService : IQueueService
    {
        private readonly AppDbContext _context;

        public QueueService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<QueueTokenDto> GenerateToken(GenerateTokenRequest request)
        {
            var user = new User
            {
                Name = request.Name,
                MobileNo = request.MobileNo
            };

            _context.Users.Add(user);

            const int maxAttempts = 5;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var tokenNumbers = await _context.QueueTokens
                    .Select(x => x.TokenNo)
                    .ToListAsync();

                var nextTokenNumber = tokenNumbers
                    .Select(tokenNo => int.TryParse(tokenNo.TrimStart('A'), out var number) ? number : 0)
                    .DefaultIfEmpty()
                    .Max() + 1;

                var token = new QueueToken
                {
                    User = user,
                    TokenNo = $"A{nextTokenNumber:000}",
                    CreatedDate = DateTime.Now,
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
                    // Unique constraint hit — someone else took this number. Retry.
                    _context.Entry(token).State = EntityState.Detached;
                }
            }

            throw new InvalidOperationException("Could not generate a unique token number, please retry.");
        }

        public async Task<QueueTokenDto?> CallNext(int counterId)
        {
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
                    .Where(x => x.Status == QueueStatus.Waiting)
                    .OrderBy(x => x.CreatedDate)
                    .FirstOrDefaultAsync();

                if (token == null)
                {
                    await tx.CommitAsync();
                    return null;
                }

                token.Status = QueueStatus.Serving;
                token.CounterId = counterId;
                token.CalledTime = DateTime.Now;

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

        public async Task<QueueTokenDto> CompleteToken(int tokenId)
        {
            var token = await _context.QueueTokens.FindAsync(tokenId);
            if (token == null)
                throw new KeyNotFoundException($"Token with Id {tokenId} was not found.");

            if (token.Status != QueueStatus.Serving)
                throw new InvalidOperationException(
                    $"Token {token.TokenNo} cannot be completed from status '{token.Status}'. It must be Serving.");

            token.Status = QueueStatus.Completed;
            token.CompletedTime = DateTime.Now;

            await _context.SaveChangesAsync();
            return await MapToDto(token);
        }

        public async Task<List<QueueTokenDto>> GetWaitingQueue()
        {
            var tokens = await _context.QueueTokens
                .Include(x => x.User)
                .Where(x => x.Status == QueueStatus.Waiting)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();

            var result = new List<QueueTokenDto>();
            for (int i = 0; i < tokens.Count; i++)
            {
                result.Add(MapToDtoSync(tokens[i], positionInQueue: i + 1));
            }
            return result;
        }

        public async Task<QueueTokenDto> GetTokenStatus(int tokenId)
        {
            var token = await _context.QueueTokens
                .Include(x => x.User)
                .Include(x => x.Counter)
                .FirstOrDefaultAsync(x => x.Id == tokenId);

            if (token == null)
                throw new KeyNotFoundException($"Token with Id {tokenId} was not found.");

            int? position = null;
            if (token.Status == QueueStatus.Waiting)
            {
                position = await _context.QueueTokens
                    .Where(x => x.Status == QueueStatus.Waiting && x.CreatedDate < token.CreatedDate)
                    .CountAsync() + 1;
            }

            return MapToDtoSync(token, position);
        }

        // --- helpers ---

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
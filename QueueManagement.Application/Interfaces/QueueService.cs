using Microsoft.EntityFrameworkCore;
using QueueManagement.API.Data;
using QueueManagement.Domain.Entities;
using QueueManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManagement.Application.Interfaces
{
    public class QueueService : IQueueService
    {
        private readonly AppDbContext _context;

        public QueueService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<QueueToken> GenerateToken(int userId)
        {
            var todayCount = await _context.QueueTokens
                .CountAsync(x => x.CreatedDate.Date == DateTime.Today);

            string tokenNo = $"A{(todayCount + 1):000}";

            var token = new QueueToken
            {
                UserId = userId,
                TokenNo = tokenNo,
                CreatedDate = DateTime.Now,
                Status = QueueStatus.Waiting
            };

            _context.QueueTokens.Add(token);

            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<QueueToken?> CallNext(int counterId)
        {
            // 1. Check whether counter exists
            var counter = await _context.Counters
                .FirstOrDefaultAsync(x => x.Id == counterId);

            if (counter == null)
            {
                throw new KeyNotFoundException(
                    $"Counter with Id {counterId} was not found.");
            }

            // 2. Check whether counter is active
            if (!counter.IsActive)
            {
                throw new InvalidOperationException(
                    $"Counter {counter.CounterName} is inactive.");
            }

            // 3. Find the first waiting token
            var token = await _context.QueueTokens
                .Where(x => x.Status == QueueStatus.Waiting)
                .OrderBy(x => x.CreatedDate)
                .FirstOrDefaultAsync();

            // 4. No waiting customer
            if (token == null)
            {
                return null;
            }

            // 5. Assign token to counter
            token.Status = QueueStatus.Serving;
            token.CounterId = counterId;
            token.CalledTime = DateTime.Now;

            // 6. Save
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task CompleteToken(int tokenId)
        {
            var token = await _context.QueueTokens.FindAsync(tokenId);

            if (token == null)
                return;

            token.Status = QueueStatus.Completed;
            token.CompletedTime = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task<List<QueueToken>> GetWaitingQueue()
        {
            return await _context.QueueTokens
                .Where(x => x.Status == QueueStatus.Waiting)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();
        }
    }
    }

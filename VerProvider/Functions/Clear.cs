using System;
using Data.Contexts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace VerProvider.Functions
{
    public class Clear
    {
        private readonly ILogger _logger;
        private readonly DataContext _context;

        public Clear(ILoggerFactory loggerFactory, DataContext context)
        {
            _logger = loggerFactory.CreateLogger<Clear>();
            _context = context;
        }

        [Function("Clear")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        {
            var records = await _context.incomingRequests.Where(x => x.ExpiryDate <= DateTime.Now).ToListAsync();
            _context.RemoveRange(records);
            await _context.SaveChangesAsync();
        }
    }
}

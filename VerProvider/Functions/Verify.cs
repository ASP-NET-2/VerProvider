using Azure.Messaging.ServiceBus;
using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerProvider.Models;

namespace VerProvider.Functions
{
    public class Verify
    {
        private readonly ILogger<Verify> _logger;
        private readonly DataContext _context;

        public Verify(ILogger<Verify> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("Verify")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var ir = JsonConvert.DeserializeObject<VerificationRequest>(body);

                if (ir != null && !string.IsNullOrEmpty(ir.Code))
                {
                    var result = await _context.incomingRequests.FirstOrDefaultAsync(x => x.Email == ir.Email && x.Code == ir.Code);
                    if (result != null)
                    {
                        _context.Remove(result);
                        await _context.SaveChangesAsync();
                        return new OkResult();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Verify ::" + ex.Message);
            }
            return new UnauthorizedResult();
        }
    }
}

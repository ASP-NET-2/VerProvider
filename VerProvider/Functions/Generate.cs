using Azure.Messaging.ServiceBus;
using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerProvider.Models;

namespace VerProvider.Functions
{
    public class Generate
    {
        private readonly ILogger<Generate> _logger;
        private readonly DataContext _context;

        public Generate(ILogger<Generate> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("Generate")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var ir = JsonConvert.DeserializeObject<VerificationRequest>(body);

                if (ir != null)
                {
                    var rnd = new Random();
                    var code = rnd.Next(100000, 999999);

                    var entity = new incomingRequests
                    {
                        Email = ir.Email,
                        Code = code.ToString(),
                    };

                    _context.Add(entity);
                    await _context.SaveChangesAsync();

                    var client = new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBus"));
                    var sender = client.CreateSender("email_request");

                    var message = new ServiceBusMessage(JsonConvert.SerializeObject(ir))
                    {
                        ContentType = "application/json"
                    };

                    await sender.SendMessageAsync(message);
                    return new OkResult();
                }            
            }
            catch (Exception ex)
            {
                _logger.LogError($"Generate ::" + ex.Message);
            }
            return new BadRequestResult();
        }
    }
}

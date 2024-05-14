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
                    var code = rnd.Next(10000, 99999);

                    var entity = new incomingRequests
                    {
                        Email = ir.Email,
                        Code = code.ToString(),
                    };

                    _context.Add(entity);
                    await _context.SaveChangesAsync();

                    var client = new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBus"));
                    var sender = client.CreateSender("email_request");


                    var emailRequest = new EmailRequest()
                    {
                        To = ir.Email,
                        Subject = $"Verification Code {code}",
                        HtmlBody = $@"
                        <!DOCTYPE html>
                        <html lang='en'>
                        <head>
                            <meta charset='UTF-8'>
                            <title>Verification Code</title>
                        </head>
                        <body>
                            <div style='font-family: Arial, sans-serif; color: #333;'>
                                <h2>Verification Code</h2>
                                <p>Dear User,</p>
                                <p>Your verification code is: <strong style='font-size: 24px;'>{code}</strong></p>
                                <p>If you did not request this code, please ignore this email or contact support.</p>
                                <p>Best Regards,<br>Manero</p>
                            </div>
                        </body>
                        </html>",
                        PlainText = $"Please verify your account using this verification code: {code}. If you did not request this code, please ignore this email or contact support."
                    };

                    var message = new ServiceBusMessage(JsonConvert.SerializeObject(emailRequest))
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

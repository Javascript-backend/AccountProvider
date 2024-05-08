using AccountProvider.Models;
using Azure.Messaging.ServiceBus;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;

namespace AccountProvider.Functions
{
    public class SignUp(ILogger<SignUp> logger, UserManager<UserAccount> userManager)
    {
        private readonly ILogger<SignUp> _logger = logger;
        private readonly UserManager<UserAccount> _userManager = userManager;
        private Encoding? encoding;

        [Function("SignUp")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function,"post")] HttpRequest req)
        {
            string body = null!;
            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError($"StreamReader :: {ex.Message}");
            }
            
            

            if(body != null)
            {
                UserRegistrationRequest urr = null!;
                try
                {
                    urr = JsonConvert.DeserializeObject<UserRegistrationRequest>(body)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"JsonConvert.DeserializeObject<UserRegistrationRequest>(body) :: {ex.Message}");
                }
                

                if (urr != null && !string.IsNullOrEmpty(urr.Email) && !string.IsNullOrEmpty(urr.Password))
                {
                    if(! await _userManager.Users.AnyAsync(x => x.Email == urr.Email))
                    {
                        var userAccount = new UserAccount
                        {
                            FirstName = urr.FirstName,
                            LastName = urr.LastName,
                            Email = urr.Email,
                            UserName = urr.Email
                        };

                        string sbc = Environment.GetEnvironmentVariable("ServiceBus")!;
                        string queue = Environment.GetEnvironmentVariable("Queue")!;

                        var result = await _userManager.CreateAsync(userAccount, urr.Password);
                        if (result.Succeeded)
                        {
                            try
                            {
                                    
                                //ServiceBusClient client = new ServiceBusClient(sbc);
                                //ServiceBusSender sender = client.CreateSender(queue);

                                    
                                //var messageBody = JsonConvert.SerializeObject(new { Email = userAccount.Email });
                                //ServiceBusMessage message = new ServiceBusMessage(messageBody);

                                    
                                //await sender.SendMessageAsync(message);

                                //return new OkResult();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Failed to send message to Service Bus: {ex.Message}");
                            }
                        }
                        
                    }
                    else
                    {
                        return new ConflictResult();    
                    }
                }


            }
            return new BadRequestResult();
        }
    }
}

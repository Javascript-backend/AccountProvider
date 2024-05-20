using AccountProvider.Models;
using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountProvider.Functions;

public class GetAddress(ILogger<GetAddress> logger, IServiceProvider serviceProvider)
{
    private readonly ILogger<GetAddress> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [Function("GetAddress")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string body = null!;
        try
        {
            body = await new StreamReader(req.Body).ReadToEndAsync();

        }
        catch (Exception ex)
        {
            _logger.LogError($"StreamReader :: {ex.Message}");
        }

        if (body != null)
        {
            try
            {
                var requestBody = JsonConvert.DeserializeObject<GetAddressRequest>(body)!;

                if (requestBody.UserId != null)
                {
                    using var context = _serviceProvider.GetRequiredService<DataContext>();

                    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == requestBody.UserId);

                    if(user != null) 
                    {
                        var address = await context.UserAddresses.FirstOrDefaultAsync(x => x.Id == user.AddressId);

                        if(address != null)
                        {
                            var addressModel = new AddressModel
                            {
                                AddressLine_1 = address.AddressLine_1!,
                                AddressLine_2 = address.AddressLine_2!,
                                PostalCode = address.PostalCode!,
                                City = address.City!              
                            };

                            return new OkObjectResult(addressModel);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError($"JsonConvert.DeserializeObject<String>(body) :: {ex.Message}");
            }
        }

        return new BadRequestResult();
    }
}

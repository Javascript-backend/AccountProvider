using AccountProvider.Models;
using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountProvider.Functions;

public class UpdateAddress(ILogger<UpdateAddress> logger, IServiceProvider serviceProvider, UserManager<UserAccount> userManager)
{
    private readonly ILogger<UpdateAddress> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly UserManager<UserAccount> _userManager = userManager;

[Function("UpdateAddress")]
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
            AddressModel model = null!;
            try
            {
                model = JsonConvert.DeserializeObject<AddressModel>(body)!;

                if (model != null)
                {
                    using var context = _serviceProvider.GetRequiredService<DataContext>();

                    var exists = await context.UserAddresses.FirstOrDefaultAsync(x => x.AddressLine_1 == model.AddressLine_1 && x.PostalCode == model.PostalCode && x.City == model.City);
                    if (exists != null)
                    {
                        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == model.UserId);

                        if(user != null)
                        {
                            user.AddressId = exists.Id;

                            var updateResult = await userManager.UpdateAsync(user);

                            if(updateResult.Succeeded)
                            {
                                return new OkObjectResult(model);
                            }
                
                        }
                    }

                    var addressEntity = new UserAddress
                    {
                        AddressLine_1 = model.AddressLine_1,
                        AddressLine_2 = model.AddressLine_2,
                        PostalCode = model.PostalCode,
                        City = model.City
                    };

                    var createAddress = context.UserAddresses.Add(addressEntity);
                    await context.SaveChangesAsync();

                    if (createAddress != null)
                    {
                        var findUser = await context.Users.FirstOrDefaultAsync(x => x.Id == model.UserId);

                        if (findUser != null)
                        {
                            findUser.AddressId = addressEntity.Id;

                            var updateResult = await userManager.UpdateAsync(findUser);

                            if (updateResult.Succeeded)
                            {
                                return new OkObjectResult(model);
                            }

                        }

                    }



                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"JsonConvert.DeserializeObject<AddressModel>(body) :: {ex.Message}");
            }

        }

        return new BadRequestResult();
    }

}

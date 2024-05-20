using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class UserAccount : IdentityUser
{
    public string? FirstName { get; set; }

    public string? LastName { get; set;}

    public string? AddressId { get; set; }

    public string? Biography { get; set; }

    public UserAddress? Address { get; set; }
}

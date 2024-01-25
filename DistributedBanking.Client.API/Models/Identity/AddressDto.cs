using System.ComponentModel.DataAnnotations;

namespace DistributedBanking.API.Models.Identity;

public class AddressDto
{
    [Required]
    public required string Country { get; set; }
    
    [Required]
    public required string Region { get; set; }
    
    [Required]
    public required string City { get; set; }
    
    [Required]
    public required string Street { get; set; }
    
    [Required]
    public required string Building { get; set; }
    
    [Required]
    public required string PostalCode { get; set; }
}
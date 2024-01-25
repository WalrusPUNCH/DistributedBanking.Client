using System.ComponentModel.DataAnnotations;

namespace DistributedBanking.API.Models.Transaction;

public class TwoWayTransactionDto 
{
    [Required]
    public string SourceAccountId { get; set; }

    [Required]
    public required string SourceAccountSecurityCode { get; set; }
    
    [Required]
    public string  DestinationAccountId { get; set; }
    
    [Required, Range(0, double.MaxValue, ErrorMessage = "Value should be greater than 0")]
    public decimal Amount { get; set; }
    
    public string? Description { get; set; }
}
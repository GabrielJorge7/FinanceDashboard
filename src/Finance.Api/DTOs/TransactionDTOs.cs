using System.ComponentModel.DataAnnotations;
using Finance.Api.Models;

namespace Finance.Api.DTOs;

public class TransactionDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string TypeName => Type.ToString();
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CategoryId { get; set; }
    public CategoryDto Category { get; set; } = null!;
}

public class CreateTransactionDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    public TransactionType Type { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
}

public class UpdateTransactionDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    public TransactionType Type { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
}

public class TransactionSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
    public int TransactionCount { get; set; }
}

public class MonthlyFlowDto
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Balance { get; set; }
}

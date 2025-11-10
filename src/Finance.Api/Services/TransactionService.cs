using Microsoft.EntityFrameworkCore;
using Finance.Api.Data;
using Finance.Api.Models;
using Finance.Api.DTOs;

namespace Finance.Api.Services;

public interface ITransactionService
{
    Task<IEnumerable<TransactionDto>> GetTransactionsAsync(string userId, int page = 1, int pageSize = 10);
    Task<TransactionDto?> GetTransactionAsync(int id, string userId);
    Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto, string userId);
    Task<TransactionDto?> UpdateTransactionAsync(int id, UpdateTransactionDto updateTransactionDto, string userId);
    Task<bool> DeleteTransactionAsync(int id, string userId);
    Task<TransactionSummaryDto> GetSummaryAsync(string userId);
    Task<IEnumerable<MonthlyFlowDto>> GetMonthlyFlowAsync(string userId, int year);
}

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _context;
    
    public TransactionService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<TransactionDto>> GetTransactionsAsync(string userId, int page = 1, int pageSize = 10)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Description = t.Description,
                Amount = t.Amount,
                Type = t.Type,
                Date = t.Date,
                Notes = t.Notes,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CategoryId = t.CategoryId,
                Category = new CategoryDto
                {
                    Id = t.Category.Id,
                    Name = t.Category.Name,
                    Description = t.Category.Description,
                    Color = t.Category.Color,
                    CreatedAt = t.Category.CreatedAt,
                    UpdatedAt = t.Category.UpdatedAt
                }
            })
            .ToListAsync();
            
        return transactions;
    }
    
    public async Task<TransactionDto?> GetTransactionAsync(int id, string userId)
    {
        var transaction = await _context.Transactions
            .Where(t => t.Id == id && t.UserId == userId)
            .Include(t => t.Category)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Description = t.Description,
                Amount = t.Amount,
                Type = t.Type,
                Date = t.Date,
                Notes = t.Notes,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CategoryId = t.CategoryId,
                Category = new CategoryDto
                {
                    Id = t.Category.Id,
                    Name = t.Category.Name,
                    Description = t.Category.Description,
                    Color = t.Category.Color,
                    CreatedAt = t.Category.CreatedAt,
                    UpdatedAt = t.Category.UpdatedAt
                }
            })
            .FirstOrDefaultAsync();
            
        return transaction;
    }
    
    public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto, string userId)
    {
        // Validate category belongs to user
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == createTransactionDto.CategoryId && c.UserId == userId);
            
        if (!categoryExists)
            throw new ArgumentException("Category not found or doesn't belong to user");
            
        // Garantir que a data seja UTC para compatibilidade com timestamptz no PostgreSQL
        var normalizedDate = createTransactionDto.Date.Kind == DateTimeKind.Utc
            ? createTransactionDto.Date
            : DateTime.SpecifyKind(createTransactionDto.Date, DateTimeKind.Utc);
            
        var transaction = new Transaction
        {
            Description = createTransactionDto.Description,
            Amount = createTransactionDto.Amount,
            Type = createTransactionDto.Type,
            Date = normalizedDate,
            Notes = createTransactionDto.Notes,
            CategoryId = createTransactionDto.CategoryId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        
        // Load the category for the response
        var category = await _context.Categories.FindAsync(createTransactionDto.CategoryId);
        
        return new TransactionDto
        {
            Id = transaction.Id,
            Description = transaction.Description,
            Amount = transaction.Amount,
            Type = transaction.Type,
            Date = transaction.Date,
            Notes = transaction.Notes,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt,
            CategoryId = transaction.CategoryId,
            Category = new CategoryDto
            {
                Id = category!.Id,
                Name = category.Name,
                Description = category.Description,
                Color = category.Color,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            }
        };
    }
    
    public async Task<TransactionDto?> UpdateTransactionAsync(int id, UpdateTransactionDto updateTransactionDto, string userId)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            
        if (transaction == null)
            return null;
            
        // Validate category belongs to user
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == updateTransactionDto.CategoryId && c.UserId == userId);
            
        if (!categoryExists)
            throw new ArgumentException("Category not found or doesn't belong to user");
            
        // Garantir que a data seja UTC para compatibilidade com timestamptz no PostgreSQL
        var normalizedDate = updateTransactionDto.Date.Kind == DateTimeKind.Utc
            ? updateTransactionDto.Date
            : DateTime.SpecifyKind(updateTransactionDto.Date, DateTimeKind.Utc);
            
        transaction.Description = updateTransactionDto.Description;
        transaction.Amount = updateTransactionDto.Amount;
        transaction.Type = updateTransactionDto.Type;
        transaction.Date = normalizedDate;
        transaction.Notes = updateTransactionDto.Notes;
        transaction.CategoryId = updateTransactionDto.CategoryId;
        transaction.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        // Reload category if changed
        if (transaction.CategoryId != updateTransactionDto.CategoryId)
        {
            transaction.Category = await _context.Categories.FindAsync(updateTransactionDto.CategoryId);
        }
        
        return new TransactionDto
        {
            Id = transaction.Id,
            Description = transaction.Description,
            Amount = transaction.Amount,
            Type = transaction.Type,
            Date = transaction.Date,
            Notes = transaction.Notes,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt,
            CategoryId = transaction.CategoryId,
            Category = new CategoryDto
            {
                Id = transaction.Category!.Id,
                Name = transaction.Category.Name,
                Description = transaction.Category.Description,
                Color = transaction.Category.Color,
                CreatedAt = transaction.Category.CreatedAt,
                UpdatedAt = transaction.Category.UpdatedAt
            }
        };
    }
    
    public async Task<bool> DeleteTransactionAsync(int id, string userId)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            
        if (transaction == null)
            return false;
            
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<TransactionSummaryDto> GetSummaryAsync(string userId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .GroupBy(t => 1)
            .Select(g => new TransactionSummaryDto
            {
                TotalIncome = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                TotalExpense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .FirstOrDefaultAsync();
            
        if (transactions == null)
        {
            return new TransactionSummaryDto
            {
                TotalIncome = 0,
                TotalExpense = 0,
                Balance = 0,
                TransactionCount = 0
            };
        }
        
        transactions.Balance = transactions.TotalIncome - transactions.TotalExpense;
        return transactions;
    }
    
    public async Task<IEnumerable<MonthlyFlowDto>> GetMonthlyFlowAsync(string userId, int year)
    {
        // Primeiro, projetar dados numéricos traduzíveis para SQL
        var monthly = await _context.Transactions
            .Where(t => t.UserId == userId && t.Date.Year == year)
            .GroupBy(t => new { Year = t.Date.Year, Month = t.Date.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToListAsync();
        
        // Converter para DTO aplicando formatação do mês e cálculo do saldo em memória
        var result = monthly.Select(m => new MonthlyFlowDto
        {
            Month = m.Month.ToString("D2"),
            Year = m.Year,
            Income = m.Income,
            Expense = m.Expense,
            Balance = m.Income - m.Expense
        }).ToList();
        
        return result;
    }
}

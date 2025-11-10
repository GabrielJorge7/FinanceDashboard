using Microsoft.EntityFrameworkCore;
using Finance.Api.Data;
using Finance.Api.Models;
using Finance.Api.DTOs;

namespace Finance.Api.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync(string userId);
    Task<CategoryDto?> GetCategoryAsync(int id, string userId);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, string userId);
    Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto, string userId);
    Task<bool> DeleteCategoryAsync(int id, string userId);
}

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    
    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync(string userId)
    {
        var categories = await _context.Categories
            .Where(c => c.UserId == userId)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Color = c.Color,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                TransactionCount = c.Transactions.Count,
                TotalAmount = c.Transactions.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount)
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
            
        return categories;
    }
    
    public async Task<CategoryDto?> GetCategoryAsync(int id, string userId)
    {
        var category = await _context.Categories
            .Where(c => c.Id == id && c.UserId == userId)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Color = c.Color,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                TransactionCount = c.Transactions.Count,
                TotalAmount = c.Transactions.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount)
            })
            .FirstOrDefaultAsync();
            
        return category;
    }
    
    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, string userId)
    {
        var category = new Category
        {
            Name = createCategoryDto.Name,
            Description = createCategoryDto.Description,
            Color = createCategoryDto.Color,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            TransactionCount = 0,
            TotalAmount = 0
        };
    }
    
    public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto, string userId)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            
        if (category == null)
            return null;
            
        category.Name = updateCategoryDto.Name;
        category.Description = updateCategoryDto.Description;
        category.Color = updateCategoryDto.Color;
        category.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            TransactionCount = category.Transactions.Count,
            TotalAmount = category.Transactions.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount)
        };
    }
    
    public async Task<bool> DeleteCategoryAsync(int id, string userId)
    {
        var category = await _context.Categories
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            
        if (category == null)
            return false;
            
        if (category.Transactions.Any())
            throw new InvalidOperationException("Cannot delete category with existing transactions");
            
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        
        return true;
    }
}

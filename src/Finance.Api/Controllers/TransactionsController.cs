using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Finance.Api.DTOs;
using Finance.Api.Services;

namespace Finance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    
    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }
    
    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var transactions = await _transactionService.GetTransactionsAsync(GetUserId(), page, pageSize);
        return Ok(transactions);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionDto>> GetTransaction(int id)
    {
        var transaction = await _transactionService.GetTransactionAsync(id, GetUserId());
        
        if (transaction == null)
        {
            return NotFound();
        }
        
        return Ok(transaction);
    }
    
    [HttpPost]
    public async Task<ActionResult<TransactionDto>> CreateTransaction(CreateTransactionDto createTransactionDto)
    {
        try
        {
            var transaction = await _transactionService.CreateTransactionAsync(createTransactionDto, GetUserId());
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<TransactionDto>> UpdateTransaction(int id, UpdateTransactionDto updateTransactionDto)
    {
        try
        {
            var transaction = await _transactionService.UpdateTransactionAsync(id, updateTransactionDto, GetUserId());
            
            if (transaction == null)
            {
                return NotFound();
            }
            
            return Ok(transaction);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        try
        {
            var result = await _transactionService.DeleteTransactionAsync(id, GetUserId());
            
            if (!result)
            {
                return NotFound();
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("summary")]
    public async Task<ActionResult<TransactionSummaryDto>> GetSummary()
    {
        var summary = await _transactionService.GetSummaryAsync(GetUserId());
        return Ok(summary);
    }
    
    [HttpGet("monthly-flow")]
    public async Task<ActionResult<IEnumerable<MonthlyFlowDto>>> GetMonthlyFlow([FromQuery] int year = 0)
    {
        if (year == 0)
            year = DateTime.Now.Year;
            
        var monthlyFlow = await _transactionService.GetMonthlyFlowAsync(GetUserId(), year);
        return Ok(monthlyFlow);
    }
}

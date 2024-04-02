using Contracts.Models;
using DistributedBanking.API.Controllers.Identity;
using DistributedBanking.API.Filters;
using DistributedBanking.API.Models;
using DistributedBanking.API.Models.Transaction;
using DistributedBanking.Client.Domain.Models.Transaction;
using DistributedBanking.Client.Domain.Services;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Entities.Constants;
using System.Globalization;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace DistributedBanking.API.Controllers;

[ApiController]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[Route("api/transaction")]
public class TransactionController : CustomControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionController> _logger;
    
    public TransactionController(
        ITransactionService transactionService,
        ILogger<TransactionController> logger) : base(logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    [HttpPost("deposit")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Customer)]
    [TypeFilter(typeof(UserAccountCheckingActionFilterAttribute))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deposit(OneWayTransactionDto depositTransactionDto)
    {
        _logger.LogError("received deposit at {DateTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
        var depositOperation = await _transactionService.Deposit(depositTransactionDto.Adapt<OneWayTransactionModel>());
        
        return HandleOperationResult(depositOperation);
    }
    
    [HttpPost("withdraw")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Customer)]
    [TypeFilter(typeof(UserAccountCheckingActionFilterAttribute))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Withdraw(OneWaySecuredTransactionDto withdrawalTransactionDto)
    {
        var withdrawalOperation = await _transactionService.Withdraw(withdrawalTransactionDto.Adapt<OneWaySecuredTransactionModel>());
        
        return HandleOperationResult(withdrawalOperation);
    }
    
    [HttpPost("transfer")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Customer)]
    [TypeFilter(typeof(UserAccountCheckingActionFilterAttribute))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transfer(TwoWayTransactionDto transferTransactionDto)
    {
        var transferOperation = await _transactionService.Transfer(transferTransactionDto.Adapt<TwoWayTransactionModel>());
        
        return HandleOperationResult(transferOperation);
    }
    
    [HttpGet("history/{accountId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{RoleNames.Customer}, {RoleNames.Administrator}")]
    [ProducesResponseType(typeof(IEnumerable<TransactionResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AccountHistory(string accountId)
    {
        var history = await _transactionService.GetAccountTransactionHistory(accountId);
        
        return Ok(new Response<IEnumerable<TransactionResponseModel>>(OperationStatus.Success, string.Empty, history));
    }
}
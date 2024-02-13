using AutoWrapper.Extensions;
using AutoWrapper.Wrappers;
using Contracts.Extensions;
using Contracts.Models;
using DistributedBanking.API.Controllers.Identity;
using DistributedBanking.API.Filters;
using DistributedBanking.API.Models;
using DistributedBanking.API.Models.Account;
using DistributedBanking.Client.Domain.Models.Account;
using DistributedBanking.Client.Domain.Services;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Entities.Constants;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace DistributedBanking.API.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Customer)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[Route("api/account")]
public class AccountController : CustomControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAccountService accountService, ILogger<AccountController> logger) : base(logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountOwnedResponseModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAccount(AccountCreationDto accountDto)
    {
        var userId = User.Id();
        var accountCreationResult = await _accountService.CreateAsync(userId, accountDto.Adapt<AccountCreationModel>());

        return HandleOperationResult(accountCreationResult);
    }
    
    [HttpGet] //todo remove
    [ProducesResponseType(typeof(IEnumerable<AccountOwnedResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAccounts()
    {
        var items = await _accountService.GetAsync();
        
        return Ok(new Response<IEnumerable<AccountOwnedResponseModel>>(OperationStatus.Success, string.Empty, items));
    }
    
    [HttpGet("{id}")] //todo remove
    [ProducesResponseType(typeof(AccountOwnedResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccount(string id)
    {
        var item = await _accountService.GetAsync(id);
        
        return Ok(new Response<AccountOwnedResponseModel>(OperationStatus.Success, string.Empty, item));
    }
    
    [HttpGet("owned/{ownerId}")] //todo remove
    [ProducesResponseType(typeof(IEnumerable<AccountResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerAccounts(string ownerId)
    {
        var items = await _accountService.GetCustomerAccountsAsync(ownerId);
        
        return Ok(new Response<IEnumerable<AccountResponseModel>>(OperationStatus.Success, string.Empty, items));
    }
    
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<AccountResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerAccounts()
    {        
        var userId = User.Id();
        var items = await _accountService.GetCustomerAccountsAsync(userId);
        
        return Ok(new Response<IEnumerable<AccountResponseModel>>(OperationStatus.Success, string.Empty, items));
    }
    
    [HttpDelete("{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [TypeFilter(typeof(UserAccountCheckingActionFilterAttribute))]
    public async Task<IActionResult> DeleteAccount(string accountId)
    {
        var accountDeletionResult = await _accountService.DeleteAsync(accountId);

        return HandleOperationResult(accountDeletionResult);
    }
    
    private IActionResult HandleOperationResult(OperationResult<AccountOwnedResponseModel> operationResult)
    {
        switch (operationResult.Status)
        {
            case OperationStatus.Success:
                return Created(
                    operationResult.Response!.Id,
                    new Response<AccountOwnedResponseModel>(operationResult.Status, operationResult.Message,
                        operationResult.Response!));
            case OperationStatus.Processing:
                return Ok(new Response(operationResult.Status, operationResult.Message));
            case OperationStatus.BadRequest:
            {
                foreach (var error in operationResult.Messages)
                {
                    ModelState.AddModelError("", error);
                }
        
                _logger.LogInformation("Operation has ended unsuccessfully. Details: {Result}", operationResult.ToString());
                throw new ApiException(ModelState.AllErrors());
                //return BadRequest(new Response(operationResult.Status, operationResult.Message));
            }
            case OperationStatus.InternalFail:
                return StatusCode(StatusCodes.Status500InternalServerError, operationResult.Message);
            default:
                throw new ArgumentOutOfRangeException(nameof(operationResult.Status), operationResult.Status, "Operation status is out of range");
        }
    }
}
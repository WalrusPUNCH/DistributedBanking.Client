using Contracts.Extensions;
using DistributedBanking.API.Filters;
using DistributedBanking.API.Models.Account;
using DistributedBanking.Client.Domain.Models.Account;
using DistributedBanking.Client.Domain.Services;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Entities.Constants;

namespace DistributedBanking.API.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Customer)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountOwnedResponseModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAccount(AccountCreationDto accountDto)
    {
        var userId = User.Id();
        var createdAccount = await _accountService.CreateAsync(userId, accountDto.Adapt<AccountCreationModel>());
        return createdAccount ? Ok() : BadRequest();
        
        //return Created(createdAccount.Id, createdAccount);
    }
    
    [HttpGet] //todo remove
    [ProducesResponseType(typeof(IEnumerable<AccountOwnedResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAccounts()
    {
        var items = await _accountService.GetAsync();
        
        return Ok(items);
    }
    
    [HttpGet("owned/{ownerId}")] //todo remove
    [ProducesResponseType(typeof(IEnumerable<AccountResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerAccounts(string ownerId)
    {
        var items = await _accountService.GetCustomerAccountsAsync(ownerId);
        
        return Ok(items);
    }
    
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<AccountResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerAccounts()
    {        
        var userId = User.Id();
        var items = await _accountService.GetCustomerAccountsAsync(userId);
        
        return Ok(items);
    }
   
    [HttpGet("{id}")] //todo remove
    [ProducesResponseType(typeof(AccountOwnedResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccount(string id)
    {
        var item = await _accountService.GetAsync(id);
        
        return Ok(item);
    }
    
    [HttpDelete("{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [TypeFilter(typeof(UserAccountCheckingActionFilterAttribute))]
    public async Task<IActionResult> DeleteAccount(string accountId)
    {
        var accountDeletionResult = await _accountService.DeleteAsync(accountId);
        
        return accountDeletionResult ? Ok() : BadRequest();
    }
}
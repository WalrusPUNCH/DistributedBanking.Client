using AutoWrapper.Extensions;
using AutoWrapper.Wrappers;
using Contracts.Extensions;
using DistributedBanking.API.Models.Identity;
using DistributedBanking.Client.Domain.Models.Identity;
using DistributedBanking.Client.Domain.Services;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Entities.Constants;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace DistributedBanking.API.Controllers.Identity;

[ApiController]
[Route("api/identity")]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class IdentityController : IdentityControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<IdentityController> _logger;

    public IdentityController(
        IIdentityService identityService,
        ILogger<IdentityController> logger)  : base(logger)
    {
        _identityService = identityService;
        _logger = logger;
    }
    
    [HttpPost("register/customer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterCustomer(EndUserRegistrationDto registrationDto)
    {
        var customerRegistrationResult = await _identityService.RegisterCustomer(registrationDto.Adapt<EndUserRegistrationModel>());
        
        return HandleUserManagerResult(customerRegistrationResult);
    }
    
    [HttpPost("register/worker")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Administrator)]
    public async Task<IActionResult> RegisterWorker(WorkerRegistrationDto registrationDto)
    {
        var workerRegistrationResult = await _identityService.RegisterWorker(registrationDto.Adapt<WorkerRegistrationModel>(), RoleNames.Worker);
        
        return HandleUserManagerResult(workerRegistrationResult);
    }
    
    [HttpPost("register/admin")] //todo remove
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Administrator)]
    public async Task<IActionResult> RegisterAdmin(WorkerRegistrationDto registrationDto)
    {
        var adminRegistrationResult = await _identityService.RegisterWorker(registrationDto.Adapt<WorkerRegistrationModel>(), RoleNames.Administrator);
        
        return HandleUserManagerResult(adminRegistrationResult);
    }
    
    [HttpPost("login")]
    [ProducesResponseType(typeof(JwtTokenDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var loginResult = await _identityService.Login(loginDto.Adapt<LoginModel>());
        if (!loginResult.LoginResult.Succeeded)
        {
            ModelState.AddModelError(nameof(loginDto.Email), "Login Failed: invalid Email or Password");
            throw new ApiException(ModelState.AllErrors());
        }

        return Ok(new JwtTokenDto {Token = loginResult.Token!});
    }
    
    [HttpGet("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        await _identityService.Logout();
        return Ok();
    }
    
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Customer)]
    public async Task<IActionResult> Delete()
    {
        var userEmail = User.Email();
        var userDeletionResult = await _identityService.DeleteUser(userEmail);
        
        return HandleUserManagerResult(userDeletionResult);
    }

    [HttpPost("customer/update_passport")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Customer)]
    public async Task<IActionResult> UpdateCustomerPersonalInformation(CustomerPassportDto customerPassportDto)
    {
        var customerId = User.Id();
        var informationUpdateResult = await _identityService.UpdateCustomerPersonalInformation(customerId, customerPassportDto.Adapt<CustomerPassportModel>());

        return HandleUserManagerResult(informationUpdateResult);
    }
}
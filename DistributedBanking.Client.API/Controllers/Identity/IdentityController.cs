using AutoWrapper.Extensions;
using AutoWrapper.Wrappers;
using Contracts.Extensions;
using Contracts.Models;
using DistributedBanking.API.Models;
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
public class IdentityController : CustomControllerBase
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
        
        return HandleOperationResult(customerRegistrationResult);
    }
    
    [HttpPost("register/worker")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Administrator)]
    public async Task<IActionResult> RegisterWorker(WorkerRegistrationDto registrationDto)
    {
        var workerRegistrationResult = await _identityService.RegisterWorker(registrationDto.Adapt<WorkerRegistrationModel>(), RoleNames.Worker);
        
        return HandleOperationResult(workerRegistrationResult);
    }
    
    [HttpPost("register/admin")] //todo remove
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Administrator)]
    public async Task<IActionResult> RegisterAdmin(WorkerRegistrationDto registrationDto)
    {
        var adminRegistrationResult = await _identityService.RegisterWorker(registrationDto.Adapt<WorkerRegistrationModel>(), RoleNames.Administrator);

        return HandleOperationResult(adminRegistrationResult);
    }
    
    [HttpPost("login")]
    [ProducesResponseType(typeof(JwtTokenDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var loginOperationResult = await _identityService.Login(loginDto.Adapt<LoginModel>());
        if (loginOperationResult.Status != OperationStatus.Success)
        {
            ModelState.AddModelError(nameof(loginDto.Email), "Invalid email or password");
            throw new ApiException(ModelState.AllErrors());
        }

        return Ok(new Response<JwtTokenDto>(loginOperationResult.Status,
            loginOperationResult.Message,
            new JwtTokenDto { Token = loginOperationResult.Response!.Token, IsAdmin = loginOperationResult.Response.IsAdmin}));
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Delete()
    {
        var userEmail = User.Email();
        var userDeletionResult = await _identityService.DeleteUser(userEmail);
        
        return HandleOperationResult(userDeletionResult);
    }
    
    [HttpGet("customer/information")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Customer)]
    public async Task<IActionResult> CustomerIdentityInformation()
    {
        var customerId = User.Id();
        var identityInformationResult = await _identityService.CustomerIdentityInformation(customerId);

        return Ok(new Response<ShortUserModelDto>(identityInformationResult.Status,
            identityInformationResult.Message,
            identityInformationResult.Response?.Adapt<ShortUserModelDto>()));
    }
    
    [HttpGet("worker/information")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{RoleNames.Administrator}, {RoleNames.Worker}")]
    public async Task<IActionResult> WorkerIdentityInformation()
    {
        var workerId = User.Id();
        var identityInformationResult = await _identityService.WorkerIdentityInformation(workerId);

        return Ok(new Response<ShortWorkerModelDto>(identityInformationResult.Status,
            identityInformationResult.Message,
            identityInformationResult.Response?.Adapt<ShortWorkerModelDto>()));
    }

    [HttpPost("customer/update_passport")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Customer)]
    public async Task<IActionResult> UpdateCustomerPersonalInformation(CustomerPassportDto customerPassportDto)
    {
        var customerId = User.Id();
        var informationUpdateResult = await _identityService.UpdateCustomerPersonalInformation(customerId, customerPassportDto.Adapt<CustomerPassportModel>());

        return HandleOperationResult(informationUpdateResult);
    }
}
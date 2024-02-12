using AutoWrapper.Extensions;
using AutoWrapper.Wrappers;
using Contracts.Models;
using Microsoft.AspNetCore.Mvc;

namespace DistributedBanking.API.Controllers.Identity;

[ApiController]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class IdentityControllerBase  : ControllerBase
{
    private readonly ILogger<IdentityControllerBase> _logger;

    protected IdentityControllerBase(ILogger<IdentityControllerBase> logger)
    {
        _logger = logger;
    }
    
    protected IActionResult HandleUserManagerResult(IdentityOperationResult identityOperationResult)
    {
        if (identityOperationResult.Succeeded)
        {
            return Ok();
        }
        
        foreach (var error in identityOperationResult.Errors)
        {
            ModelState.AddModelError("", error);
        }
        
        _logger.LogInformation("Identity action has ended unsuccessfully. Details: {Result}", 
            identityOperationResult.ToString());
        
        throw new ApiException(ModelState.AllErrors());
    }
}
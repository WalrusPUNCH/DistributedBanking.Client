using AutoWrapper.Extensions;
using AutoWrapper.Wrappers;
using DistributedBanking.Client.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Entities.Constants;
using System.ComponentModel.DataAnnotations;

namespace DistributedBanking.API.Controllers;

[Route("api/identity/role")]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Administrator)]
public class RoleController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        IIdentityService identityService,
        ILogger<RoleController> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    
    [HttpPost]
    public async Task<IActionResult> CreateRole([Required] string roleName)
    {
        var result = await _identityService.CreateRole(roleName);
        if (result.Succeeded)
        {
            return Ok();
        }

        throw new ApiException(ModelState.AllErrors());
    }
    
    [AllowAnonymous]
    [HttpGet("initialize")]
    public async Task<IActionResult> Initialize()
    {
        var roles = new List<string> { RoleNames.Administrator, RoleNames.Worker, RoleNames.Customer};

        foreach (var roleName in roles)
        {
            var result = await _identityService.CreateRole(roleName);
            if (result.Succeeded)
            {
                continue;
            }

            throw new ApiException(ModelState.AllErrors());
        }

        return Ok();
    }
}
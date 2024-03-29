﻿using System.ComponentModel.DataAnnotations;
using AutoWrapper.Extensions;
using AutoWrapper.Wrappers;
using Contracts.Models;
using DistributedBanking.Client.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Entities.Constants;

namespace DistributedBanking.API.Controllers.Identity;

[Route("api/identity/role")]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RoleNames.Administrator)]
public class RoleController : CustomControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        IIdentityService identityService,
        ILogger<RoleController> logger) : base(logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    
    [HttpPost]
    public async Task<IActionResult> CreateRole([Required] string roleName)
    {
        var result = await _identityService.CreateRole(roleName);
        
        return HandleOperationResult(result);
    }
    
    [AllowAnonymous]
    [HttpGet("initialize")]
    public async Task<IActionResult> Initialize()
    {
        var roles = new List<string> { RoleNames.Administrator, RoleNames.Worker, RoleNames.Customer};

        foreach (var roleName in roles)
        {
            var roleCreationOperation = await _identityService.CreateRole(roleName);
            if (roleCreationOperation.Status is OperationStatus.Success or OperationStatus.Processing)
            {
                continue;
            }

            throw new ApiException(ModelState.AllErrors());
        }

        return Ok();
    }
}
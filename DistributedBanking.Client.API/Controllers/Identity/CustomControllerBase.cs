using AutoWrapper.Extensions;
using AutoWrapper.Wrappers;
using Contracts.Models;
using DistributedBanking.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DistributedBanking.API.Controllers.Identity;

[ApiController]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class CustomControllerBase  : ControllerBase
{
    private readonly ILogger<CustomControllerBase> _logger;

    protected CustomControllerBase(ILogger<CustomControllerBase> logger)
    {
        _logger = logger;
    }
    
    protected IActionResult HandleOperationResult(OperationResult operationResult)
    {
        switch (operationResult.Status)
        {
            case OperationStatus.Success:
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
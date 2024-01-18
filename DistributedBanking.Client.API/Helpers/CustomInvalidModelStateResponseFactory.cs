using AutoWrapper.Extensions;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace DistributedBanking.API.Helpers;

public static class CustomInvalidModelStateResponseFactory
{
    public static IActionResult MakeFailedValidationResponse(ActionContext context)
    {
        var allErrors = context.ModelState.AllErrors();
        throw new ApiException(allErrors);
    }
}
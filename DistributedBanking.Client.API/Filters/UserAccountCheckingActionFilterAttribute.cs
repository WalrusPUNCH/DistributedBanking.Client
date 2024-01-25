using Contracts.Extensions;
using DistributedBanking.API.Models.Transaction;
using DistributedBanking.Client.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DistributedBanking.API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class UserAccountCheckingActionFilterAttribute : Attribute, IAsyncActionFilter
{
    private readonly IAccountService _accountService;

    public UserAccountCheckingActionFilterAttribute(IAccountService accountService)
    {
        _accountService = accountService;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var transactionObject = context.ActionArguments
            .FirstOrDefault(p => p.Value is OneWayTransactionDto or TwoWayTransactionDto);

        var sourceAccountId = transactionObject.Value switch
        {
            OneWayTransactionDto oneWayTransaction => oneWayTransaction.SourceAccountId,
            TwoWayTransactionDto twoWayTransaction => twoWayTransaction.SourceAccountId,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(sourceAccountId))
        {
            sourceAccountId = context.HttpContext.Request.RouteValues.TryGetValue("accountId", out var value) 
                              && value is string stringValue && !string.IsNullOrWhiteSpace(stringValue) 
                ? stringValue
                : null;
        }

        if (!string.IsNullOrWhiteSpace(sourceAccountId))
        {
            var isAccountBelongsToUser = await _accountService.BelongsTo(sourceAccountId, context.HttpContext.User.Id());
            if (!isAccountBelongsToUser)
            {
                context.Result = new  ObjectResult(context.ModelState)
                {
                    Value = null,
                    StatusCode = StatusCodes.Status403Forbidden,
                };
                
                return;
            }
        }

        await next();
    }
}
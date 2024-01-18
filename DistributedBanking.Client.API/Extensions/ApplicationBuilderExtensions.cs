using AutoWrapper;
using DistributedBanking.API.Middleware;
using Serilog;

namespace DistributedBanking.API.Extensions;

public static class ApplicationBuilderExtensions
{
    internal static IApplicationBuilder UseAppCore(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(conf =>
            {
                conf.MapControllers();
            });

        return applicationBuilder;
    }
    
    internal static IApplicationBuilder UseAppSerilog(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseSerilogRequestLogging();

        return applicationBuilder;
    }
    
    internal static IApplicationBuilder UseAppSwagger(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder
            .UseSwagger()
            .UseSwaggerUI(options => 
            { 
                options.RoutePrefix = string.Empty;
                
                options.ShowCommonExtensions(); 
                options.ShowExtensions(); 
                options.DisplayRequestDuration(); 
                options.DisplayOperationId(); 
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });

        return applicationBuilder;
    }
    
    internal static IApplicationBuilder UseMiddleware(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseMiddleware<ExceptionHandlingMiddleware>();
        
        return applicationBuilder;
    }
    
    internal static IApplicationBuilder UseAutoWrapper(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseApiResponseAndExceptionWrapper(
            new AutoWrapperOptions 
            { 
                IgnoreWrapForOkRequests = true
            });
        
        return applicationBuilder;
    }
}
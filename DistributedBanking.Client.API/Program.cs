using DistributedBanking.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddApi(configuration)
    .AddSwagger()
    .AddServices(configuration)
    .ConfigureOptions(configuration);

builder.Host.UseSerilogAppLogging();

var application = builder.Build();
application
    .UseAppSerilog()
    .UseMiddleware()
    .UseAutoWrapper()
    .UseAppCore()
    .UseAppSwagger();

await application.RunAsync();

using DistributedBanking.API.Helpers;
using DistributedBanking.Client.Data.Repositories;
using DistributedBanking.Client.Data.Repositories.Base;
using DistributedBanking.Client.Data.Repositories.Implementation;
using DistributedBanking.Client.Domain.Models.Transaction;
using DistributedBanking.Client.Domain.Options;
using DistributedBanking.Client.Domain.Services;
using DistributedBanking.Client.Domain.Services.Implementation;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using Shared.Data.Entities;
using Shared.Data.Services;
using Shared.Data.Services.Implementation.MongoDb;
using Shared.Kafka.Extensions;
using Shared.Kafka.Options;
using Shared.Messaging.Messages.Account;
using Shared.Messaging.Messages.Identity;
using Shared.Messaging.Messages.Identity.Registration;
using Shared.Messaging.Messages.Transaction;
using Shared.Redis.Extensions;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace DistributedBanking.API.Extensions;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
        ArgumentNullException.ThrowIfNull(jwtOptions);
        
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services
            .AddRouting()
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });
        
        services
            .AddAuthorization()
            .AddEndpointsApiExplorer()
            .AddHttpContextAccessor()
            .AddMapster();
        
        return services;
    }
    
    internal static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        
        services
            .AddSwaggerGen(options =>
            {
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                options.UseInlineDefinitionsForEnums();
                
                // JWT
                options.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter the Bearer Authorization string as following: `Bearer *JWT-Token*`",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
            });
        
        return services;
    }
    
    internal static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = CustomInvalidModelStateResponseFactory.MakeFailedValidationResponse;
        });

        services.Configure<DatabaseOptions>(configuration.GetSection(nameof(DatabaseOptions)));
        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));
        
        return services;
    }
    
    internal static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMongoDatabase(configuration)
            .AddDataRepositories();

        services.AddRedis(configuration);
        
        services.AddTransient<IRolesManager, RolesManager>()
            .AddTransient<IUserManager, UserManager>()
            .AddTransient<IAccountService, AccountService>()
            .AddTransient<IPasswordHashingService, PasswordHashingService>()
            .AddTransient<ITokenService, TokenService>()
            .AddTransient<IIdentityService, IdentityService>()
            .AddTransient<ITransactionService, TransactionService>();

        services.AddKafkaProducers(configuration);

        return services;
    }

    private static IServiceCollection AddMapster(this IServiceCollection services)
    {
        TypeAdapterConfig<TransactionEntity, TransactionResponseModel>.NewConfig()
            .Map(dest => dest.Timestamp, src => src.DateTime);
        
        TypeAdapterConfig<ObjectId, string>.NewConfig()
            .MapWith(objectId => objectId.ToString());

        TypeAdapterConfig<string, ObjectId>.NewConfig()
            .MapWith(value => new ObjectId(value));
        
        return services;
    }
    
    private static IServiceCollection AddMongoDatabase(this IServiceCollection services, IConfiguration configuration)
    {
       var databaseOptions = configuration.GetSection(nameof(DatabaseOptions)).Get<DatabaseOptions>();
        ArgumentNullException.ThrowIfNull(databaseOptions);

        services.AddSingleton<IMongoDbFactory>(new MongoDbFactory(databaseOptions.ConnectionString, databaseOptions.DatabaseName));

        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new EnumRepresentationConvention(BsonType.String),
            new IgnoreIfNullConvention(true)
        };
        ConventionRegistry.Register("CamelCase_StringEnum_IgnoreNull_Convention", pack, _ => true);
        
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc));

       return services;
    }
    
    private static IServiceCollection AddDataRepositories(this IServiceCollection services)
    {
        services.AddTransient(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
        
        services.AddTransient<IUsersRepository, UsersRepository>();
        services.AddTransient<IRolesRepository, RolesRepository>();
        services.AddTransient<IAccountsRepository, AccountsRepository>();
        services.AddTransient<ICustomersRepository, CustomersRepository>();
        services.AddTransient<IWorkersRepository, WorkersRepository>();
        services.AddTransient<ITransactionsRepository, TransactionsRepository>();
        
        return services;
    }

    private static IServiceCollection AddKafkaProducers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKafkaProducer<UserRegistrationMessage>(configuration, KafkaTopicSource.CustomersRegistration);
        services.AddKafkaProducer<WorkerRegistrationMessage>(configuration, KafkaTopicSource.WorkersRegistration);
        services.AddKafkaProducer<CustomerInformationUpdateMessage>(configuration, KafkaTopicSource.CustomersUpdate);
        services.AddKafkaProducer<EndUserDeletionMessage>(configuration, KafkaTopicSource.UsersDeletion);
        services.AddKafkaProducer<AccountCreationMessage>(configuration, KafkaTopicSource.AccountCreation);
        services.AddKafkaProducer<AccountDeletionMessage>(configuration, KafkaTopicSource.AccountDeletion);
        services.AddKafkaProducer<TransactionMessage>(configuration, KafkaTopicSource.TransactionsCreation);
        
        return services;
    }
}
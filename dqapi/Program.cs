using DotNetEnv;
using dqapi.Application.Common;
using dqapi.Domain.Entities.Common;
using dqapi.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Data;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Load environment variables
    Env.Load();
    builder.Configuration.AddEnvironmentVariables();

    // Configure Serilog for file logging (json format)
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .WriteTo.Console()
        .WriteTo.File(new JsonFormatter(), "logs/applog-.json", rollingInterval: RollingInterval.Day)
        .CreateLogger();

    builder.Host.UseSerilog();

    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
    builder.Services.AddTransient<IDbConnection>(provider =>
        new SqlConnection(builder.Configuration.GetConnectionString("dqapi"))
    );

    // Add services to the container.
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<AppLogger>();
    builder.Services.AddScoped<AuthHelper>();
    builder.Services.AddScoped<AppDbDataContext>();
    builder.Services.AddScoped<ResponseHandler>();
    builder.Services.AddSingleton<JsonHelper>();
    
    // Configure Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        // Default global rate limit policy
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1)
                }));

        // Anonymous endpoints policy (sign-up, sign-in)
        options.AddPolicy("anonymous", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1)
                }));

        // Authenticated endpoints policy
        options.AddPolicy("authenticated", httpContext =>
            RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: httpContext.User?.Identity?.Name ?? 
                              httpContext.Connection.RemoteIpAddress?.ToString() ?? 
                              httpContext.Request.Headers.Host.ToString(),
                factory: partition => new TokenBucketRateLimiterOptions
                {
                    AutoReplenishment = true,
                    TokenLimit = 50,
                    TokensPerPeriod = 10,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10)
                }));

        // Express controller endpoints policy
        options.AddPolicy("express", httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: httpContext.User?.Identity?.Name ?? 
                              httpContext.Connection.RemoteIpAddress?.ToString() ?? 
                              httpContext.Request.Headers.Host.ToString(),
                factory: partition => new SlidingWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6
                }));

        // Configure rate limit exceeded response
        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";
            
            var errorMessage = new ErrorMessage
            {
                TraceUuid = context.HttpContext.TraceIdentifier,
                ResponseCode = StatusCodes.Status429TooManyRequests,
                ResponseMessage = "Too many requests. Please try again later."
            };
            
            await context.HttpContext.Response.WriteAsJsonAsync(errorMessage, token);
        };
    });

    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errorMessage = new ErrorMessage
                {
                    TraceUuid = context.HttpContext.TraceIdentifier,
                    ResponseCode = StatusCodes.Status400BadRequest,
                    ResponseMessage = string.Join("; ",
                        context.ModelState
                            .Where(ms => ms.Value != null && ms.Value.Errors.Count > 0)
                            .SelectMany(ms => ms.Value!.Errors.Select(e => e.ErrorMessage))
                            .ToList()
                    )
                };

                return new BadRequestObjectResult(errorMessage);
            };
        });

    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<GzipCompressionProvider>();
    });
    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "DQ (Do it Quickly) API",
            Version = "v1",
            Description = "This project is designed to streamline the development process for web applications, focusing on rapid prototyping in a development environment.",
            License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://github.com/nova177dev/dqapi/blob/master/LICENSE.txt") },
            Contact = new OpenApiContact
            {
                Name = "Anton V. Novoseltsev",
                Email = "nova177dev@gmail.com"
            }
        });

        // Add security definition
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });

        // Add security requirement
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
           {
           {
               new OpenApiSecurityScheme
               {
                   Reference = new OpenApiReference
                   {
                       Type = ReferenceType.SecurityScheme,
                       Id = "Bearer"
                   }
               },
               new string[] {}
           }
           });
    });

    // JWT Setup
    // Token
    string tokenKeyString = builder.Configuration["AppSettings:TokenKey"] ?? throw new ArgumentNullException(nameof(tokenKeyString), "Couldn't extract token key.");

    SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                tokenKeyString
            )
        );

    TokenValidationParameters tokenValidationParameters = new()
    {
        IssuerSigningKey = tokenKey,
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["AppSettings:JwtIssuer"],
        ValidAudience = builder.Configuration["AppSettings:JwtAudience"]
    };

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = tokenValidationParameters;

            // Customize the response for 401 errors
            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    // Skip the default WWW-Authenticate header
                    context.HandleResponse();

                    // Return a custom 401 JSON response
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    // Return the response
                    return context.Response.WriteAsync(
                        $$"""
                    {
                        "TraceUuid": "{{context.HttpContext.TraceIdentifier}}",
                        "ResponseCode": "{{StatusCodes.Status401Unauthorized}}",
                        "ResponseMessage": "Unauthorized"
                    }
                    """
                    );
                }
            };
        });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    
    // Apply rate limiting middleware
    app.UseRateLimiter();
    
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<ExceptionHandler>();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
public partial class Program { }

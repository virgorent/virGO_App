using System.Threading.RateLimiting;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.AspNetCoreServer.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using VirGo.Api.Errors;
using VirGo.Api.Middleware;
using VirGo.Api.OpenApi;
using VirGo.Api.Security;
using VirGo.Application.Configuration;
using VirGo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerOpenApiTransformer>());

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services
    .AddOptions<CorsOptions>()
    .Bind(builder.Configuration.GetSection("Cors"));
builder.Services
    .AddOptions<RateLimitingOptions>()
    .Bind(builder.Configuration.GetSection("RateLimiting"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddCors(options => options.AddPolicy("MobileAndWeb", policy =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is required.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.EventsType = typeof(JwtSessionEvents);
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtSessionEvents>();

var rateLimitOptions = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingOptions>()
    ?? new RateLimitingOptions();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitOptions.PermitLimit,
                Window = TimeSpan.FromSeconds(rateLimitOptions.WindowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages(async (StatusCodeContext statusContext) =>
{
    var response = statusContext.HttpContext.Response;
    if (response.HasStarted || response.StatusCode is < 400 or >= 600)
    {
        return;
    }

    var problemDetails = new ProblemDetails
    {
        Status = response.StatusCode,
        Title = response.StatusCode == StatusCodes.Status401Unauthorized
            ? "La autenticación es necesaria."
            : "La solicitud no pudo completarse.",
        Instance = statusContext.HttpContext.Request.Path
    };
    problemDetails.Extensions["traceId"] = statusContext.HttpContext.TraceIdentifier;
    response.ContentType = "application/problem+json";
    await response.WriteAsync(JsonSerializer.Serialize(problemDetails));
});
app.UseCors("MobileAndWeb");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.MapOpenApi();
app.MapControllers();

app.Run();

public partial class Program;

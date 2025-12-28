using Serilog;
using Po.LearnCert.Api.Middleware;
using Po.LearnCert.Api.Features.Authentication.Infrastructure;
using Po.LearnCert.Api.Features.Quiz.Infrastructure;
using Po.LearnCert.Api.Features.Quiz.Services;
using Po.LearnCert.Api.Features.Quiz.Services.Handlers;
using Po.LearnCert.Api.Features.Certifications.Infrastructure;
using Po.LearnCert.Api.Features.Certifications.Services;
using Po.LearnCert.Api.Features.Leaderboards.Infrastructure;
using Po.LearnCert.Api.Features.Leaderboards.Services;
using Po.LearnCert.Api.Features.Statistics.Repositories;
using Po.LearnCert.Api.Features.Statistics.Services;
using Po.LearnCert.Api.Health;
using Po.LearnCert.Api.Infrastructure;
using Azure.Data.Tables;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Po.LearnCert.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire ServiceDefaults for observability and resilience
builder.AddServiceDefaults();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/polearncer-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting PoLearnCert API");

    // Add services to the container
    builder.Services.AddControllers();

    // Configure Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // Use Microsoft.OpenApi types (package provides these). This keeps metadata simple.
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "PoLearnCert API",
            Version = "v1",
            Description = "API for PoLearnCert Certification Quiz Platform",
            Contact = new OpenApiContact
            {
                Name = "PoLearnCert Team"
            }
        });
    });

    // Configure Azure Table Storage via Aspire
    builder.AddAzureTableServiceClient("tables");

    // Configure ASP.NET Core Identity
    builder.Services.AddScoped<IUserStore<UserEntity>, TableUserStore>();
    builder.Services.AddScoped<IRoleStore<RoleEntity>, TableRoleStore>();

    builder.Services.AddIdentity<UserEntity, RoleEntity>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.RequireUniqueEmail = true;
    })
    .AddDefaultTokenProviders();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });

    // Register repositories
    builder.Services.AddScoped<ICertificationRepository, CertificationRepository>();
    builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
    builder.Services.AddScoped<IQuizSessionRepository, QuizSessionRepository>();
    builder.Services.AddScoped<IUserStatisticsRepository, UserStatisticsRepository>();
    builder.Services.AddScoped<ISubtopicRepository, SubtopicRepository>();
    builder.Services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();

    // Register handlers
    builder.Services.AddScoped<IAnswerSubmissionHandler, AnswerSubmissionHandler>();
    builder.Services.AddScoped<IQuizCompletionHandler, QuizCompletionHandler>();

    // Register services
    builder.Services.AddScoped<ICertificationService, CertificationService>();
    builder.Services.AddScoped<IQuizSessionService, QuizSessionService>();
    builder.Services.AddScoped<IUserStatisticsService, UserStatisticsService>();
    builder.Services.AddScoped<LeaderboardService>();
    builder.Services.AddScoped<Po.LearnCert.Api.Features.Authentication.Services.AuthenticationService>();
    builder.Services.AddScoped<DataSeeder>();

    // Configure Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck<AzureTableStorageHealthCheck>(
            "azure_table_storage",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "database", "storage" });

    var app = builder.Build();

    // Seed data on startup (skip during integration tests)
    if (!app.Environment.IsEnvironment("Testing"))
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }

    // Use Problem Details exception handling
    app.UseProblemDetailsExceptionHandler();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "PoLearnCert API v1");
            options.RoutePrefix = "swagger";
        });
    }

    if (!app.Environment.IsEnvironment("Testing"))
    {
        app.UseHttpsRedirection();
    }

    // Serve Blazor WebAssembly static files
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    // Map Health Checks endpoint
    app.MapHealthChecks("/api/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.ToString(),
                    exception = e.Value.Exception?.Message,
                    data = e.Value.Data
                }),
                totalDuration = report.TotalDuration.ToString()
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.MapControllers();
    app.MapFallbackToFile("index.html");

    Log.Information("PoLearnCert API started successfully");

    // Map Aspire default endpoints (metrics, traces, etc.)
    app.MapDefaultEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
public partial class Program { }

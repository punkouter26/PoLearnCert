using Serilog;
using Po.LearnCert.Api.Middleware;
using Po.LearnCert.Api.Features.Authentication.Infrastructure;
using Po.LearnCert.Api.Features.Quiz.Infrastructure;
using Po.LearnCert.Api.Features.Quiz.Services;
using Po.LearnCert.Api.Features.Certifications.Infrastructure;
using Po.LearnCert.Api.Features.Certifications.Services;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

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
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "PoLearnCert API",
            Version = "v1",
            Description = "API for PoLearnCert Certification Quiz Platform",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "PoLearnCert Team"
            }
        });
    });

    // Configure CORS for Blazor Client
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("BlazorClient", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5000",
                    "https://localhost:5001",
                    "http://localhost:5173", // Vite dev server
                    "https://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // Configure Azure Table Storage
    var azureTableConnectionString = builder.Configuration["AzureTableStorage:ConnectionString"] 
        ?? builder.Configuration.GetConnectionString("AzureTableStorage");
    builder.Services.AddSingleton(new TableServiceClient(azureTableConnectionString));

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
    
    // Register services
    builder.Services.AddScoped<ICertificationService, CertificationService>();
    builder.Services.AddScoped<IQuizSessionService, QuizSessionService>();

    var app = builder.Build();

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

    app.UseHttpsRedirection();
    
    // Serve Blazor WebAssembly static files
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
    
    // Use CORS
    app.UseCors("BlazorClient");
    
    app.UseRouting();
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();
    app.MapFallbackToFile("index.html");

    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

    Log.Information("PoLearnCert API started successfully");
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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

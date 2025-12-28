using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Po.LearnCert.Client;
using Po.LearnCert.Client.Features.Quiz;
using Po.LearnCert.Client.Features.Quiz.Services;
using Po.LearnCert.Client.Features.Certifications.Services;
using Po.LearnCert.Client.Features.Statistics.Services;
using Po.LearnCert.Client.Features.Leaderboards.Services;
using Po.LearnCert.Client.Features.QuestionGeneration;
using Po.LearnCert.Client.Features.Authentication;
using Po.LearnCert.Client.Features.Authentication.Services;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for API calls - use base address of hosting server
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Add Radzen services
builder.Services.AddRadzenComponents();

// Add Authorization support
builder.Services.AddAuthorizationCore();

// Register authentication services
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Register application services
builder.Services.AddScoped<IQuizSessionService, QuizSessionService>();
builder.Services.AddScoped<ICertificationService, CertificationService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IQuestionGenerationService, QuestionGenerationService>();

// Register state management
builder.Services.AddScoped<QuizSessionState>();

await builder.Build().RunAsync();

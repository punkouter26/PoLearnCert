using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Po.LearnCert.Client;
using Po.LearnCert.Client.Features.Quiz;
using Po.LearnCert.Client.Features.Quiz.Services;
using Po.LearnCert.Client.Features.Certifications.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for API calls
var apiBaseAddress = builder.Configuration.GetValue<string>("ApiBaseAddress") 
    ?? "http://localhost:5000";

builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(apiBaseAddress) 
});

// Add Authorization support
builder.Services.AddAuthorizationCore();

// Register application services
builder.Services.AddScoped<IQuizSessionService, QuizSessionService>();
builder.Services.AddScoped<ICertificationService, CertificationService>();

// Register state management
builder.Services.AddScoped<QuizSessionState>();

await builder.Build().RunAsync();

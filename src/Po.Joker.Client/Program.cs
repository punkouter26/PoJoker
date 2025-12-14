using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Po.Joker.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register client-side services
builder.Services.AddScoped<ISpeechService, SpeechService>();
builder.Services.AddScoped<IAudioService, AudioService>();
builder.Services.AddScoped<ISessionService, SessionService>();

await builder.Build().RunAsync();

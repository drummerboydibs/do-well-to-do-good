using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DoWellToDoGood;
using DoWellToDoGood.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CryptoService>();
builder.Services.AddScoped<EntriesService>();
builder.Services.AddScoped<TipHistoryService>();

var host = builder.Build();

// Pick up a magic-link redirect or restore a stored session before first render.
await host.Services.GetRequiredService<AuthService>().InitializeAsync();

await host.RunAsync();

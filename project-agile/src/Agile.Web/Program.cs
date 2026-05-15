using Agile.Core.Clients;
using Agile.Core.Services;
using Agile.Web.Components;
using Agile.Web.Services;

// Walk up the directory tree to find the .env file
var currentDir = new System.IO.DirectoryInfo(Environment.CurrentDirectory);
string envPath = ".env";
while (currentDir != null)
{
    var potentialEnv = System.IO.Path.Combine(currentDir.FullName, ".env");
    if (System.IO.File.Exists(potentialEnv))
    {
        envPath = potentialEnv;
        break;
    }
    currentDir = currentDir.Parent;
}
DotNetEnv.Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<QuickDatasetGenerator>(_ =>
    new QuickDatasetGenerator(new GitHubModelsClient("gpt-4.1")));
builder.Services.AddScoped<QuickAuditRunner>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

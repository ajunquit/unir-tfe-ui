using UNIR.TFE.Polyrepo.Addition.Module.Application;
using UNIR.TFE.Polyrepo.Division.Module.Application;
using UNIR.TFE.Polyrepo.Multiplication.Module.Application;
using UNIR.TFE.Polyrepo.Subtraction.Module.Application;
using UNIR.TFE.Polyrepo.UI.Infrastructure.External.GitHub;
using UNIR.TFE.Polyrepo.UI.Infrastructure.External.GitHub.Impl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IAdditionAppService, AdditionAppService>();
builder.Services.AddScoped<ISubtractionAppService, SubtractionAppService>();
builder.Services.AddScoped<IMultiplicationAppService, MultiplicationAppService>();
builder.Services.AddScoped<IDivisionAppService, DivisionAppService>();
builder.Services.AddScoped<IGitRepositoryAnalyzerService, GitRepositoryAnalyzerService>();
builder.Services.AddScoped<IGitHubRepositoryService, GitHubRepositoryService>();
builder.Services.AddScoped<IGitHubUrlParser, GitHubUrlParser>();

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
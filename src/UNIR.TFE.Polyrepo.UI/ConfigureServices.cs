using UNIR.TFE.Polyrepo.UI.Infrastructure.External.GitHub;
using UNIR.TFE.Polyrepo.UI.Infrastructure.External.GitHub.Impl;

namespace UNIR.TFE.Polyrepo.UI
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureModule(this IServiceCollection services)
        {
            services.AddScoped<IGitRepositoryAnalyzerService, GitRepositoryAnalyzerService>();
            services.AddScoped<IGitHubRepositoryService, GitHubRepositoryService>();
            services.AddScoped<IGitHubUrlParser, GitHubUrlParser>();
            return services;
        }
    }
}

using UNIR.TFE.Polyrepo.UI.Models.Common;

namespace UNIR.TFE.Polyrepo.UI.Infrastructure.External.GitHub
{
    public interface IGitRepositoryAnalyzerService
    {
        Task<GitModel> AnalyzeRepositoryAsync(string repoUrl, string branch, string? token = null);
    }
}

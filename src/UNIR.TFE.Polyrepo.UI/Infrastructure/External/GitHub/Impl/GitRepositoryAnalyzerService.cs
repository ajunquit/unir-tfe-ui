using UNIR.TFE.Polyrepo.UI.Models.Common;

namespace UNIR.TFE.Polyrepo.UI.Infrastructure.External.GitHub.Impl
{
    public class GitRepositoryAnalyzerService : IGitRepositoryAnalyzerService
    {
        private readonly IGitHubRepositoryService _gitHubService;
        private readonly IGitHubUrlParser _urlParser;

        public GitRepositoryAnalyzerService(
            IGitHubRepositoryService gitHubService,
            IGitHubUrlParser urlParser)
        {
            _gitHubService = gitHubService;
            _urlParser = urlParser;
        }

        public async Task<GitModel> AnalyzeRepositoryAsync(string repoUrl, string branch, string? token = null)
        {
            var (owner, repo) = _urlParser.ParseGitHubUrl(repoUrl);

            // Obtener información del superproyecto
            var mainCommitSha = await _gitHubService.GetBranchCommitShaAsync(owner, repo, branch, token);
            var superproject = new SubmoduleInfo(repo, mainCommitSha, repoUrl);

            // Obtener submódulos
            var submodules = await _gitHubService.GetSubmodulesAsync(owner, repo, mainCommitSha, token);

            // Construir URLs completas para los submódulos (asumiendo que son del mismo owner)
            var enrichedSubmodules = submodules.Select(s =>
                new SubmoduleInfo(s.Path, s.Sha, $"https://github.com/{owner}/{s.Path}")).ToList();

            return new GitModel
            {
                Superproject = superproject,
                Submodules = enrichedSubmodules
            };
        }
    }
}

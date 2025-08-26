using UNIR.TFE.Polyrepo.UI.Models.Common;

namespace UNIR.TFE.Polyrepo.UI.Infrastructure.External.GitHub
{
    public interface IGitHubRepositoryService
    {
        Task<string> GetBranchCommitShaAsync(string owner, string repo, string branch, string? token);
        Task<string> GetDefaultBranchAsync(string owner, string repo, string? token);
        Task<List<SubmoduleInfo>> GetSubmodulesAsync(string owner, string repo, string commitSha, string? token);
    }

    public interface IGitHubUrlParser
    {
        (string Owner, string Repo) ParseGitHubUrl(string url);
    }

}

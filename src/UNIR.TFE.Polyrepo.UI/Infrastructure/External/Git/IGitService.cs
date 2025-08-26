using UNIR.TFE.Polyrepo.UI.Infrastructure.External.Git.Models;

namespace UNIR.TFE.Polyrepo.UI.Infrastructure.External.Git
{
    public interface IGitService
    {
        public GitInfo GetSuperProjectInfo(string repoPath, string branch);
        public List<GitInfo> GetSubmodulesProjectsInfo(string repoPath);
    }
}

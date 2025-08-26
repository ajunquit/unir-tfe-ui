namespace UNIR.TFE.Polyrepo.UI.Models.Common
{
    public record SubmoduleInfo(string Path, string Sha, string? Url = null);

    public class GitModel
    {
        public SubmoduleInfo Superproject { get; set; } = new(string.Empty, string.Empty);
        public List<SubmoduleInfo> Submodules { get; set; } = new();
    }
}

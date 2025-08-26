using UNIR.TFE.Polyrepo.UI.Models.Common;
using System.Net.Http.Headers;
using System.Text.Json;

namespace UNIR.TFE.Polyrepo.UI.Infrastructure.External.GitHub.Impl
{
    public class GitHubRepositoryService : IGitHubRepositoryService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GitHubRepositoryService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetBranchCommitShaAsync(string owner, string repo, string branch, string? token)
        {
            using var client = CreateHttpClient(token);

            var url = $"https://api.github.com/repos/{owner}/{repo}/branches/{branch}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al obtener rama: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            return doc.RootElement.GetProperty("commit").GetProperty("sha").GetString()
                   ?? throw new Exception("No se pudo obtener SHA del commit");
        }

        public async Task<string> GetDefaultBranchAsync(string owner, string repo, string? token)
        {
            using var client = CreateHttpClient(token);

            var url = $"https://api.github.com/repos/{owner}/{repo}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al obtener repo: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            return doc.RootElement.GetProperty("default_branch").GetString()
                   ?? throw new Exception("No se pudo obtener rama por defecto");
        }

        public async Task<List<SubmoduleInfo>> GetSubmodulesAsync(string owner, string repo, string commitSha, string? token)
        {
            using var client = CreateHttpClient(token);

            var url = $"https://api.github.com/repos/{owner}/{repo}/git/trees/{commitSha}?recursive=1";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al obtener árbol: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            var submodules = new List<SubmoduleInfo>();
            var tree = doc.RootElement.GetProperty("tree");

            foreach (var item in tree.EnumerateArray())
            {
                if (item.TryGetProperty("type", out var type) && type.GetString() == "commit" &&
                    item.TryGetProperty("mode", out var mode) && mode.GetString() == "160000")
                {
                    submodules.Add(new SubmoduleInfo(
                        item.GetProperty("path").GetString() ?? "",
                        item.GetProperty("sha").GetString() ?? ""
                    ));
                }
            }

            return submodules;
        }

        private HttpClient CreateHttpClient(string? token)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("GitHub-SHA-Fetcher/1.0");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }
    }

    public class GitHubUrlParser : IGitHubUrlParser
    {
        public (string Owner, string Repo) ParseGitHubUrl(string url)
        {
            // Eliminar .git si está presente y cualquier trailing slash
            url = url.TrimEnd('/').Replace(".git", "");

            var parts = url.Split('/');
            if (parts.Length < 2)
                throw new ArgumentException("URL de GitHub no válida");

            // Tomar los últimos dos segmentos como owner/repo
            return (parts[^2], parts[^1]);
        }
    }
}

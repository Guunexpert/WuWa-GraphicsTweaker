using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;

namespace PhoebeEditor.Services;

public class UpdateService
{
    private readonly HttpClient _http = new();
    private const string RepoApiUrl = "https://api.github.com/repos/Guunexpert/WuWa-GraphicsTweaker/releases/latest";

    public async Task<UpdateResult> CheckForUpdatesAsync()
    {
        var currentVersion = GetAssemblyVersion();

        try
        {
            _http.DefaultRequestHeaders.Add("User-Agent", "Phoebe-Editor-UpdateChecker");

            var response = await _http.GetAsync(RepoApiUrl);
            if (!response.IsSuccessStatusCode)
                return new UpdateResult(false, currentVersion, currentVersion, string.Empty, "Check failed");

            var json = await response.Content.ReadFromJsonAsync<GithubRelease>();
            if (json == null)
                return new UpdateResult(false, currentVersion, currentVersion, string.Empty, "Parse error");

            var latestVersion = NormalizeVersion(json.TagName ?? "");

            var hasUpdate = CompareVersions(latestVersion, currentVersion) > 0;

            return new UpdateResult(
                hasUpdate,
                currentVersion,
                latestVersion,
                json.HtmlUrl ?? string.Empty,
                json.Body ?? string.Empty
            );
        }
        catch
        {
            return new UpdateResult(false, currentVersion, currentVersion, string.Empty, "Network error");
        }
    }

    private static string GetAssemblyVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString(3) ?? "0.0.0";
    }

    private static string NormalizeVersion(string tag)
    {
    var clean = tag.TrimStart('v', 'V');
    var parts = clean.Split('.');
    return parts.Length switch
    {
        1 => $"{clean}.0.0",
        2 => $"{clean}.0",
        _ => clean
    };
    }

    private static int CompareVersions(string a, string b)
    {
        var partsA = ParseVersionParts(a);
        var partsB = ParseVersionParts(b);

        for (int i = 0; i < Math.Max(partsA.Length, partsB.Length); i++)
        {
            var valA = i < partsA.Length ? partsA[i] : 0;
            var valB = i < partsB.Length ? partsB[i] : 0;
            if (valA != valB)
                return valA.CompareTo(valB);
        }
        return 0;
    }

    private static int[] ParseVersionParts(string version)
    {
        var clean = new string(version.Where(c => char.IsDigit(c) || c == '.').ToArray());
        return clean.Split('.').Select(p => int.TryParse(p, out var n) ? n : 0).ToArray();
    }

    private record GithubRelease(string? TagName, string? HtmlUrl, string? Body);
}

namespace PhoebeEditor.Services;

public record UpdateResult(
    bool HasUpdate,
    string CurrentVersion,
    string LatestVersion,
    string DownloadUrl,
    string ReleaseNotes
);

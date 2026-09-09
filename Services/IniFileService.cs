using System.Text;

namespace PhoebeEditor.Services;

public class IniFileService
{
    public Dictionary<string, string> ReadSection(
        string filePath,
        string section)
    {
        var result = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(filePath))
            return result;

        var lines = File.ReadAllLines(filePath);
        var inSection = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            if (trimmed.StartsWith('['))
            {
                inSection = trimmed.Equals(
                    $"[{section}]",
                    StringComparison.OrdinalIgnoreCase);

                continue;
            }

            if (!inSection)
                continue;

            if (string.IsNullOrWhiteSpace(trimmed) ||
                trimmed.StartsWith(';') ||
                trimmed.StartsWith('#'))
                continue;

            var eqIndex = trimmed.IndexOf('=');

            if (eqIndex <= 0)
                continue;

            var key = trimmed[..eqIndex].Trim();
            var value = trimmed[(eqIndex + 1)..].Trim();

            result[key] = value;
        }

        return result;
    }
    
    /// updates only the supplied keys, Existing unknown keys, comments and unrelated settings are preserved
    public void UpdateValues(
        string filePath,
        string section,
        IReadOnlyDictionary<string, string> settings)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException(
                "INI file path cannot be empty.",
                nameof(filePath));

        if (settings.Count == 0)
            return;

        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var lines = File.Exists(filePath)
            ? File.ReadAllLines(filePath).ToList()
            : new List<string>();

        var sectionHeader = $"[{section}]";

        var sectionStart = lines.FindIndex(l =>
            l.Trim().Equals(
                sectionHeader,
                StringComparison.OrdinalIgnoreCase));

        if (sectionStart == -1)
        {
            if (lines.Count > 0 &&
                !string.IsNullOrWhiteSpace(lines[^1]))
            {
                lines.Add(string.Empty);
            }

            lines.Add(sectionHeader);

            foreach (var pair in settings)
                lines.Add($"{pair.Key}={pair.Value}");
        }
        else
        {
            var sectionEnd = sectionStart + 1;

            while (sectionEnd < lines.Count)
            {
                var trimmed = lines[sectionEnd].Trim();

                if (trimmed.StartsWith('['))
                    break;

                sectionEnd++;
            }

            // keep track of keys already present.
            var existingKeys =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            for (var i = sectionStart + 1; i < sectionEnd; i++)
            {
                var trimmed = lines[i].Trim();

                if (string.IsNullOrWhiteSpace(trimmed) ||
                    trimmed.StartsWith(';') ||
                    trimmed.StartsWith('#'))
                    continue;

                var eqIndex = trimmed.IndexOf('=');

                if (eqIndex <= 0)
                    continue;

                var key = trimmed[..eqIndex].Trim();

                existingKeys[key] = i;
            }

            // update existing keys in-place.
            foreach (var pair in settings)
            {
                if (existingKeys.TryGetValue(pair.Key, out var lineIndex))
                {
                    var original = lines[lineIndex];

                    var indentation =
                        original[..(original.Length -
                                   original.TrimStart().Length)];

                    lines[lineIndex] =
                        $"{indentation}{pair.Key}={pair.Value}";
                }
            }

            // add keys which do not exist yet.
            foreach (var pair in settings)
            {
                if (existingKeys.ContainsKey(pair.Key))
                    continue;

                lines.Insert(
                    sectionEnd,
                    $"{pair.Key}={pair.Value}");

                sectionEnd++;
            }
        }

        WriteAtomically(filePath, lines);
    }

    // creates a backup of the current INI file.
    public string CreateBackup(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                "INI file does not exist.",
                filePath);

        var directory =
            Path.GetDirectoryName(filePath)
            ?? AppContext.BaseDirectory;

        var backupDirectory =
            Path.Combine(directory, "Backups");

        Directory.CreateDirectory(backupDirectory);

        var timestamp =
            DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");

        var backupPath = Path.Combine(
            backupDirectory,
            $"Engine_{timestamp}.ini");

        File.Copy(
            filePath,
            backupPath,
            overwrite: false);

        return backupPath;
    }

    public void SetValue(
        string filePath,
        string section,
        string key,
        string value)
    {
        UpdateValues(
            filePath,
            section,
            new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase)
            {
                [key] = value
            });
    }

    private static void WriteAtomically(
        string filePath,
        IReadOnlyList<string> lines)
    {
        var tempPath = filePath + ".tmp";

        try
        {
            File.WriteAllLines(
                tempPath,
                lines,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            // validate the temporary file can be read.
            _ = File.ReadAllLines(tempPath);

            if (File.Exists(filePath))
            {
                File.Replace(
                    tempPath,
                    filePath,
                    destinationBackupFileName: null);
            }
            else
            {
                File.Move(
                    tempPath,
                    filePath);
            }
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public List<string> GetSections(string filePath)
    {
        if (!File.Exists(filePath))
            return [];

        return File.ReadAllLines(filePath)
            .Where(l =>
                l.Trim().StartsWith('[') &&
                l.Trim().EndsWith(']'))
            .Select(l => l.Trim()[1..^1])
            .ToList();
    }
}

using System.Text;
using System.IO;
namespace PhoebeEditor.Services;

public class IniFileService
{
    public Dictionary<string, string> ReadSection(string filePath, string section)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(filePath)) return result;

        var lines = File.ReadAllLines(filePath);
        var inSection = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            if (trimmed.StartsWith('['))
            {
                inSection = trimmed.Equals($"[{section}]", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inSection) continue;
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(';')) continue;

            var eqIndex = trimmed.IndexOf('=');
            if (eqIndex <= 0) continue;

            var key = trimmed[..eqIndex].Trim();
            var value = trimmed[(eqIndex + 1)..].Trim();
            result[key] = value;
        }

        return result;
    }

    public void WriteSection(string filePath, string section, Dictionary<string, string> settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var lines = File.Exists(filePath)
            ? File.ReadAllLines(filePath).ToList()
            : new List<string>();

        var sectionHeader = $"[{section}]";
        var sectionStart = lines.FindIndex(l =>
            l.Trim().Equals(sectionHeader, StringComparison.OrdinalIgnoreCase));

        if (sectionStart == -1)
        {
            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                lines.Add(string.Empty);

            lines.Add(sectionHeader);
            foreach (var kvp in settings)
                lines.Add($"{kvp.Key}={kvp.Value}");
        }
        else
        {
            var sectionEnd = sectionStart + 1;
            while (sectionEnd < lines.Count)
            {
                var l = lines[sectionEnd].Trim();
                if (l.StartsWith('[')) break;
                sectionEnd++;
            }
            lines.RemoveRange(sectionStart + 1, sectionEnd - sectionStart - 1);
            var newLines = settings.Select(kvp => $"{kvp.Key}={kvp.Value}").ToList();
            lines.InsertRange(sectionStart + 1, newLines);
        }

        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }

    public void SetValue(string filePath, string section, string key, string value)
    {
        var existing = ReadSection(filePath, section);
        existing[key] = value;
        WriteSection(filePath, section, existing);
    }

    public List<string> GetSections(string filePath)
    {
        if (!File.Exists(filePath)) return [];

        return File.ReadAllLines(filePath)
            .Where(l => l.Trim().StartsWith('[') && l.Trim().EndsWith(']'))
            .Select(l => l.Trim()[1..^1])
            .ToList();
    }
}

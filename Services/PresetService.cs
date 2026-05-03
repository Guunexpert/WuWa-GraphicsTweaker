using System.Text.Json;
using PhoebeEditor.Models;
using System.IO;
namespace PhoebeEditor.Services;

public class PresetService
{
    private readonly string _presetsDir;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public PresetService()
    {
        var exeDir = Path.GetDirectoryName(Environment.ProcessPath)
                  ?? AppContext.BaseDirectory;
        _presetsDir = Path.Combine(exeDir, "Presets");
        Directory.CreateDirectory(_presetsDir);
    }

    public List<Preset> LoadAll()
    {
        var presets = new List<Preset>();

        foreach (var file in Directory.GetFiles(_presetsDir, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var preset = JsonSerializer.Deserialize<Preset>(json);
                if (preset != null) presets.Add(preset);
            }
            catch { /* skip corrupt files */ }
        }

        return presets.OrderBy(p => p.CreatedAt).ToList();
    }

    public void Save(Preset preset)
    {
        var safeFileName = string.Concat(preset.Name
            .Split(Path.GetInvalidFileNameChars()))
            .Replace(" ", "_");

        var filePath = Path.Combine(_presetsDir, $"{safeFileName}.json");
        var json = JsonSerializer.Serialize(preset, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    public void Delete(Preset preset)
    {
        var safeFileName = string.Concat(preset.Name
            .Split(Path.GetInvalidFileNameChars()))
            .Replace(" ", "_");

        var filePath = Path.Combine(_presetsDir, $"{safeFileName}.json");
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    public string PresetsDirectory => _presetsDir;
}

using System.Text.Json;

namespace SupplierService.Tests;

/// <summary>
/// Loads captured supplier responses. Fixtures are real wire format, redacted -
/// never hand-authored to match the mapper (ADR-0004).
/// </summary>
public static class FixtureLoader
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static T Load<T>(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Fixture '{fileName}' not found. Capture it from the supplier sandbox and redact it.", path);

        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options)
               ?? throw new InvalidOperationException($"Fixture '{fileName}' deserialised to null.");
    }
}

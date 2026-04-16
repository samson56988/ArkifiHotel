using System.Reflection;
using Shared.Services.Abstractions;

namespace Shared.Services.Services;

public sealed class EmbeddedEmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly Assembly _assembly;
    private readonly Dictionary<string, string> _templateCache = new(StringComparer.OrdinalIgnoreCase);

    public EmbeddedEmailTemplateRenderer()
    {
        _assembly = typeof(Shared.Data.Emails.EmailMessage).Assembly;
    }

    public string Render(string templateName, IReadOnlyDictionary<string, string> values)
    {
        var template = LoadTemplate(templateName);
        var rendered = template;

        foreach (var pair in values)
        {
            rendered = rendered.Replace(
                $"{{{{{pair.Key}}}}}",
                pair.Value ?? string.Empty,
                StringComparison.Ordinal);
        }

        return rendered;
    }

    private string LoadTemplate(string templateName)
    {
        if (_templateCache.TryGetValue(templateName, out var cached))
        {
            return cached;
        }

        var resourceName = $"{_assembly.GetName().Name}.Templates.{templateName}.html";
        using var stream = _assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Email template '{templateName}' not found as embedded resource.");
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        _templateCache[templateName] = content;
        return content;
    }
}

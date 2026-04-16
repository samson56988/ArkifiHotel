namespace Shared.Services.Abstractions;

public interface IEmailTemplateRenderer
{
    string Render(string templateName, IReadOnlyDictionary<string, string> values);
}

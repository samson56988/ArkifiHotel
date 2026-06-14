namespace Admin.Infrastructure.Helpers;

/// <summary>Generates guest-facing codes like MAR-48291037 from the business name prefix.</summary>
public static class BusinessReferenceCodeGenerator
{
    private const int MaxAttempts = 32;

    /// <summary>First three letters (A–Z) from the business name, padded with X if needed.</summary>
    public static string GetPrefixFromBusinessName(string? businessName)
    {
        if (string.IsNullOrWhiteSpace(businessName))
        {
            return "REF";
        }

        Span<char> prefix = stackalloc char[3];
        var count = 0;

        foreach (var c in businessName.Trim().ToUpperInvariant())
        {
            if (c is >= 'A' and <= 'Z')
            {
                prefix[count++] = c;
                if (count == 3)
                {
                    break;
                }
            }
        }

        while (count < 3)
        {
            prefix[count++] = 'X';
        }

        return new string(prefix);
    }

    /// <summary>Prefix plus hyphen and eight random digits, e.g. MAR-48291037.</summary>
    public static string FormatCode(string prefix)
    {
        var digits = Random.Shared.Next(0, 100_000_000).ToString("D8", System.Globalization.CultureInfo.InvariantCulture);
        return $"{prefix}-{digits}";
    }

    /// <summary>Generates a code and retries until <paramref name="existsAsync"/> returns false.</summary>
    public static async Task<string> GenerateUniqueAsync(
        string? businessName,
        Func<string, CancellationToken, Task<bool>> existsAsync,
        CancellationToken cancellationToken = default)
    {
        var prefix = GetPrefixFromBusinessName(businessName);

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var code = FormatCode(prefix);
            if (!await existsAsync(code, cancellationToken).ConfigureAwait(false))
            {
                return code;
            }
        }

        return FormatCode(prefix);
    }
}

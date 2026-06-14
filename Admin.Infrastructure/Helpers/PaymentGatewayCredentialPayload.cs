using System.Text.Json;
using System.Text.Json.Serialization;
using Admin.Data.Enums;

namespace Admin.Infrastructure.Helpers;

/// <summary>Plain JSON shape stored (then encrypted) per gateway.</summary>
public sealed class PaymentGatewayCredentialPayload
{
    [JsonPropertyName("secretKey")]
    public string? SecretKey { get; set; }

    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }

    [JsonPropertyName("contractCode")]
    public string? ContractCode { get; set; }

    public static PaymentGatewayCredentialPayload Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PaymentGatewayCredentialPayload();
        }

        return JsonSerializer.Deserialize<PaymentGatewayCredentialPayload>(json)
               ?? new PaymentGatewayCredentialPayload();
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }

    public bool IsCompleteFor(PaymentGatewayProvider gateway)
    {
        return gateway switch
        {
            PaymentGatewayProvider.Paystack => HasValue(SecretKey),
            PaymentGatewayProvider.Flutterwave => HasValue(SecretKey),
            PaymentGatewayProvider.Monify => HasValue(ApiKey) && HasValue(SecretKey) && HasValue(ContractCode),
            _ => false,
        };
    }

    public PaymentGatewayCredentialPayload MergeIncoming(
        PaymentGatewayProvider gateway,
        string? secretKey,
        string? apiKey,
        string? contractCode)
    {
        var merged = new PaymentGatewayCredentialPayload
        {
            SecretKey = ChooseValue(secretKey, SecretKey),
            ApiKey = ChooseValue(apiKey, ApiKey),
            ContractCode = ChooseValue(contractCode, ContractCode),
        };

        if (gateway is PaymentGatewayProvider.Paystack or PaymentGatewayProvider.Flutterwave)
        {
            merged.ApiKey = null;
            merged.ContractCode = null;
        }

        return merged;
    }

    private static string? ChooseValue(string? incoming, string? existing)
    {
        return !string.IsNullOrWhiteSpace(incoming) ? incoming.Trim() : existing;
    }

    private static bool HasValue(string? value) => !string.IsNullOrWhiteSpace(value);
}

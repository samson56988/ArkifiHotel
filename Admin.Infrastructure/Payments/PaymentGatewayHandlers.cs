using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Admin.Data.Enums;

namespace Admin.Infrastructure.Payments;

public sealed class PaystackGatewayHandler : IPaymentGatewayHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _http;

    public PaystackGatewayHandler(HttpClient http)
    {
        _http = http;
    }

    public PaymentGatewayProvider Provider => PaymentGatewayProvider.Paystack;

    public async Task<PaymentInitializeResult> InitializeAsync(
        PaymentInitializeContext context,
        CancellationToken cancellationToken)
    {
        var amountKobo = (long)Math.Round(context.Amount * 100m, 0, MidpointRounding.AwayFromZero);
        var payload = new
        {
            email = context.CustomerEmail,
            amount = amountKobo,
            reference = context.Reference,
            currency = context.Currency,
            callback_url = context.RedirectUrl,
            metadata = new { custom_fields = new[] { new { display_name = "Guest", variable_name = "guest_name", value = context.CustomerName } } },
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.paystack.co/transaction/initialize");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.SecretKey);
        request.Content = JsonContent.Create(payload);

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return PaymentInitializeResult.Fail(ParseMessage(body) ?? "Paystack could not start checkout.");
        }

        var parsed = JsonSerializer.Deserialize<PaystackInitResponse>(body, JsonOptions);
        var paymentUrl = parsed?.Data?.AuthorizationUrl;
        if (string.IsNullOrWhiteSpace(paymentUrl))
        {
            paymentUrl = TryReadString(body, "data", "authorization_url");
        }

        if (parsed?.Status != true || string.IsNullOrWhiteSpace(paymentUrl))
        {
            return PaymentInitializeResult.Fail(parsed?.Message ?? "Paystack did not return a checkout URL.");
        }

        return PaymentInitializeResult.Ok(paymentUrl);
    }

    public async Task<PaymentVerifyResult> VerifyAsync(PaymentVerifyContext context, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.paystack.co/transaction/verify/{Uri.EscapeDataString(context.Reference)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.SecretKey);

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return PaymentVerifyResult.Fail(ParseMessage(body) ?? "Paystack verification failed.");
        }

        var parsed = JsonSerializer.Deserialize<PaystackVerifyResponse>(body, JsonOptions);
        if (parsed?.Status != true || parsed.Data is null)
        {
            return PaymentVerifyResult.Fail(parsed?.Message ?? "Paystack verification returned no data.");
        }

        if (!string.Equals(parsed.Data.Status, "success", StringComparison.OrdinalIgnoreCase))
        {
            return PaymentVerifyResult.Fail($"Payment status: {parsed.Data.Status ?? "unknown"}.");
        }

        var paid = parsed.Data.Amount / 100m;
        if (!AmountMatches(paid, context.ExpectedAmount))
        {
            return PaymentVerifyResult.Fail("Paid amount does not match booking total.");
        }

        return PaymentVerifyResult.Ok(parsed.Data.Reference ?? context.Reference);
    }

    private static bool AmountMatches(decimal paid, decimal expected) =>
        Math.Abs(paid - expected) <= 0.01m;

    private static string? ParseMessage(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var msg))
            {
                return msg.GetString();
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static string? TryReadString(string json, params string[] path)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            JsonElement el = doc.RootElement;
            foreach (var segment in path)
            {
                if (!el.TryGetProperty(segment, out el))
                {
                    return null;
                }
            }

            return el.GetString();
        }
        catch
        {
            return null;
        }
    }

    private sealed class PaystackInitResponse
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public PaystackInitData? Data { get; set; }
    }

    private sealed class PaystackInitData
    {
        [JsonPropertyName("authorization_url")]
        public string? AuthorizationUrl { get; set; }
    }

    private sealed class PaystackVerifyResponse
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public PaystackVerifyData? Data { get; set; }
    }

    private sealed class PaystackVerifyData
    {
        public string? Status { get; set; }
        public long Amount { get; set; }
        public string? Reference { get; set; }
    }
}

public sealed class FlutterwaveGatewayHandler : IPaymentGatewayHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _http;

    public FlutterwaveGatewayHandler(HttpClient http)
    {
        _http = http;
    }

    public PaymentGatewayProvider Provider => PaymentGatewayProvider.Flutterwave;

    public async Task<PaymentInitializeResult> InitializeAsync(
        PaymentInitializeContext context,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            tx_ref = context.Reference,
            amount = context.Amount,
            currency = context.Currency,
            redirect_url = context.RedirectUrl,
            customer = new
            {
                email = context.CustomerEmail,
                name = context.CustomerName,
                phonenumber = context.CustomerPhone,
            },
            customizations = new
            {
                title = "Hotel booking",
                description = context.Description,
            },
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.flutterwave.com/v3/payments");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.SecretKey);
        request.Content = JsonContent.Create(payload);

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return PaymentInitializeResult.Fail(ParseMessage(body) ?? "Flutterwave could not start checkout.");
        }

        var parsed = JsonSerializer.Deserialize<FlutterwaveInitResponse>(body, JsonOptions);
        if (!string.Equals(parsed?.Status, "success", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(parsed.Data?.Link))
        {
            return PaymentInitializeResult.Fail(parsed?.Message ?? "Flutterwave did not return a checkout URL.");
        }

        return PaymentInitializeResult.Ok(parsed.Data.Link);
    }

    public async Task<PaymentVerifyResult> VerifyAsync(PaymentVerifyContext context, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.flutterwave.com/v3/transactions/verify_by_reference?tx_ref={Uri.EscapeDataString(context.Reference)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.SecretKey);

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return PaymentVerifyResult.Fail(ParseMessage(body) ?? "Flutterwave verification failed.");
        }

        var parsed = JsonSerializer.Deserialize<FlutterwaveVerifyResponse>(body, JsonOptions);
        if (!string.Equals(parsed?.Status, "success", StringComparison.OrdinalIgnoreCase) || parsed.Data is null)
        {
            return PaymentVerifyResult.Fail(parsed?.Message ?? "Flutterwave verification returned no data.");
        }

        if (!string.Equals(parsed.Data.Status, "successful", StringComparison.OrdinalIgnoreCase))
        {
            return PaymentVerifyResult.Fail($"Payment status: {parsed.Data.Status ?? "unknown"}.");
        }

        if (!string.Equals(parsed.Data.Currency, context.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return PaymentVerifyResult.Fail("Payment currency mismatch.");
        }

        if (Math.Abs(parsed.Data.Amount - context.ExpectedAmount) > 0.01m)
        {
            return PaymentVerifyResult.Fail("Paid amount does not match booking total.");
        }

        return PaymentVerifyResult.Ok(parsed.Data.TxRef ?? context.Reference);
    }

    private static string? ParseMessage(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var msg))
            {
                return msg.GetString();
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private sealed class FlutterwaveInitResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public FlutterwaveInitData? Data { get; set; }
    }

    private sealed class FlutterwaveInitData
    {
        public string? Link { get; set; }
    }

    private sealed class FlutterwaveVerifyResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public FlutterwaveVerifyData? Data { get; set; }
    }

    private sealed class FlutterwaveVerifyData
    {
        public string? Status { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }

        [JsonPropertyName("tx_ref")]
        public string? TxRef { get; set; }
    }
}

public sealed class MonifyGatewayHandler : IPaymentGatewayHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _http;

    public MonifyGatewayHandler(HttpClient http)
    {
        _http = http;
    }

    public PaymentGatewayProvider Provider => PaymentGatewayProvider.Monify;

    public async Task<PaymentInitializeResult> InitializeAsync(
        PaymentInitializeContext context,
        CancellationToken cancellationToken)
    {
        var token = await GetAccessTokenAsync(context.ApiKey!, context.SecretKey, cancellationToken).ConfigureAwait(false);
        if (token is null)
        {
            return PaymentInitializeResult.Fail("Monify authentication failed.");
        }

        var payload = new
        {
            amount = context.Amount,
            customerName = context.CustomerName,
            customerEmail = context.CustomerEmail,
            paymentReference = context.Reference,
            paymentDescription = context.Description,
            currencyCode = context.Currency,
            contractCode = context.ContractCode,
            redirectUrl = context.RedirectUrl,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.monnify.com/api/v1/merchant/transactions/init-transaction");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(payload);

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return PaymentInitializeResult.Fail(ParseMonifyMessage(body) ?? "Monify could not start checkout.");
        }

        var parsed = JsonSerializer.Deserialize<MonifyEnvelope<MonifyInitBody>>(body, JsonOptions);
        if (parsed?.RequestSuccessful != true || string.IsNullOrWhiteSpace(parsed.ResponseBody?.CheckoutUrl))
        {
            return PaymentInitializeResult.Fail(parsed?.ResponseMessage ?? "Monify did not return a checkout URL.");
        }

        return PaymentInitializeResult.Ok(parsed.ResponseBody!.CheckoutUrl!);
    }

    public async Task<PaymentVerifyResult> VerifyAsync(PaymentVerifyContext context, CancellationToken cancellationToken)
    {
        var token = await GetAccessTokenAsync(context.ApiKey!, context.SecretKey, cancellationToken).ConfigureAwait(false);
        if (token is null)
        {
            return PaymentVerifyResult.Fail("Monify authentication failed.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.monnify.com/api/v2/transactions/{Uri.EscapeDataString(context.Reference)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return PaymentVerifyResult.Fail(ParseMonifyMessage(body) ?? "Monify verification failed.");
        }

        var parsed = JsonSerializer.Deserialize<MonifyEnvelope<MonifyVerifyBody>>(body, JsonOptions);
        if (parsed?.RequestSuccessful != true || parsed.ResponseBody is null)
        {
            return PaymentVerifyResult.Fail(parsed?.ResponseMessage ?? "Monify verification returned no data.");
        }

        var tx = parsed.ResponseBody;
        if (!string.Equals(tx.PaymentStatus, "PAID", StringComparison.OrdinalIgnoreCase))
        {
            return PaymentVerifyResult.Fail($"Payment status: {tx.PaymentStatus ?? "unknown"}.");
        }

        if (Math.Abs(tx.AmountPaid - context.ExpectedAmount) > 0.01m)
        {
            return PaymentVerifyResult.Fail("Paid amount does not match booking total.");
        }

        return PaymentVerifyResult.Ok(tx.TransactionReference ?? tx.PaymentReference ?? context.Reference);
    }

    private async Task<string?> GetAccessTokenAsync(string apiKey, string secretKey, CancellationToken cancellationToken)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{secretKey}"));
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.monnify.com/api/v1/auth/login");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var parsed = JsonSerializer.Deserialize<MonifyEnvelope<MonifyAuthBody>>(body, JsonOptions);
        return parsed?.ResponseBody?.AccessToken;
    }

    private static string? ParseMonifyMessage(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("responseMessage", out var msg))
            {
                return msg.GetString();
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private sealed class MonifyEnvelope<T>
    {
        public bool RequestSuccessful { get; set; }
        public string? ResponseMessage { get; set; }
        public T? ResponseBody { get; set; }
    }

    private sealed class MonifyAuthBody
    {
        public string? AccessToken { get; set; }
    }

    private sealed class MonifyInitBody
    {
        public string? CheckoutUrl { get; set; }
    }

    private sealed class MonifyVerifyBody
    {
        public string? PaymentStatus { get; set; }
        public decimal AmountPaid { get; set; }
        public string? PaymentReference { get; set; }
        public string? TransactionReference { get; set; }
    }
}

public sealed class PaymentGatewayRouter
{
    private readonly IReadOnlyDictionary<PaymentGatewayProvider, IPaymentGatewayHandler> _handlers;

    public PaymentGatewayRouter(IEnumerable<IPaymentGatewayHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.Provider);
    }

    public IPaymentGatewayHandler Get(PaymentGatewayProvider provider)
    {
        if (!_handlers.TryGetValue(provider, out var handler))
        {
            throw new InvalidOperationException($"No payment handler registered for {provider}.");
        }

        return handler;
    }
}

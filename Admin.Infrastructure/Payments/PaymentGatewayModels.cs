using Admin.Data.Enums;

namespace Admin.Infrastructure.Payments;

public sealed class PaymentInitializeContext
{
    public required PaymentGatewayProvider Provider { get; init; }

    public required string SecretKey { get; init; }

    public string? ApiKey { get; init; }

    public string? ContractCode { get; init; }

    public required string Reference { get; init; }

    public required decimal Amount { get; init; }

    public required string Currency { get; init; }

    public required string CustomerEmail { get; init; }

    public required string CustomerName { get; init; }

    public required string CustomerPhone { get; init; }

    public required string RedirectUrl { get; init; }

    public required string Description { get; init; }
}

public sealed class PaymentInitializeResult
{
    public bool Success { get; init; }

    public string? PaymentUrl { get; init; }

    public string? Message { get; init; }

    public static PaymentInitializeResult Ok(string paymentUrl) =>
        new() { Success = true, PaymentUrl = paymentUrl };

    public static PaymentInitializeResult Fail(string message) =>
        new() { Success = false, Message = message };
}

public sealed class PaymentVerifyContext
{
    public required PaymentGatewayProvider Provider { get; init; }

    public required string SecretKey { get; init; }

    public string? ApiKey { get; init; }

    public string? ContractCode { get; init; }

    public required string Reference { get; init; }

    public required decimal ExpectedAmount { get; init; }

    public required string Currency { get; init; }
}

public sealed class PaymentVerifyResult
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public string? ProviderReference { get; init; }

    public static PaymentVerifyResult Ok(string? providerReference = null) =>
        new() { Success = true, ProviderReference = providerReference };

    public static PaymentVerifyResult Fail(string message) =>
        new() { Success = false, Message = message };
}

public interface IPaymentGatewayHandler
{
    PaymentGatewayProvider Provider { get; }

    Task<PaymentInitializeResult> InitializeAsync(PaymentInitializeContext context, CancellationToken cancellationToken);

    Task<PaymentVerifyResult> VerifyAsync(PaymentVerifyContext context, CancellationToken cancellationToken);
}

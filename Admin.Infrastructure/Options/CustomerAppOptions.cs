namespace Admin.Infrastructure.Options;

public sealed class CustomerAppOptions
{
    public const string SectionName = "CustomerApp";

    /// <summary>HotelCustomer SPA base URL (no trailing slash), used for payment return redirects.</summary>
    public string BaseUrl { get; set; } = "http://localhost:4201";
}

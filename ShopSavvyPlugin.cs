using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using ShopSavvy.DataApi;

namespace ShopSavvy.SemanticKernel;

/// <summary>
/// Semantic Kernel plugin for ShopSavvy product search and price comparison.
/// Provides functions for searching products, getting offers, price history, and deals.
/// </summary>
public class ShopSavvyPlugin : IDisposable
{
    private readonly ShopSavvyDataApiClient _client;
    private bool _disposed;

    /// <summary>
    /// Create a new ShopSavvyPlugin with the given API key.
    /// </summary>
    /// <param name="apiKey">Your ShopSavvy API key (get one at shopsavvy.com/data)</param>
    public ShopSavvyPlugin(string apiKey)
    {
        _client = new ShopSavvyDataApiClient(apiKey);
    }

    /// <summary>
    /// Search for products by keyword, name, or description.
    /// </summary>
    [KernelFunction("SearchProducts")]
    [Description("Search for products by keyword. Returns product details including title, brand, category, and identifiers like barcode and ASIN.")]
    public async Task<string> SearchProducts(
        [Description("Search query (product name, keyword, or description)")] string query,
        [Description("Maximum number of results to return")] int limit = 10
    )
    {
        var result = await _client.SearchProductsAsync(query, limit: limit);
        var products = result.Data.Select(p => new
        {
            p.Title,
            p.Brand,
            p.Category,
            p.Barcode,
            Asin = p.Amazon,
            ShopSavvyId = p.ShopSavvy,
        });
        return JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Get current offers from retailers for a product.
    /// </summary>
    [KernelFunction("GetOffers")]
    [Description("Get current offers from retailers for a product. Accepts barcode, ASIN, URL, model number, or ShopSavvy product ID. Returns prices sorted by price.")]
    public async Task<string> GetOffers(
        [Description("Product identifier (barcode, ASIN, URL, model number, or ShopSavvy ID)")] string identifier,
        [Description("Filter to a specific retailer (optional)")] string? retailer = null
    )
    {
        var result = await _client.GetOffersAsync(identifier, retailer: retailer);
        var output = result.Data.Select(p => new
        {
            p.Title,
            Offers = p.Offers?.Select(o => new
            {
                o.Retailer,
                o.Price,
                o.Currency,
                o.Availability,
                o.Condition,
                Url = o.URL,
            })
        });
        return JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Get historical price data for a product over a date range.
    /// </summary>
    [KernelFunction("GetPriceHistory")]
    [Description("Get historical price data for a product over a date range. Helps determine if the current price is a good deal by showing past prices.")]
    public async Task<string> GetPriceHistory(
        [Description("Product identifier (barcode, ASIN, URL, model number, or ShopSavvy ID)")] string identifier,
        [Description("Start date in YYYY-MM-DD format")] string startDate,
        [Description("End date in YYYY-MM-DD format")] string endDate,
        [Description("Filter to a specific retailer (optional)")] string? retailer = null
    )
    {
        var result = await _client.GetPriceHistoryAsync(identifier, startDate, endDate, retailer: retailer);
        return JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Browse current shopping deals with optional sorting and filtering.
    /// </summary>
    [KernelFunction("GetDeals")]
    [Description("Browse current shopping deals. Returns deals with expert grades, pricing, and community votes. Sort by 'hot', 'new', 'top-hour', 'top-day', or 'top-week'.")]
    public async Task<string> GetDeals(
        [Description("Sort order: hot, new, top-hour, top-day, top-week")] string? sort = null,
        [Description("Maximum number of deals to return")] int? limit = null,
        [Description("Filter to a specific category")] string? category = null,
        [Description("Filter to a specific retailer")] string? retailer = null
    )
    {
        var result = await _client.GetDealsAsync(sort: sort, limit: limit, category: category, retailer: retailer);
        var deals = result.Deals?.Select(d => new
        {
            d.Title,
            Grade = $"{d.Grade?.Letter}{d.Grade?.Suffix}",
            Price = d.Pricing?.Current,
            OriginalPrice = d.Pricing?.Original,
            Retailer = d.Retailer?.Name,
            d.Url,
            Score = d.Votes?.Score,
        });
        return JsonSerializer.Serialize(deals, new JsonSerializerOptions { WriteIndented = true });
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _client.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

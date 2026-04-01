# Semantic Kernel + ShopSavvy

A Microsoft Semantic Kernel plugin for product search and price comparison using the [ShopSavvy Data API](https://shopsavvy.com/data).

## Installation

Add the NuGet package:

```bash
dotnet add package ShopSavvy.SemanticKernel
```

Or add the project reference directly.

## Setup

Get an API key at [shopsavvy.com/data](https://shopsavvy.com/data).

```csharp
using Microsoft.SemanticKernel;
using ShopSavvy.SemanticKernel;

var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion("gpt-4o", Environment.GetEnvironmentVariable("OPENAI_API_KEY")!);
builder.Plugins.AddFromObject(
    new ShopSavvyPlugin(Environment.GetEnvironmentVariable("SHOPSAVVY_API_KEY")!)
);

var kernel = builder.Build();
```

## Usage

```csharp
var result = await kernel.InvokePromptAsync(
    "Find the best price for Sony WH-1000XM5 headphones"
);
Console.WriteLine(result);
```

## Plugin Functions

### SearchProducts

Search for products by keyword. Returns product details including title, brand, and identifiers.

```csharp
var plugin = new ShopSavvyPlugin(apiKey);
var results = await plugin.SearchProducts("sony headphones", limit: 5);
```

### GetOffers

Get current offers from retailers for a product by barcode, ASIN, URL, or model number.

```csharp
var offers = await plugin.GetOffers("B09XS7JWHH");
```

### GetPriceHistory

Get historical price data for a product over a date range.

```csharp
var history = await plugin.GetPriceHistory("B09XS7JWHH", "2024-01-01", "2024-06-01");
```

### GetDeals

Browse current shopping deals with optional sorting and filtering.

```csharp
var deals = await plugin.GetDeals(sort: "hot", limit: 10);
```

## Building

```bash
dotnet build
```

## License

MIT

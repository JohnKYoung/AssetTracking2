using System;
using System.Globalization;
using System.Linq;
using AssetTracking2.Data;
using AssetTracking2.Models;
using Microsoft.EntityFrameworkCore;

internal static class Program
{
    private const int AssetLifetimeYears = 3;
    private const int IdColumnWidth = 4;
    private const int CategoryColumnWidth = 10;
    private const int AssetTypeColumnWidth = 15;
    private const int BrandColumnWidth = 12;
    private const int ModelColumnWidth = 15;
    private const int PurchaseDateColumnWidth = 13;
    private const int OfficeColumnWidth = 10;

    private static readonly CultureInfo UsCulture = CultureInfo.GetCultureInfo("en-US");

    private static void Main()
    {
        // Create the database and tables if they do not exist yet.
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
        }

        // Check and display every asset's current lifetime status at startup.
        ShowAllAssets();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("================ ASSET TRACKING SYSTEM (LEVEL 2) ================");
            Console.WriteLine("1. Add Asset");
            Console.WriteLine("2. Show All Assets");
            Console.WriteLine("3. Update Asset");
            Console.WriteLine("4. Delete Asset");
            Console.WriteLine("5. Exit");
            Console.Write("Select an option (1-5): ");

            switch (Console.ReadLine())
            {
                case "1":
                    AddAsset();
                    break;
                case "2":
                    ShowAllAssets();
                    break;
                case "3":
                    UpdateAsset();
                    break;
                case "4":
                    DeleteAsset();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Invalid option. Press any key to try again.");
                    Pause();
                    break;
            }
        }
    }

    private static void AddAsset()
    {
        Console.Clear();
        Console.WriteLine("=== ADD NEW ASSET ===");

        var asset = ReadNewAssetType();
        asset.Brand = ReadRequiredText("Brand");
        asset.ModelName = ReadRequiredText("Model Name");
        asset.PurchaseDate = ReadPurchaseDate();
        asset.PurchasePriceUSD = ReadPurchasePrice();
        asset.OfficeLocation = ReadRequiredText("Office Location");
        asset.SerialNumber = ReadRequiredText("Serial Number");

        using (var db = new AppDbContext())
        {
            db.Assets.Add(asset);
            db.SaveChanges();
        }

        Console.WriteLine("\nAsset successfully saved. Press any key to return to the menu.");
        Pause();
    }

    private static Asset ReadNewAssetType()
    {
        while (true)
        {
            Console.WriteLine("Select Asset Type:");
            Console.WriteLine("1. Laptop");
            Console.WriteLine("2. Desktop Computer");
            Console.WriteLine("3. iPhone");
            Console.WriteLine("4. Samsung");
            Console.WriteLine("5. Nokia");
            Console.WriteLine("6. Tablet");
            Console.Write("Enter choice (1-6): ");

            Asset? asset = Console.ReadLine() switch
            {
                "1" => new Laptop(),
                "2" => new DesktopComputer(),
                "3" => new IPhone(),
                "4" => new Samsung(),
                "5" => new Nokia(),
                "6" => new Tablet(),
                _ => null
            };

            if (asset is not null)
            {
                return asset;
            }

            Console.WriteLine("Invalid asset type. Please choose a number from 1 to 6.\n");
        }
    }

    private static void ShowAllAssets()
    {
        Console.Clear();
        Console.WriteLine("============================== ASSET LIST ==============================");

        using (var db = new AppDbContext())
        {
            // Group by category, then show older purchases first within each category.
            var assets = db.Assets
                .AsNoTracking()
                .OrderBy(asset => asset.AssetType)
                .ThenBy(asset => asset.PurchaseDate)
                .ThenBy(asset => asset.Id)
                .ToList();

            const string priceHeader = "Price ($)";
            var prices = assets
                .Select(asset => asset.PurchasePriceUSD.ToString("C2", UsCulture))
                .ToArray();
            var priceWidth = Math.Max(
                priceHeader.Length,
                prices.Select(price => price.Length).DefaultIfEmpty(0).Max());
            var tableWidth =
                IdColumnWidth +
                CategoryColumnWidth +
                AssetTypeColumnWidth +
                BrandColumnWidth +
                ModelColumnWidth +
                PurchaseDateColumnWidth +
                priceWidth +
                OfficeColumnWidth +
                3 * 7;

            Console.WriteLine(string.Join(" | ", new[]
            {
                "ID".PadRight(IdColumnWidth),
                "Category".PadRight(CategoryColumnWidth),
                "Asset Type".PadRight(AssetTypeColumnWidth),
                "Brand".PadRight(BrandColumnWidth),
                "Model".PadRight(ModelColumnWidth),
                "Purchase Date".PadRight(PurchaseDateColumnWidth),
                priceHeader.PadRight(priceWidth),
                "Office".PadRight(OfficeColumnWidth)
            }));
            Console.WriteLine(new string('-', tableWidth));

            if (assets.Count == 0)
            {
                Console.WriteLine("No assets found in database.");
            }
            else
            {
                var today = DateTime.Today;
                for (var index = 0; index < assets.Count; index++)
                {
                    WriteAssetRow(assets[index], prices[index], today, priceWidth);
                }
            }
        }

        Console.WriteLine("\nPress any key to return to the menu.");
        Pause();
    }

    private static void WriteAssetRow(Asset asset, string formattedPrice, DateTime today, int priceWidth)
    {
        var status = GetLifetimeStatus(asset, today);
        var row = string.Join(" | ", new[]
        {
            asset.Id.ToString(CultureInfo.InvariantCulture).PadRight(IdColumnWidth),
            asset.AssetType.PadRight(CategoryColumnWidth),
            asset.GetType().Name.PadRight(AssetTypeColumnWidth),
            asset.Brand.PadRight(BrandColumnWidth),
            asset.ModelName.PadRight(ModelColumnWidth),
            asset.PurchaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                .PadRight(PurchaseDateColumnWidth),
            formattedPrice.PadLeft(priceWidth),
            asset.OfficeLocation.PadRight(OfficeColumnWidth)
        });

        var originalColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = GetStatusColor(status, originalColor);
            Console.WriteLine(row);
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }

    private static ConsoleColor GetStatusColor(LifetimeStatus status, ConsoleColor defaultColor)
    {
        return status switch
        {
            LifetimeStatus.Yellow => ConsoleColor.Yellow,
            LifetimeStatus.Red => ConsoleColor.Red,
            _ => defaultColor
        };
    }

    private static LifetimeStatus GetLifetimeStatus(Asset asset, DateTime today)
    {
        var endOfLifetime = asset.PurchaseDate.Date.AddYears(AssetLifetimeYears);

        // The brief's thresholds overlap; its order makes under 3 months YELLOW,
        // 3 to under 6 months RED, and 6 months or more NORMAL.
        if (endOfLifetime < today.AddMonths(3))
        {
            return LifetimeStatus.Yellow;
        }

        if (endOfLifetime < today.AddMonths(6))
        {
            return LifetimeStatus.Red;
        }

        return LifetimeStatus.Normal;
    }

    private static void UpdateAsset()
    {
        Console.Clear();
        Console.WriteLine("=== UPDATE ASSET ===");

        using (var db = new AppDbContext())
        {
            var id = ReadAssetId("Enter the ID of the asset to update: ");
            var asset = db.Assets.Find(id);
            if (asset is null)
            {
                Console.WriteLine($"No asset with ID {id} was found. Press any key to return.");
                Pause();
                return;
            }

            Console.WriteLine($"Updating {asset.GetType().Name} (ID {asset.Id}).");
            Console.WriteLine("Press Enter at a prompt to keep its current value.\n");

            asset.Brand = ReadRequiredText("Brand", asset.Brand);
            asset.ModelName = ReadRequiredText("Model Name", asset.ModelName);
            var previousPurchaseDate = asset.PurchaseDate.Date;
            var previousStatus = GetLifetimeStatus(asset, DateTime.Today);
            asset.PurchaseDate = ReadPurchaseDate(asset.PurchaseDate);
            var purchaseDateChanged = asset.PurchaseDate.Date != previousPurchaseDate;
            asset.PurchasePriceUSD = ReadPurchasePrice(asset.PurchasePriceUSD);
            asset.OfficeLocation = ReadRequiredText("Office Location", asset.OfficeLocation);
            asset.SerialNumber = ReadRequiredText("Serial Number", asset.SerialNumber);

            db.SaveChanges();

            if (purchaseDateChanged)
            {
                var updatedStatus = GetLifetimeStatus(asset, DateTime.Today);
                Console.Write("\nLifetime status after date update: ");
                WriteColoredStatus(updatedStatus);
                Console.WriteLine(updatedStatus == previousStatus
                    ? " (unchanged)"
                    : $" (changed from {previousStatus.ToString().ToUpperInvariant()})");
            }
        }

        Console.WriteLine("\nAsset successfully updated. Press any key to return to the menu.");
        Pause();
    }

    private static void WriteColoredStatus(LifetimeStatus status)
    {
        var originalColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = GetStatusColor(status, originalColor);
            Console.Write(status.ToString().ToUpperInvariant());
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }

    private static void DeleteAsset()
    {
        Console.Clear();
        Console.WriteLine("=== DELETE ASSET ===");

        using (var db = new AppDbContext())
        {
            var id = ReadAssetId("Enter the ID of the asset to delete: ");
            var asset = db.Assets.Find(id);
            if (asset is null)
            {
                Console.WriteLine($"No asset with ID {id} was found. Press any key to return.");
                Pause();
                return;
            }

            Console.Write($"Delete {asset.GetType().Name} (ID {asset.Id}, {asset.Brand} {asset.ModelName})? (y/n): ");
            if (!string.Equals(Console.ReadLine()?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Delete cancelled. Press any key to return.");
                Pause();
                return;
            }

            db.Assets.Remove(asset);
            db.SaveChanges();
        }

        Console.WriteLine("\nAsset successfully deleted. Press any key to return to the menu.");
        Pause();
    }

    private static int ReadAssetId(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out var id) && id > 0)
            {
                return id;
            }

            Console.WriteLine("Enter a positive whole-number ID.");
        }
    }

    private static string ReadRequiredText(string fieldName, string? currentValue = null)
    {
        while (true)
        {
            if (currentValue is null)
            {
                Console.Write($"Enter {fieldName}: ");
            }
            else
            {
                Console.Write($"Enter {fieldName} [{currentValue}] (Enter to keep): ");
            }

            var input = Console.ReadLine();
            if (currentValue is not null && string.IsNullOrWhiteSpace(input))
            {
                return currentValue;
            }

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            Console.WriteLine($"{fieldName} cannot be empty.");
        }
    }

    private static DateTime ReadPurchaseDate(DateTime? currentValue = null)
    {
        while (true)
        {
            Console.Write(currentValue.HasValue
                ? $"Enter Purchase Date (YYYY-MM-DD) [{currentValue.Value:yyyy-MM-dd}] (Enter to keep): "
                : "Enter Purchase Date (YYYY-MM-DD): ");

            var input = Console.ReadLine()?.Trim();
            if (currentValue.HasValue && string.IsNullOrEmpty(input))
            {
                return currentValue.Value;
            }

            if (!string.IsNullOrEmpty(input) &&
                DateTime.TryParseExact(
                    input,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var purchaseDate) &&
                purchaseDate.Date <= DateTime.Today)
            {
                return purchaseDate.Date;
            }

            Console.WriteLine("Enter a valid date in YYYY-MM-DD format that is not in the future.");
        }
    }

    private static decimal ReadPurchasePrice(decimal? currentValue = null)
    {
        while (true)
        {
            Console.Write(currentValue.HasValue
                ? $"Enter Purchase Price (USD) [{currentValue.Value.ToString("F2", UsCulture)}] (Enter to keep): "
                : "Enter Purchase Price (USD): ");

            var input = Console.ReadLine()?.Trim();
            if (currentValue.HasValue && string.IsNullOrEmpty(input))
            {
                return currentValue.Value;
            }

            if (decimal.TryParse(input, NumberStyles.Number, UsCulture, out var price) && price >= 0)
            {
                return price;
            }

            Console.WriteLine("Enter a valid, non-negative price using digits and an optional decimal point.");
        }
    }

    private static void Pause()
    {
        Console.ReadKey();
    }

    private enum LifetimeStatus
    {
        Yellow,
        Red,
        Normal
    }
}

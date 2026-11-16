using System;
using System.Globalization;
using System.Linq;
using AssetTracking2.Data;
using AssetTracking2.Models;

class Program
{
    static void Main(string[] args)
    {
        // This is the app's entry point. EnsureCreated creates the database and tables
        // if they are missing; it does not apply EF Core migrations.
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
        }

        // Keep showing the menu until the user chooses Exit.
        bool exit = false;
        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("================ ASSET TRACKING SYSTEM (LEVEL 1) ================");
            Console.WriteLine("1. Add Asset");
            Console.WriteLine("2. Show All Assets");
            Console.WriteLine("3. Exit");
            Console.Write("Select an option (1-3): ");

            string? choice = Console.ReadLine();
            // Route the menu choice to the matching action.
            switch (choice)
            {
                case "1":
                    AddAsset();
                    break;
                case "2":
                    ShowAllAssets();
                    break;
                case "3":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Invalid option. Press any key to try again.");
                    Console.ReadKey();
                    break;
            }
        }
    }

    static void AddAsset()
    {
        Console.Clear();
        Console.WriteLine("=== ADD NEW ASSET ===");
        Console.WriteLine("Select Asset Type:");
        Console.WriteLine("1. Laptop");
        Console.WriteLine("2. Desktop Computer");
        Console.WriteLine("3. iPhone");
        Console.WriteLine("4. Samsung");
        Console.WriteLine("5. Nokia");
        Console.WriteLine("6. Tablet");
        Console.Write("Enter choice (1-6): ");

        string? typeChoice = Console.ReadLine();

        // Each menu choice creates a specific asset type. All of these types inherit
        // the shared fields from Asset so they can be saved through the same DbSet.
        Asset? asset = typeChoice switch
        {
            "1" => new Laptop(),
            "2" => new DesktopComputer(),
            "3" => new IPhone(),
            "4" => new Samsung(),
            "5" => new Nokia(),
            "6" => new Tablet(),
            _ => null
        };

        if (asset == null)
        {
            // An unrecognized choice has no matching asset type, so return to the menu.
            Console.WriteLine("Invalid asset type. Press any key to return.");
            Console.ReadKey();
            return;
        }

        Console.Write("Enter Brand: ");
        asset.Brand = Console.ReadLine() ?? "Unknown";

        Console.Write("Enter Model Name: ");
        asset.ModelName = Console.ReadLine() ?? "Unknown";

        Console.Write("Enter Purchase Date (YYYY-MM-DD): ");
        // TryParse handles invalid input without throwing an exception.
        if (DateTime.TryParse(Console.ReadLine(), out DateTime pDate))
        {
            asset.PurchaseDate = pDate;
        }
        else
        {
            asset.PurchaseDate = DateTime.Now;
            Console.WriteLine("Invalid date format. Defaulted to today.");
        }

        Console.Write("Enter Purchase Price (USD): ");
        if (decimal.TryParse(Console.ReadLine(), out decimal price))
        {
            asset.PurchasePriceUSD = price;
        }
        else
        {
            asset.PurchasePriceUSD = 0;
        }

        Console.Write("Enter Office Location (e.g., Sweden, USA): ");
        asset.OfficeLocation = Console.ReadLine() ?? "HQ";

        Console.Write("Enter Serial Number: ");
        asset.SerialNumber = Console.ReadLine() ?? "SN000";

        using (var db = new AppDbContext())
        {
            // Add marks the new object for insertion; SaveChanges sends it to SQL Server.
            db.Assets.Add(asset);
            db.SaveChanges();
        }

        Console.WriteLine("\nAsset successfully saved to database! Press any key to continue.");
        Console.ReadKey();
    }

    static void ShowAllAssets()
    {
        Console.Clear();
        Console.WriteLine("==================================== ASSET LIST =====================================");

        using (var db = new AppDbContext())
        {
            // ToList runs the database query and loads the matching assets into memory.
            var assets = db.Assets.ToList();
            const string priceHeader = "Price ($)";
            // Format prices as USD first, then size the column for the widest value.
            // This keeps the pipe separators aligned even when a price has more digits.
            var prices = assets
                .Select(a => a.PurchasePriceUSD.ToString("C2", CultureInfo.GetCultureInfo("en-US")))
                .ToArray();
            var priceWidth = Math.Max(
                priceHeader.Length,
                prices.Select(price => price.Length).DefaultIfEmpty(0).Max());
            const int idWidth = 4;
            const int typeWidth = 10;
            const int brandWidth = 12;
            const int modelWidth = 15;
            const int purchaseDateWidth = 13;
            const int officeWidth = 10;
            var tableWidth = idWidth + typeWidth + brandWidth + modelWidth +
                purchaseDateWidth + priceWidth + officeWidth + 3 * 6;

            // Pad each field to its column width; the " | " separator is shared by
            // the header and data rows so their columns line up.
            Console.WriteLine(string.Join(" | ", new[]
            {
                "ID".PadRight(idWidth),
                "Type".PadRight(typeWidth),
                "Brand".PadRight(brandWidth),
                "Model".PadRight(modelWidth),
                "Purchase Date".PadRight(purchaseDateWidth),
                priceHeader.PadRight(priceWidth),
                "Office".PadRight(officeWidth)
            }));
            Console.WriteLine(new string('-', tableWidth));

            if (!assets.Any())
            {
                Console.WriteLine("No assets found in database.");
            }
            else
            {
                for (var index = 0; index < assets.Count; index++)
                {
                    var a = assets[index];
                    Console.WriteLine(string.Join(" | ", new[]
                    {
                        a.Id.ToString().PadRight(idWidth),
                        a.AssetType.PadRight(typeWidth),
                        a.Brand.PadRight(brandWidth),
                        a.ModelName.PadRight(modelWidth),
                        a.PurchaseDate.ToShortDateString().PadRight(purchaseDateWidth),
                        prices[index].PadLeft(priceWidth),
                        a.OfficeLocation.PadRight(officeWidth)
                    }));
                }
            }
        }

        Console.WriteLine("\nPress any key to return to menu.");
        Console.ReadKey();
    }
}
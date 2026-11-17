# AssetTracking2

AssetTracking2 is a .NET 10 console application for recording and viewing computer and mobile assets. It uses Entity Framework Core (EF Core) to store data in SQL Server.

> This is currently a console application; it does not yet have a web interface.

## Features

- Create, read, update, and delete laptops, desktop computers, iPhones, Samsung devices, Nokia devices, and tablets.
- Store each asset's type, brand, model, purchase date, price in USD, office location, and serial number.
- Validate required text, purchase dates, prices, and asset IDs.
- List assets grouped by category and sorted by purchase date.
- At startup and whenever the asset list is displayed, calculate each asset's status from its three-year lifetime and color the entire row: YELLOW under 3 months, RED from 3 to under 6 months, and NORMAL at 6 months or more (default terminal color).
- After changing a purchase date, immediately display the asset's recalculated status and whether it changed.

## Project structure

```text
AssetTracking2/
├── Data/
│   └── AppDbContext.cs       # EF Core database configuration
├── Models/
│   ├── Asset.cs              # Shared asset properties
│   ├── ComputerAsset.cs      # Shared computer category
│   ├── MobileAsset.cs        # Shared mobile category
│   └── ConcreteAssets.cs     # Laptop, phone, desktop, and tablet types
├── Program.cs                # Console menu and application flow
├── AssetTracking2.csproj     # .NET target and NuGet dependencies
└── README.md
```

## Requirements

- .NET 10 SDK
- A running SQL Server instance. The current configuration expects a Docker-hosted SQL Server reachable at `localhost,1433`.

## Configure the database

Before running the app, set the `ASSETTRACKING_SQL_CONNECTION_STRING` environment variable to a connection string for your SQL Server container. It should include the server address, database name, and login details. The app expects the database name `AssetTrackingDb`.

The app uses a database named `AssetTrackingDb`. On startup, `EnsureCreated()` creates the database and tables if they do not already exist. It does **not** apply EF Core migrations or update an existing schema when the models change.

Do not store real database passwords in this README or commit them in source code. Keep the connection string in a local environment variable or another secure configuration store.

## Build and run

From the project directory:

```bash
dotnet restore
dotnet build
dotnet run
```

The first run needs a working SQL Server connection. On startup, the app displays the current asset list and lifetime-status colors. Press any key to reach the menu:

1. Choose **1** to add an asset.
2. Choose **2** to view assets, sorted by category and purchase date.
3. Choose **3** to update an asset by ID. Leave a field blank to keep its current value.
4. Choose **4** to delete an asset by ID; confirm the deletion when prompted.
5. Choose **5** to exit.

## Database layout

EF Core stores the asset hierarchy in the `dbo.Assets` table. The `Discriminator` column identifies the specific model type (for example, `Laptop` or `IPhone`).

To inspect records in a SQL client connected to `AssetTrackingDb`, run:

```sql
SELECT TOP (1000) *
FROM dbo.Assets
ORDER BY Id;
```

## License

This project is licensed under the [MIT License](https://opensource.org/license/mit).

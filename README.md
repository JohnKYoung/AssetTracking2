# AssetTracking2

AssetTracking2 is a .NET 10 console application for recording and viewing computer and mobile assets. It uses Entity Framework Core (EF Core) to store data in SQL Server.

> This is currently a console application; it does not yet have a web interface.

## Features

- Add laptops, desktop computers, iPhones, Samsung devices, Nokia devices, and tablets.
- Store each asset's type, brand, model, purchase date, price in USD, office location, and serial number.
- List saved assets in a formatted table.

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

The first run needs a working SQL Server connection. Once the menu appears:

1. Choose **1** to add an asset and enter its details.
2. Choose **2** to view saved assets.
3. Choose **3** to exit.

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

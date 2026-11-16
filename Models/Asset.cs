using System;

namespace AssetTracking2.Models
{
    // Common data shared by every tracked item. Abstract means the app creates
    // specific asset types (such as Laptop), rather than a plain Asset.
    public abstract class Asset
    {
        // EF Core recognizes Id by convention as this entity's primary key.
        public int Id { get; set; }

        // Basic inventory details stored for all asset types.
        public string AssetType { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePriceUSD { get; set; }
        public string OfficeLocation { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
    }
}

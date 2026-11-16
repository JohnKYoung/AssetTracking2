namespace AssetTracking2.Models
{
    // Shared parent for phones and tablets; concrete mobile classes inherit Asset's fields.
    public abstract class MobileAsset : Asset
    {
        protected MobileAsset()
        {
            // Set a broad category that can be used when displaying or filtering assets.
            AssetType = "Mobile";
        }
    }
}

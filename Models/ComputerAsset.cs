namespace AssetTracking2.Models
{
    // Shared parent for computer devices; concrete computer classes inherit Asset's fields.
    public abstract class ComputerAsset : Asset
    {
        protected ComputerAsset()
        {
            // Set a broad category that can be used when displaying or filtering assets.
            AssetType = "Computer";
        }
    }
}

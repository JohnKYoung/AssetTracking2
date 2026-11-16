namespace AssetTracking2.Models
{
    // These are the specific asset types the menu can create. They inherit shared
    // properties and category behavior from ComputerAsset or MobileAsset.
    public class Laptop : ComputerAsset
    {
        public Laptop() { }
    }

    public class DesktopComputer : ComputerAsset
    {
        public DesktopComputer() { }
    }

    public class IPhone : MobileAsset
    {
        public IPhone() { Brand = "Apple"; }
    }

    public class Samsung : MobileAsset
    {
        public Samsung() { Brand = "Samsung"; }
    }

    public class Nokia : MobileAsset
    {
        public Nokia() { Brand = "Nokia"; }
    }

    public class Tablet : MobileAsset
    {
        public Tablet() { }
    }
}

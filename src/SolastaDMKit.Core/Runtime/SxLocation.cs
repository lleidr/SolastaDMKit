namespace SolastaDMKit.Core.Runtime;

public static class SxLocation
{
    public static string CurrentTitle
    {
        get
        {
            var userLocation = Gui.GameLocation?.UserLocation;
            return userLocation?.Title ?? string.Empty;
        }
    }

    public static bool IsUserLocation => Gui.GameLocation?.UserLocation != null;

    public static UserLocation Current => Gui.GameLocation?.UserLocation;
}

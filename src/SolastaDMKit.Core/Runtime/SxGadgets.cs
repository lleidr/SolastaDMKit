using System.Collections.Generic;
using System.Linq;

namespace SolastaDMKit.Core.Runtime;

public static class SxGadgets
{
    private static readonly List<GameLocationCharacter> EmptyActors = new();

    public static GameGadget FindByUniqueName(string uniqueName)
    {
        if (string.IsNullOrEmpty(uniqueName) || Gui.GameLocation == null)
        {
            return null;
        }

        foreach (var sector in Gui.GameLocation.GameSectors)
        {
            foreach (var gadget in sector.GameGadgets)
            {
                if (gadget.UniqueNameId == uniqueName)
                {
                    return gadget;
                }
            }
        }

        return null;
    }

    public static IEnumerable<GameGadget> AllInCurrentLocation()
    {
        if (Gui.GameLocation == null)
        {
            return Enumerable.Empty<GameGadget>();
        }

        return Gui.GameLocation.GameSectors.SelectMany(s => s.GameGadgets);
    }

    public static bool IsEnabled(GameGadget gadget)
    {
        return gadget != null && gadget.CheckConditionName("Param_Enabled", true, true);
    }

    public static bool IsActivated(GameGadget gadget)
    {
        return gadget != null && gadget.CheckConditionName("Param_Activated", true, false);
    }

    public static bool CheckCondition(GameGadget gadget, string conditionName, bool expectedValue = true, bool defaultIfMissing = false)
    {
        return gadget != null && gadget.CheckConditionName(conditionName, expectedValue, defaultIfMissing);
    }

    public static void SetCondition(GameGadget gadget, int conditionIndex, bool state, IList<GameLocationCharacter> actingCharacters = null)
    {
        if (gadget == null)
        {
            return;
        }

        gadget.SetCondition(conditionIndex, state, actingCharacters as List<GameLocationCharacter> ?? EmptyActors);
    }
}

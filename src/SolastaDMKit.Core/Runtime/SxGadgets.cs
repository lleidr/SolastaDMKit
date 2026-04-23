using System;
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

    public static WorldGadget GetWorldGadget(GameGadget gadget)
    {
        if (gadget == null)
        {
            return null;
        }

        var service = ServiceRepository.GetService<IGameLocationService>();
        if (service?.WorldLocation == null)
        {
            return null;
        }

        foreach (var sector in service.WorldLocation.WorldSectors)
        {
            foreach (var wg in sector.WorldGadgets)
            {
                if (wg.GameGadget == gadget)
                {
                    return wg;
                }
            }
        }

        return null;
    }

    public static int ConditionIndexOf(GameGadget gadget, string conditionName)
    {
        var names = gadget?.conditionNames;
        return names == null ? -1 : names.IndexOf(conditionName);
    }

    public static string ConditionNameAt(GameGadget gadget, int conditionIndex)
    {
        var names = gadget?.conditionNames;
        if (names == null || conditionIndex < 0 || conditionIndex >= names.Count)
        {
            return null;
        }

        return names[conditionIndex];
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

    public static bool SetCondition(GameGadget gadget, int conditionIndex, bool state, IList<GameLocationCharacter> actingCharacters = null)
    {
        if (gadget == null)
        {
            return false;
        }

        gadget.SetCondition(conditionIndex, state, actingCharacters as List<GameLocationCharacter> ?? EmptyActors);
        return true;
    }

    public static bool SetConditionByName(GameGadget gadget, string conditionName, bool state)
    {
        var index = ConditionIndexOf(gadget, conditionName);
        if (index < 0)
        {
            return false;
        }

        return SetCondition(gadget, index, state);
    }

    public static bool Enable(GameGadget gadget) => SetConditionByName(gadget, "Param_Enabled", true);
    public static bool Disable(GameGadget gadget) => SetConditionByName(gadget, "Param_Enabled", false);
    public static bool Activate(GameGadget gadget) => SetConditionByName(gadget, "Param_Activated", true);
    public static bool Deactivate(GameGadget gadget) => SetConditionByName(gadget, "Param_Activated", false);
}

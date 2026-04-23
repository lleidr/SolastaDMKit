using HarmonyLib;
using SolastaDMKit.Core.Events;
using SolastaDMKit.Core.Runtime;

namespace SolastaDMKit.Core.Patches;

[HarmonyPatch(typeof(GameGadget), nameof(GameGadget.SetCondition))]
internal static class GameGadgetSetConditionPatch
{
    internal static bool Prefix(GameGadget __instance, int conditionIndex, bool state)
    {
        var evt = new ObjectInteracted
        {
            Gadget = __instance,
            ConditionIndex = conditionIndex,
            ConditionName = SxGadgets.ConditionNameAt(__instance, conditionIndex),
            NewState = state,
        };
        EventBus.Publish(evt);
        return !evt.Skip;
    }
}

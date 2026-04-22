using HarmonyLib;
using SolastaDMKit.Core.Events;

namespace SolastaDMKit.Core.Patches;

[HarmonyPatch(typeof(CharacterActionMoveStepWalk), nameof(CharacterActionMoveStepWalk.ChangeStartProneStatusIfNecessary))]
internal static class CharacterActionMoveStepWalkStartPatch
{
    internal static void Prefix(
        CharacterActionMoveStepWalk __instance,
        CharacterActionMoveStepWalk.MoveStep currentStep)
    {
        var mover = __instance.ActingCharacter;
        EventBus.Publish(new TileEntered
        {
            Character = mover,
            Source = mover.LocationPosition,
            Destination = currentStep.position,
        });
    }
}

using HarmonyLib;
using SolastaDMKit.Core.Events;

namespace SolastaDMKit.Core.Patches;

[HarmonyPatch(typeof(GameLocationCharacter), nameof(GameLocationCharacter.StartBattleTurn))]
internal static class GameLocationCharacterStartBattleTurnPatch
{
    internal static void Postfix(GameLocationCharacter __instance)
    {
        EventBus.Publish(new TurnStarted { Character = __instance });
    }
}

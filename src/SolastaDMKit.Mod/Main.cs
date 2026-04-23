using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SolastaDMKit.Core.Diagnostics;
using SolastaDMKit.Core.Events;
using SolastaDMKit.Core.Runtime;
using UnityModManagerNet;

namespace SolastaDMKit;

internal static class Main
{
    private static Harmony _harmony;

    internal static UnityModManager.ModEntry ModEntry { get; private set; }

    internal static string ModFolder =>
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    internal static void Log(string message) =>
        ModEntry.Logger.Log(message);

    internal static bool Load(UnityModManager.ModEntry modEntry)
    {
        ModEntry = modEntry;

        Log("Hello from SolastaDMKit!");

        SxLog.InfoCallback = ModEntry.Logger.Log;
        SxLog.ErrorCallback = (msg, ex) => ModEntry.Logger.Error(ex == null ? msg : $"{msg}\n{ex}");

        _harmony = new Harmony(modEntry.Info.Id);
        _harmony.PatchAll(typeof(EventBus).Assembly);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());

        WireDevSubscriptions();

        Log("SolastaDMKit loaded.");
        return true;
    }

    private static bool _shownWelcomeModal;

    private static void WireDevSubscriptions()
    {
        EventBus.Subscribe<TileEntered>(_ =>
        {
            if (_shownWelcomeModal)
            {
                return;
            }

            _shownWelcomeModal = true;
            SxUI.Log($"[SolastaDMKit] Runtime ready. Party size = {SxParty.Count}.");
            SxUI.ShowChoice(
                "SolastaDMKit Stage 2A smoke test.\n\nPick a button — the choice will be logged to UMM's log.",
                new[] { "Option A", "Option B", "Option C" },
                choice => Log($"[SxUI] Modal choice = {choice}"));
        });

        EventBus.Subscribe<ObjectInteracted>(e =>
        {
            Log($"[Event] ObjectInteracted condIdx={e.ConditionIndex} name='{e.ConditionName ?? "?"}' state={e.NewState}");
            if (e.Gadget != null)
            {
                var roundTrip = SxGadgets.FindByUniqueName(e.Gadget.UniqueNameId);
                Log($"[SxGadgets] lookup '{e.Gadget.UniqueNameId}' -> {(roundTrip != null ? "OK" : "NOT FOUND")}, enabled={SxGadgets.IsEnabled(e.Gadget)}, activated={SxGadgets.IsActivated(e.Gadget)}");
            }
        });

        EventBus.Subscribe<DamageAbout>(e =>
            Log($"[Event] DamageAbout amount={e.RolledDamage} type={e.DamageType}"));

        EventBus.Subscribe<DamageApplied>(e =>
            Log($"[Event] DamageApplied amount={e.RolledDamage} type={e.DamageType}"));

        EventBus.Subscribe<TurnStarted>(e =>
        {
            var name = SxCharacters.GetName(e.Character);
            Log($"[Event] TurnStarted character={name}");

            var total = SxVariables.GetInt("dmkit_turn_counter", 0) + 1;
            SxVariables.SetInt("dmkit_turn_counter", total);
            Log($"[SxVariables] dmkit_turn_counter = {total}");

            var firstMember = SxParty.Members().FirstOrDefault();
            if (firstMember != null && e.Character == firstMember)
            {
                var result = SxCharacters.RollSkillCheck(firstMember, "Perception", 10);
                Log($"[SxCharacters] Perception DC 10 -> {result}");
            }
        });
    }
}

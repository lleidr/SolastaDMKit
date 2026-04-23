using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SolastaDMKit.Core.Diagnostics;
using SolastaDMKit.Core.Events;
using SolastaDMKit.Core.Runtime;
using UnityEngine;
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

            var propCount = SxProps.BlueprintCount;
            var firstBp = SxProps.AllBlueprints().FirstOrDefault();
            var firstBpInfo = firstBp == null
                ? "none"
                : $"{firstBp.Name} (env count={firstBp.PrefabsByEnvironment?.Count ?? 0})";
            Log($"[SxProps] PropBlueprint DB: {propCount} entries; first='{firstBpInfo}'");

            var sceneObjects = SxProps.AllInCurrentLocation().ToList();
            var samples = string.Join(", ", sceneObjects.Take(5).Select(go => go.name));
            Log($"[SxProps] Scene children under world sectors: {sceneObjects.Count}; sample: {samples}");

            SxUI.ShowChoice(
                "SolastaDMKit 2C — pick a test:",
                new[]
                {
                    "Skip / log only",
                    "SpawnAsync only ('Rock_C')",
                    "Full SxProps suite (spawn + clone + hide + show + destroy)",
                },
                choice =>
                {
                    Log($"[SxUI] Modal choice = {choice}");
                    switch (choice)
                    {
                        case 1:
                            RunSpawnOnlyTest();
                            break;
                        case 2:
                            RunFullPropsTestSuite();
                            break;
                    }
                });
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

    private static Vector3 PickTestPosition()
    {
        var service = ServiceRepository.GetService<IGameLocationCharacterService>();
        var first = service?.PartyCharacters?.FirstOrDefault();
        if (first != null)
        {
            var lp = first.LocationPosition;
            return new Vector3(lp.x, lp.y, lp.z);
        }

        return Vector3.zero;
    }

    private static void RunSpawnOnlyTest()
    {
        var pos = PickTestPosition();
        Log($"[Test] SpawnAsync('Rock_C', {pos}, identity)...");
        SxProps.SpawnAsync("Rock_C", pos, Quaternion.identity, spawned =>
        {
            Log($"[Test] Spawn result: {(spawned != null ? spawned.name + " @ " + spawned.transform.position : "NULL")}");
        });
    }

    private static void RunFullPropsTestSuite()
    {
        Log("[Test] === SxProps full suite start ===");
        var pos = PickTestPosition();
        Log($"[Test] using position = {pos}");

        Log("[Test] 1. SpawnAsync('Rock_C', pos, identity)...");
        SxProps.SpawnAsync("Rock_C", pos, Quaternion.identity, spawned =>
        {
            if (spawned == null)
            {
                Log("[Test] 1. Spawn FAILED (returned null). Aborting suite.");
                return;
            }

            Log($"[Test] 1. Spawn OK: '{spawned.name}' at {spawned.transform.position}");

            Log("[Test] 2. CloneAt(spawn, pos + (2,0,2))...");
            var clone = SxProps.CloneAt(spawned, pos + new Vector3(2, 0, 2));
            Log($"[Test] 2. Clone {(clone != null ? "OK: " + clone.name : "FAILED")}");

            Log("[Test] 3. Hide(spawn)...");
            SxProps.Hide(spawned);
            Log($"[Test] 3. spawn.activeSelf after Hide = {spawned.activeSelf}");

            Log("[Test] 4. Show(spawn)...");
            SxProps.Show(spawned);
            Log($"[Test] 4. spawn.activeSelf after Show = {spawned.activeSelf}");

            Log("[Test] 5. Destroy(spawn) + Destroy(clone)...");
            SxProps.Destroy(spawned);
            if (clone != null)
            {
                SxProps.Destroy(clone);
            }

            Log("[Test] 5. Destroy() called (effect takes hold at end-of-frame)");
            Log("[Test] === SxProps full suite end ===");
        });
    }
}

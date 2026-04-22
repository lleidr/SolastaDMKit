using System.IO;
using System.Reflection;
using HarmonyLib;
using SolastaDMKit.Core.Diagnostics;
using SolastaDMKit.Core.Events;
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

    private static void WireDevSubscriptions()
    {
        EventBus.Subscribe<TileEntered>(_ =>
            Log("[Event] TileEntered"));

        EventBus.Subscribe<ObjectInteracted>(e =>
            Log($"[Event] ObjectInteracted condIdx={e.ConditionIndex} state={e.NewState}"));

        EventBus.Subscribe<DamageAbout>(e =>
            Log($"[Event] DamageAbout amount={e.RolledDamage} type={e.DamageType}"));

        EventBus.Subscribe<DamageApplied>(e =>
            Log($"[Event] DamageApplied amount={e.RolledDamage} type={e.DamageType}"));

        EventBus.Subscribe<TurnStarted>(_ =>
            Log("[Event] TurnStarted"));
    }
}

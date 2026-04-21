using System.IO;
using System.Reflection;
using HarmonyLib;
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

        _harmony = new Harmony(modEntry.Info.Id);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());

        Log("SolastaDMKit loaded.");
        return true;
    }
}

using System.Collections.Generic;

namespace SolastaDMKit.Core.Runtime;

public static class SxVariables
{
    private static readonly Dictionary<string, object> Store = new();

    private static string Key(string name)
    {
        var campaignId = Gui.GameCampaign?.campaignDefinition?.Name ?? "default";
        return $"{campaignId}.{name}";
    }

    public static bool GetBool(string name, bool defaultValue = false)
    {
        return Store.TryGetValue(Key(name), out var v) && v is bool b ? b : defaultValue;
    }

    public static void SetBool(string name, bool value)
    {
        Store[Key(name)] = value;
    }

    public static int GetInt(string name, int defaultValue = 0)
    {
        return Store.TryGetValue(Key(name), out var v) && v is int i ? i : defaultValue;
    }

    public static void SetInt(string name, int value)
    {
        Store[Key(name)] = value;
    }

    public static string GetString(string name, string defaultValue = "")
    {
        return Store.TryGetValue(Key(name), out var v) && v is string s ? s : defaultValue;
    }

    public static void SetString(string name, string value)
    {
        Store[Key(name)] = value ?? string.Empty;
    }

    public static bool Has(string name)
    {
        return Store.ContainsKey(Key(name));
    }

    public static void Clear(string name)
    {
        Store.Remove(Key(name));
    }

    public static void ClearAllForCurrentCampaign()
    {
        var prefix = Key(string.Empty);
        var keys = new List<string>();
        foreach (var k in Store.Keys)
        {
            if (k.StartsWith(prefix))
            {
                keys.Add(k);
            }
        }

        foreach (var k in keys)
        {
            Store.Remove(k);
        }
    }
}

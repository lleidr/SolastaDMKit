using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using SolastaDMKit.Core.Diagnostics;
using SolastaDMKit.Core.Events;
using SolastaDMKit.Core.Runtime;

namespace SolastaDMKit.Core.Scripting;

public sealed class ScriptRuntime
{
    private static readonly string UserContentRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "AppData",
        "LocalLow",
        "Tactical Adventures",
        "Solasta",
        "UserContent");

    private static readonly string ScriptsRoot = Path.Combine(UserContentRoot, "Scripts");

    private readonly List<object> _currentOwners = new();
    private string _loadedCampaign;
    private ScriptOptions _options;
    private Action<string, string> _errorReporter;

    public ScriptRuntime(Action<string, string> errorReporter = null)
    {
        _errorReporter = errorReporter ?? ((title, msg) => SxLog.Error($"[Scripts] {title}: {msg}"));
        BuildOptions();
    }

    public string LoadedCampaign => _loadedCampaign;

    public int LoadedScriptCount => _currentOwners.Count;

    public static string GetCampaignScriptDir(string campaignInternalName)
    {
        return Path.Combine(ScriptsRoot, campaignInternalName);
    }

    public void LoadCampaign(string campaignInternalName)
    {
        UnloadAll();

        if (string.IsNullOrEmpty(campaignInternalName))
        {
            return;
        }

        _loadedCampaign = campaignInternalName;

        var campaignDir = GetCampaignScriptDir(campaignInternalName);
        if (!Directory.Exists(campaignDir))
        {
            try
            {
                Directory.CreateDirectory(campaignDir);
                SxLog.Info($"[Scripts] Created campaign script folder: {campaignDir}");
            }
            catch (Exception ex)
            {
                SxLog.Error($"[Scripts] Could not create folder {campaignDir}", ex);
            }

            return;
        }

        var files = Directory.GetFiles(campaignDir, "*.csx", SearchOption.AllDirectories);
        SxLog.Info($"[Scripts] Loading {files.Length} .csx file(s) for campaign '{campaignInternalName}'.");

        foreach (var file in files)
        {
            RunScript(file, campaignInternalName);
        }
    }

    public void UnloadAll()
    {
        foreach (var owner in _currentOwners)
        {
            EventBus.UnsubscribeAll(owner);
        }

        var count = _currentOwners.Count;
        _currentOwners.Clear();

        if (count > 0)
        {
            SxLog.Info($"[Scripts] Unsubscribed {count} script owner(s).");
        }

        _loadedCampaign = null;
    }

    public void ReloadAll()
    {
        var toReload = _loadedCampaign;
        if (string.IsNullOrEmpty(toReload))
        {
            SxLog.Info("[Scripts] No campaign loaded — nothing to reload.");
            return;
        }

        SxLog.Info($"[Scripts] Reloading scripts for '{toReload}'...");
        LoadCampaign(toReload);
    }

    private void RunScript(string path, string campaignInternalName)
    {
        string code;
        try
        {
            code = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            Report(path, $"Failed to read script file: {ex.Message}");
            return;
        }

        var owner = new object();
        var globals = new ScriptGlobals
        {
            Owner = owner,
            CampaignName = campaignInternalName,
            ScriptPath = path,
        };

        try
        {
            CSharpScript.RunAsync(code, _options, globals).GetAwaiter().GetResult();
            _currentOwners.Add(owner);
            SxLog.Info($"[Scripts] Loaded: {Path.GetFileName(path)}");
        }
        catch (CompilationErrorException ex)
        {
            var diagText = string.Join("\n", ex.Diagnostics.Select(d => d.ToString()));
            Report(path, $"Compile error:\n{diagText}");
        }
        catch (Exception ex)
        {
            Report(path, $"Runtime error: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void Report(string path, string message)
    {
        var title = $"Script error in {Path.GetFileName(path)}";
        SxLog.Error($"[Scripts] {title}\n{message}");
        _errorReporter?.Invoke(title, message);
    }

    private void BuildOptions()
    {
        var refs = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a =>
            {
                try { return MetadataReference.CreateFromFile(a.Location); }
                catch { return null; }
            })
            .Where(r => r != null)
            .Cast<MetadataReference>()
            .ToArray();

        _options = ScriptOptions.Default
            .WithReferences(refs)
            .WithImports(
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "UnityEngine",
                "SolastaDMKit.Core.Events",
                "SolastaDMKit.Core.Runtime",
                "SolastaDMKit.Core.Diagnostics");
    }
}

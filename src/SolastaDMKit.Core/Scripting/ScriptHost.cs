using SolastaDMKit.Core.Diagnostics;
using SolastaDMKit.Core.Runtime;
using UnityEngine;

namespace SolastaDMKit.Core.Scripting;

public sealed class ScriptHost : MonoBehaviour
{
    private ScriptRuntime _runtime;
    private string _lastCampaignKey = string.Empty;
    private float _lastPollTime;

    public static ScriptHost Install()
    {
        var go = new GameObject("SxScriptHost");
        DontDestroyOnLoad(go);
        return go.AddComponent<ScriptHost>();
    }

    private void Awake()
    {
        _runtime = new ScriptRuntime(errorReporter: ShowError);
        SxLog.Info("[ScriptHost] installed.");
    }

    private void Update()
    {
        if (IsReloadHotkey())
        {
            SxLog.Info("[ScriptHost] Ctrl+Shift+R pressed — reloading scripts.");
            _runtime.ReloadAll();
        }

        if (Time.unscaledTime - _lastPollTime >= 1f)
        {
            _lastPollTime = Time.unscaledTime;
            PollCampaign();
        }
    }

    private static bool IsReloadHotkey()
    {
        var ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        var shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        return ctrl && shift && Input.GetKeyDown(KeyCode.R);
    }

    private void PollCampaign()
    {
        var key = SxCampaign.InternalName;
        if (key == _lastCampaignKey)
        {
            return;
        }

        _lastCampaignKey = key;

        SxLog.Info($"[ScriptHost] Campaign change detected. Resolution: {SxCampaign.ResolutionDiagnostic()}");

        if (string.IsNullOrEmpty(key))
        {
            _runtime.UnloadAll();
        }
        else
        {
            _runtime.LoadCampaign(key);
        }
    }

    private static void ShowError(string title, string message)
    {
        SxUI.ShowChoice(
            $"{title}\n\n{message}",
            new[] { "Dismiss" },
            _ => { });
    }
}

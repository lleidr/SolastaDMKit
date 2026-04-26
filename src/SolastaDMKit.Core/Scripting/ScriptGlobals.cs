using System;
using SolastaDMKit.Core.Events;

namespace SolastaDMKit.Core.Scripting;

public class ScriptGlobals
{
    internal object Owner { get; set; }

    public string CampaignName { get; internal set; } = string.Empty;

    public string ScriptPath { get; internal set; } = string.Empty;

    public IDisposable Subscribe<T>(Action<T> handler, int priority = 0) where T : ISxEvent
    {
        return EventBus.Subscribe(handler, priority, Owner);
    }
}

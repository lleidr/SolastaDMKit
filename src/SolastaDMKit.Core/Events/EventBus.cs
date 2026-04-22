using System;
using System.Collections.Generic;
using System.Linq;
using SolastaDMKit.Core.Diagnostics;

namespace SolastaDMKit.Core.Events;

public interface ISxEvent
{
}

public static class EventBus
{
    private sealed class Entry
    {
        public Delegate Handler;
        public int Priority;
        public object Owner;
        public bool Disposed;
    }

    private sealed class Subscription : IDisposable
    {
        private readonly Entry _entry;

        public Subscription(Entry entry)
        {
            _entry = entry;
        }

        public void Dispose()
        {
            if (_entry != null)
            {
                _entry.Disposed = true;
            }
        }
    }

    private static readonly Dictionary<Type, List<Entry>> Handlers = new();

    public static IDisposable Subscribe<T>(Action<T> handler, int priority = 0, object owner = null)
        where T : ISxEvent
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var entry = new Entry
        {
            Handler = handler,
            Priority = priority,
            Owner = owner,
            Disposed = false,
        };

        if (!Handlers.TryGetValue(typeof(T), out var list))
        {
            list = new List<Entry>();
            Handlers[typeof(T)] = list;
        }

        var insertIndex = list.FindIndex(e => e.Priority < priority);
        if (insertIndex < 0)
        {
            list.Add(entry);
        }
        else
        {
            list.Insert(insertIndex, entry);
        }

        return new Subscription(entry);
    }

    public static void UnsubscribeAll(object owner)
    {
        if (owner == null)
        {
            return;
        }

        foreach (var list in Handlers.Values)
        {
            foreach (var entry in list)
            {
                if (ReferenceEquals(entry.Owner, owner))
                {
                    entry.Disposed = true;
                }
            }
        }
    }

    public static void Publish<T>(T evt) where T : ISxEvent
    {
        if (!Handlers.TryGetValue(typeof(T), out var list) || list.Count == 0)
        {
            return;
        }

        var snapshot = list.ToArray();

        foreach (var entry in snapshot)
        {
            if (entry.Disposed)
            {
                continue;
            }

            try
            {
                ((Action<T>)entry.Handler)(evt);
            }
            catch (Exception ex)
            {
                SxLog.Error($"EventBus handler for {typeof(T).Name} threw", ex);
            }
        }

        list.RemoveAll(e => e.Disposed);
    }

    public static int HandlerCount<T>() where T : ISxEvent
    {
        return Handlers.TryGetValue(typeof(T), out var list)
            ? list.Count(e => !e.Disposed)
            : 0;
    }
}

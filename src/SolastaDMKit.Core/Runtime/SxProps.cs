using System;
using System.Collections.Generic;
using System.Linq;
using SolastaDMKit.Core.Diagnostics;
using UnityEngine;

namespace SolastaDMKit.Core.Runtime;

#pragma warning disable CA1711

public static class SxProps
{
    public static PropBlueprint FindBlueprint(string blueprintName)
    {
        if (string.IsNullOrEmpty(blueprintName))
        {
            return null;
        }

        var db = DatabaseRepository.GetDatabase<PropBlueprint>();
        if (db == null)
        {
            return null;
        }

        return db.TryGetElement(blueprintName, out var bp) ? bp : null;
    }

    public static IEnumerable<PropBlueprint> AllBlueprints()
    {
        var db = DatabaseRepository.GetDatabase<PropBlueprint>();
        return db ?? Enumerable.Empty<PropBlueprint>();
    }

    public static int BlueprintCount => AllBlueprints().Count();

    public static void Destroy(GameObject prop)
    {
        if (prop != null)
        {
            UnityEngine.Object.Destroy(prop);
        }
    }

    public static void Hide(GameObject prop)
    {
        if (prop != null)
        {
            prop.SetActive(false);
        }
    }

    public static void Show(GameObject prop)
    {
        if (prop != null)
        {
            prop.SetActive(true);
        }
    }

    public static GameObject CloneAt(GameObject existingProp, Vector3 worldPosition, Quaternion rotation = default)
    {
        if (existingProp == null)
        {
            return null;
        }

        if (rotation == default)
        {
            rotation = existingProp.transform.rotation;
        }

        var clone = UnityEngine.Object.Instantiate(existingProp, worldPosition, rotation, existingProp.transform.parent);
        clone.name = existingProp.name + "_Clone";
        return clone;
    }

    public static IEnumerable<GameObject> AllInCurrentLocation()
    {
        var service = ServiceRepository.GetService<IGameLocationService>();
        if (service?.WorldLocation == null)
        {
            yield break;
        }

        foreach (var sector in service.WorldLocation.WorldSectors)
        {
            if (sector == null)
            {
                continue;
            }

            foreach (var descendant in EnumerateDescendants(sector.transform))
            {
                yield return descendant.gameObject;
            }
        }
    }

    public static IEnumerable<GameObject> FindByName(string substring)
    {
        if (string.IsNullOrEmpty(substring))
        {
            return Enumerable.Empty<GameObject>();
        }

        return AllInCurrentLocation().Where(go => go != null && go.name.Contains(substring));
    }

    public static IEnumerable<GameObject> FindNear(Vector3 worldPosition, float radius)
    {
        if (radius <= 0f)
        {
            return Enumerable.Empty<GameObject>();
        }

        var radiusSq = radius * radius;
        return AllInCurrentLocation()
            .Where(go => go != null && (go.transform.position - worldPosition).sqrMagnitude <= radiusSq);
    }

    private static IEnumerable<Transform> EnumerateDescendants(Transform root)
    {
        if (root == null)
        {
            yield break;
        }

        foreach (Transform child in root)
        {
            yield return child;
            foreach (var grandchild in EnumerateDescendants(child))
            {
                yield return grandchild;
            }
        }
    }

    public static void SpawnAsync(
        string blueprintName,
        Vector3 worldPosition,
        Quaternion rotation,
        Action<GameObject> onLoaded)
    {
        var blueprint = FindBlueprint(blueprintName);
        if (blueprint == null)
        {
            SxLog.Error($"SxProps.SpawnAsync: blueprint '{blueprintName}' not found");
            onLoaded?.Invoke(null);
            return;
        }

        if (blueprint.PrefabsByEnvironment == null || blueprint.PrefabsByEnvironment.Count == 0)
        {
            SxLog.Error($"SxProps.SpawnAsync: blueprint '{blueprintName}' has no PrefabsByEnvironment entries");
            onLoaded?.Invoke(null);
            return;
        }

        var reference = blueprint.PrefabsByEnvironment[0].PrefabReference;
        if (reference == null)
        {
            SxLog.Error($"SxProps.SpawnAsync: blueprint '{blueprintName}' has null PrefabReference");
            onLoaded?.Invoke(null);
            return;
        }

        var handle = reference.InstantiateAsync(worldPosition, rotation);
        handle.Completed += op => onLoaded?.Invoke(op.Result);
    }
}

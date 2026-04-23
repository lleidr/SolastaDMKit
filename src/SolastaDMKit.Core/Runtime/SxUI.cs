using System;
using UnityEngine;

namespace SolastaDMKit.Core.Runtime;

public static class SxUI
{
    private static SxChoiceModalBehaviour _activeModal;

    public static void Log(string message)
    {
        var game = Gui.Game;
        if (game?.GameConsole == null)
        {
            return;
        }

        game.GameConsole.LogSimpleLine(message);
    }

    public static void ShowChoice(string message, string[] choices, Action<int> onSelected)
    {
        if (_activeModal != null)
        {
            onSelected?.Invoke(-1);
            return;
        }

        var go = new GameObject("SxChoiceModal");
        UnityEngine.Object.DontDestroyOnLoad(go);
        _activeModal = go.AddComponent<SxChoiceModalBehaviour>();
        _activeModal.Init(message, choices ?? Array.Empty<string>(), result =>
        {
            onSelected?.Invoke(result);
            if (_activeModal != null)
            {
                UnityEngine.Object.Destroy(_activeModal.gameObject);
                _activeModal = null;
            }
        });
    }

    public static bool IsModalOpen => _activeModal != null;
}

using System;
using UnityEngine;

namespace SolastaDMKit.Core.Runtime;

internal sealed class SxChoiceModalBehaviour : MonoBehaviour
{
    private const int WindowId = 1984042100;
    private const float WindowWidth = 560f;
    private const float ButtonHeight = 36f;
    private const float ButtonSpacing = 8f;
    private const float Padding = 16f;
    private const float TitleHeight = 28f;
    private const float MessageHeight = 110f;

    private string _message;
    private string[] _choices;
    private Action<int> _onSelected;
    private Rect _windowRect;
    private bool _positioned;

    public void Init(string message, string[] choices, Action<int> onSelected)
    {
        _message = message ?? string.Empty;
        _choices = choices ?? Array.Empty<string>();
        _onSelected = onSelected;

        var height = TitleHeight + MessageHeight + (ButtonHeight + ButtonSpacing) * _choices.Length + Padding * 2;
        _windowRect = new Rect(0f, 0f, WindowWidth, height);
    }

    private void OnGUI()
    {
        if (!_positioned)
        {
            _windowRect.x = (Screen.width - _windowRect.width) / 2f;
            _windowRect.y = (Screen.height - _windowRect.height) / 2f;
            _positioned = true;
        }

        _windowRect = GUI.ModalWindow(WindowId, _windowRect, DrawWindow, "SolastaDMKit");
    }

    private void DrawWindow(int id)
    {
        var contentWidth = _windowRect.width - Padding * 2;

        var messageStyle = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 14 };
        GUI.Label(
            new Rect(Padding, Padding + TitleHeight, contentWidth, MessageHeight),
            _message,
            messageStyle);

        var buttonY = Padding + TitleHeight + MessageHeight;
        for (var i = 0; i < _choices.Length; i++)
        {
            if (GUI.Button(new Rect(Padding, buttonY, contentWidth, ButtonHeight), _choices[i]))
            {
                var cb = _onSelected;
                _onSelected = null;
                cb?.Invoke(i);
                return;
            }

            buttonY += ButtonHeight + ButtonSpacing;
        }
    }
}

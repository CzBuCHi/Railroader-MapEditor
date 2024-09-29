using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace MapEditor.Features.Settings;

[PublicAPI]
public class KeyPressListener : MonoBehaviour
{
    private static KeyCode[] _KeyCodes = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().ToArray();

    private Action<KeyCode> _OnKeyDown = null!;

    public void Listen(Action<KeyCode> onKeyDown)
    {
        if (enabled)
        {
            throw new InvalidOperationException("Listener is already listening");
        }

        _OnKeyDown = onKeyDown;
        enabled = true;
    }

    public void Update()
    {
        if (!Input.anyKeyDown)
        {
            return;
        }

        var keyCode = _KeyCodes.First(Input.GetKeyDown);
        _OnKeyDown(keyCode);
        enabled = false;
    }
}

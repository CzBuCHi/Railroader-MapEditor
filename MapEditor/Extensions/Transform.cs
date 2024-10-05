using UnityEngine;

namespace MapEditor.Extensions;

public static class TransformExtensions
{
    public static Transform? GetChild(this Transform transform, string name) {
        for (var i = 0; i < transform.childCount; i++) {
            var child = transform.GetChild(i);
            if (child != null && child.name == name) {
                return child;
            }
        }

        return null;
    }
}

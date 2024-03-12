using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Tank.Scripts.Utility
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class VectorExtensions
    {
        public static Vector3 XOY(this Vector2 v) => new (v.x, 0, v.y);
        public static Vector3 X0Z(this Vector2 v) => new (v.x, 0, v.y);
        public static Vector2 XY(this Vector3 v) => new (v.x, v.y);
        public static Vector3 XZ(this Vector3 v) => new (v.x, 0, v.z);
    }
}
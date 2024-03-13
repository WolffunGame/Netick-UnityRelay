using UnityEngine;

public static class EncodeDir
{
    public static byte EncodeDirection(Vector2 dir)
    {
        if (dir == default)
            return default;
        var directionAngle = Vector2.SignedAngle(Vector2.right, dir);
        directionAngle = (directionAngle + 360) % 360 / 2 + 1;
        return (byte)directionAngle;
    }

    public static Vector2 DecodeDirection(byte value)
    {
        if (value == default)
            return Vector2.zero;
        var angle = (value - 1) * 2;
        var angleRad = Mathf.Deg2Rad * angle;
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
}
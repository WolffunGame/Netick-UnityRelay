using UnityEngine;

namespace Tank.Scripts.Utility
{
	public static class Draw
	{
		public static void DrawArrow(Vector3 start, Vector3 end, Color color, float duration = 0)
		{
			Debug.DrawLine(start, end, color, duration);
			var direction = end - start;
            var magnitude = direction.magnitude;
			var right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 30, 0) * Vector3.forward;
			var left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 30, 0) * Vector3.forward;
			Debug.DrawRay(end, right * 0.1f * magnitude, color, duration);
			Debug.DrawRay(end, left * 0.1f * magnitude, color, duration);
		}
	}
}
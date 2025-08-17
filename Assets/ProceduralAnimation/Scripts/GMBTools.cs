using System.Collections.Generic;
using UnityEngine;

public static class GMBTools
{
	public class KalmanFilterVector3
	{
		public const float DEFAULT_Q = 1E-06f;

		public const float DEFAULT_R = 0.01f;

		public const float DEFAULT_P = 1f;

		private float q;

		private float r;

		private float p;

		private Vector3 x;

		private float k;

		public KalmanFilterVector3()
		{
		}

		public KalmanFilterVector3(float aQ = 1E-06f, float aR = 0.01f)
		{
		}

		public Vector3 Update(Vector3 measurement, float? newQ = null, float? newR = null)
		{
			return default(Vector3);
		}

		public Vector3 Update(List<Vector3> measurements, bool areMeasurementsNewestFirst = false, float? newQ = null, float? newR = null)
		{
			return default(Vector3);
		}

		public void Reset()
		{
		}
	}

	public struct DampenedSpringMotionParams
	{
		public float PosPosCoef;

		public float PosVelCoef;

		public float VelPosCoef;

		public float VelVelCoef;
	}

	public static float ClampAngle(this float angle)
	{
		return 0f;
	}

	public static float ForceFloat(this float force)
	{
		return 0f;
	}

	public static Vector3 MaxVector(Vector3 vecOne, Vector3 vecTwo)
	{
		return default(Vector3);
	}

	public static void DrawDebugCircle(Vector3 position, Vector3 up, float radius, float duration = 0f, Color color = default(Color))
	{
	}

	public static Vector3 FirstOrderIntercept(Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed, Vector3 targetPosition, Vector3 targetVelocity)
	{
		return default(Vector3);
	}

	public static float FirstOrderInterceptTime(float shotSpeed, Vector3 targetRelativePosition, Vector3 targetRelativeVelocity)
	{
		return 0f;
	}

	public static void Calculate(ref Vector3 state, ref Vector3 velocity, Vector3 targetState, DampenedSpringMotionParams springMotionParams)
	{
	}

	public static void Calculate(ref Vector2 state, ref Vector2 velocity, Vector2 targetState, DampenedSpringMotionParams springMotionParams)
	{
	}

	public static void Calculate(ref float state, ref float velocity, float targetState, DampenedSpringMotionParams springMotionParams)
	{
	}

	public static DampenedSpringMotionParams CalcDampedSpringMotionParams(float dampingRatio, float angularFrequency)
	{
		return default(DampenedSpringMotionParams);
	}

	public static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness)
	{
		return null;
	}

	public static void SubdivideLines(List<Vector3> pointsIn, int subdivisions, List<Vector3> pointsOut)
	{
	}
}

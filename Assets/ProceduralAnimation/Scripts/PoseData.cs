using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PoseData", menuName = "GMB/Animation/PoseData")]
public class PoseData : ScriptableObject
{
	[Serializable]
	public class Pose
	{
		public string boneName;

		public Vector3 pos;

		public Quaternion rot;

		public Vector3 scale;
	}

	public Pose[] pose;

	public float transitionDuration;

	public AnimationCurve transitionCurve;
}

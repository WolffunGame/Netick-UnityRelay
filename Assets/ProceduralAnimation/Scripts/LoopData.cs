using UnityEngine;

[CreateAssetMenu(fileName = "PoseData", menuName = "GMB/Animation/LoopData")]
public class LoopData : ScriptableObject
{
	public float mediumOffset;

	public float bigOffset;

	public float changeWeightSmoothTime;

	public float constantSpeed;

	public float velocityThresholdSpeed;

	public float velocityMinThresholdSpeed;

	public float velocityMinSpeed;

	public float velocityMaxSpeed;

	public float yawDeltaMultiplier;

	public float accelerationMultiplier;

	public AnimationCurve curve;
}

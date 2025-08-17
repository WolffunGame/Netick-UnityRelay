using UnityEngine;

[CreateAssetMenu(fileName = "LegAnimData", menuName = "GMB/Animation/LegAnimData")]
public class LegAnimationData : ScriptableObject
{
	public AnimationCurve stepAnimCurve;

	public AnimationCurve stepSpeedCurve;

	public AnimationCurve feetRotPitchCurve;

	public float speedAtMaxLength;

	public float stepLength;

	public float inAirMaxLength;

	public float stepHeight;

	[Range(0f, 1f)]
	public float stepTime;

	public float stepYawDeltaOffset;

	public float stepRandomRadius;

	public float inAirDamp;

	public float feetRotSYaw;

	public float feetRotPitch;

	public float velocityDamp;

	public float legInertiaDamp;

	public float legInertiaThreshold;

	public float legInertiaMultiplier;

	public float legInertiaMinDamp;

	public float legInAirInertiaMultiplier;

	public float legInAirYInertiaMultiplier;

	public bool useBodyRotationWhenInAir;

	public float rayCastCheckOffset;

	public bool useRayCastCheck;
}

using UnityEngine;

public class AnimatedLimb : MonoBehaviour
{
	public Transform ikTarget;

	public Transform moveTarget;

	public Transform bendGoal;

	public ParticleSystem glideTrack;

	public FloatingBoneChain floatingBoneChain;

	[Range(-1f, 1f)]
	public float offset;

	[HideInInspector]
	public float timer;

	[HideInInspector]
	public int numLeg;

	[HideInInspector]
	public bool moving;

	[HideInInspector]
	public bool moveDown;

	[HideInInspector]
	public bool activatedNext;

	[HideInInspector]
	public Vector3 startPos;

	[HideInInspector]
	public Quaternion startRot;

	[HideInInspector]
	public Vector3 randomOffset;

	public Vector3 moveTargetOrigin;

	public Vector3 bendGoalOrigin;

	public void ResetBoneChain()
	{
	}

	public void SetOrigins(Transform moveTargetHolder, Transform bendGoalHolder, Transform ikTargetHolder)
	{
	}
}

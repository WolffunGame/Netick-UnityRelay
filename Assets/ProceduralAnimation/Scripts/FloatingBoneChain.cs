using UnityEngine;

public class FloatingBoneChain : MonoBehaviour
{
	public FloatingBoneChainData boneChainData;

	public Vector3 startAxis;

	public Vector3 endAxis;

	public Transform target;

	public Transform origin;

	public Transform pole;

	public Vector3[] rotationCorrection;

	public bool useRotationCorrection;

	private Vector3 state;

	private Vector3 velocity;

	private Vector3 stateOne;

	private Vector3 velocityOne;

	public Transform[] bones;

	private float currentWeight;

	private float weightTimer;

	private float weightDuration;

	private float targetWeight;

	private float startWeight;

	private bool changingWeight;

	private GMBTools.DampenedSpringMotionParams dampenedSpringMotionParams;

	public bool liveUpdate;

	public bool debug;

	private void OnDrawGizmos()
	{
	}

	private void Start()
	{
	}

	private void LateUpdate()
	{
	}

	public void SetWeight(float duration, float targetWeight)
	{
	}

	public void ResetSpring()
	{
	}
}

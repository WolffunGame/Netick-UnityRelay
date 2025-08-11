using UnityEngine;

[CreateAssetMenu(fileName = "GenericAnimData", menuName = "GMB/Animation/GenericAnimData")]
public class GenericAnimData : ScriptableObject
{
	public Vector3 scale;

	public Vector3 pos;

	public Vector3 rotation;

	public AnimationCurve curve;

	public float duration;

	public bool affectScale;

	public bool affectPos;

	public bool affectRot;

	public bool additive;

	public Vector3 targetScale;

	public Vector3 targetPos;

	public Vector3 targetRotation;
}

using UnityEngine;

[CreateAssetMenu(fileName = "FloatAnimData", menuName = "GMB/Animation/FloatAnimData")]
public class FloatAnimData : ScriptableObject
{
	public float start;

	public float end;

	public AnimationCurve curve;

	public float duration;

	public bool loop;
}

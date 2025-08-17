using UnityEngine;

[CreateAssetMenu(fileName = "LineLegData", menuName = "GMB/Tech/LineLegData")]
public class FloatingBoneChainData : ScriptableObject
{
	public AnimationCurve shape;

	public AnimationCurve rotationMask;

	public AnimationCurve widthCurve;

	public AnimationCurve springCurve;

	public float widthMultiplierMax;

	public float widthMultiplierMin;

	public int segments;

	public float twist;

	public float animatedTwistSpeed;

	public float shapeStrengthMin;

	public float shapeStrengthMax;

	public float velocityOffset;

	public float normalYworldUpNormalBlend;

	public float maxLength;

	public float minLength;

	public bool useClamp;

	public float worldPosHeightClamp;

	public float ratio;

	public float frequency;
}

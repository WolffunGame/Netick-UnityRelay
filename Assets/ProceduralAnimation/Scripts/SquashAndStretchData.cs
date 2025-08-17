using UnityEngine;

[CreateAssetMenu(fileName = "SquashAndStretchData", menuName = "GMB/Animation/SquashAndStretchData")]
public class SquashAndStretchData : ScriptableObject
{
	public float snsVelMultiplier;

	public float snsVelMax;

	public float snsAccMultiplier;

	public float snsAccMax;

	public int accSmoothIterations;

	public float damp;
}

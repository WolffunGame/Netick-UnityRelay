using UnityEngine;
public class MotorShake : MonoBehaviour
{
	[SerializeField] private Vector3 _shakeAmountByAxis = Vector3.zero;
	[SerializeField] private float _shakeSpeed = 10f;

	private float _offset;
	private Vector3 _originScale;

	private void Start()
	{
		_originScale = transform.localScale;
		_offset = Random.Range(-Mathf.PI, Mathf.PI);
	}

	private Vector3 CalculateShake()
	{
		var shake = new Vector3(Mathf.Sin(Time.time * _shakeSpeed + _offset), Mathf.Sin(Time.time * _shakeSpeed + _offset), Mathf.Sin(Time.time * _shakeSpeed + _offset));
		shake.x *= _shakeAmountByAxis.x;
		shake.y *= _shakeAmountByAxis.y;
		shake.z *= _shakeAmountByAxis.z;
		return shake;
	}

	private void LateUpdate() => transform.localScale = _originScale + CalculateShake();
}
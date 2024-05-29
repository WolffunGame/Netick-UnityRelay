using System.Collections;
using System.Globalization;
using Cysharp.Threading.Tasks;
using Examples.Tank;
using UnityEngine;
using TMPro;

namespace Examples.Tank
{
	public class CountdownManager : MonoBehaviour
	{
		[SerializeField] private float _countdownFrom;
		[SerializeField] private AnimationCurve _countdownCurve;
		[SerializeField] private TextMeshProUGUI _countdownUI;
		[SerializeField] AudioEmitter _audioEmitter;

		private float _countdownTimer;

		public delegate void Callback();

		private void Start() => Reset();

		public void Reset() => _countdownUI.transform.localScale = Vector3.zero;

		public async UniTask Countdown(Callback callback)
		{
			_countdownUI.transform.localScale = Vector3.zero;

			_countdownUI.text = _countdownFrom.ToString(CultureInfo.InvariantCulture);
			_countdownUI.gameObject.SetActive(true);

			var lastCount = Mathf.CeilToInt(_countdownFrom + 1);
			_countdownTimer = _countdownFrom;

			while (_countdownTimer > 0)
			{
				var currentCount = Mathf.CeilToInt(_countdownTimer);

				if (lastCount != currentCount)
				{
					lastCount = currentCount;
					_countdownUI.text = currentCount.ToString();
					_audioEmitter.PlayOneShot();
				}

				var x = _countdownTimer - Mathf.Floor(_countdownTimer);
				
				var t = _countdownCurve.Evaluate(x);
				if (t >= 0)
					_countdownUI.transform.localScale = Vector3.one * t;

				_countdownTimer -= Time.deltaTime * 1.5f;
				await UniTask.NextFrame();
			}

			_countdownUI.gameObject.SetActive(false);

			callback?.Invoke();
		}
	}
}
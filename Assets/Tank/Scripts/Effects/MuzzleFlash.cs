using UnityEngine;

namespace Examples.Tank
{
	public class MuzzleFlash : AutoReleasedFx
	{
		[SerializeField] private AudioEmitter _audioEmitter;
		[SerializeField] private ParticleSystem _particleEmitter;
		[SerializeField] private float _timeToFade;

		protected override float Duration => _timeToFade;

		protected override void OnEnable()
		{
			base.OnEnable();
			_audioEmitter.PlayOneShot();
			_particleEmitter.Play();
		}
	}
}
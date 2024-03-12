using System.Collections.Generic;
using UnityEngine;

namespace Examples.Tank
{
	[CreateAssetMenu(fileName = "AudioClip", menuName = "ScriptableObjects/AudioClip")]
	public class AudioClipData : ScriptableObject
	{
		[SerializeField] private List<AudioClip> _audioClips;
		[SerializeField] private float _pitchBase = 1f;
		[SerializeField] private float _pitchVariation = 0f;

		public AudioClip GetAudioClip() => _audioClips[Random.Range(0, _audioClips.Count)];

		public float GetPitchOffset()
		{
			var pitchVariationHalf = _pitchVariation / 2f;
			return _pitchBase + Random.Range(-pitchVariationHalf, pitchVariationHalf);
		}
	}
}
using UnityEngine;

namespace Examples.Tank
{
    public abstract class AutoReleasedFx : MonoBehaviour
    {
        private float _timeToDie;

        protected abstract float Duration { get; }

        protected virtual void OnEnable() => _timeToDie = Duration;

        private void Update()
        {
            if (!(_timeToDie > 0)) return;
            _timeToDie -= Time.deltaTime;
            if (_timeToDie <= 0 && Application.isPlaying)
                LocalObjectPool.Release(this);
        }
    }
}
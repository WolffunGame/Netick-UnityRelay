using UnityEngine;

namespace Netick.Samples
{
    public class FrameRateSetUp : MonoBehaviour
    {
        [SerializeField] private int _frameRate = 60;
        private void Awake()=>Application.targetFrameRate=_frameRate;
    }
}

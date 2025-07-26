using UnityEngine;

namespace UnitComponents
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerVelocity _playerVelocity;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private InputDelayHandle _input;

        public PlayerVelocity PlayerVelocity => _playerVelocity;
        public PlayerMovement PlayerMovement => _playerMovement;
        public InputDelayHandle Input => _input;
    }
}
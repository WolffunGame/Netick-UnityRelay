using Netick;
using UnityEngine;

namespace UnitComponents
{
    [ExecutionOrder(1000)]
    public class PlayerMovement : PlayerBaseComp
    {
        [SerializeField] private float _force = 1700;
        [SerializeField] private float _extraDrag = 200;
        private Rigidbody2D _rigidbody;
        private void Awake() => _rigidbody = GetComponentInParent<Rigidbody2D>();

        public override void NetworkFixedUpdate()
        {
            var direction = Player.Input.InputData.GetMoveDirection();
            if(!_rigidbody)
                return;
            if (direction != default)
                _rigidbody.AddForce(direction * (TimeHandler.timeScale  * 1f * _force * Player.PlayerVelocity.Mass * 0.01f), ForceMode2D.Force);
            _rigidbody.velocity -= _rigidbody.velocity * (TimeHandler.timeScale * 0.01f * 0.1f * _extraDrag);
        }
    }
}
using UnityEngine;

namespace UnitComponents
{
    public class PlayerMovement : PlayerBaseComp
    {
        [SerializeField] private float _force = 1700;
        [SerializeField] private float _extraDrag = 200;

        public override void NetworkFixedUpdate()
        {
            var direction = Player.Input.InputData.GetMoveDirection();
            if (direction != default)
                Player.PlayerVelocity.AddForce(direction * (TimeHandler.timeScale  * 1f * _force * Player.PlayerVelocity.Mass * 0.01f), ForceMode2D.Force);
            Player.PlayerVelocity.Velocity -= Player.PlayerVelocity.Velocity * (TimeHandler.timeScale * 0.01f * 0.1f * _extraDrag);
        }
    }
}
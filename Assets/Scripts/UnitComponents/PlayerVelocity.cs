using Netick;
using Netick.Unity;
using UnityEngine;

namespace UnitComponents
{
    public class PlayerVelocity : PlayerBaseComp
    {
        [SerializeField] private bool _isKinematic;
        [Networked] internal float Mass { get; set; }= 100f;
        [Networked] internal Vector2 Velocity { get; set; }

        public override void NetworkFixedUpdate()
        {
            if (_isKinematic)
            {
                Velocity *= 0f;
                return;
            }
            Velocity += Vector2.down * (Time.fixedDeltaTime * TimeHandler.timeScale * 20f);
            transform.position += Sandbox.FixedDeltaTime * TimeHandler.timeScale * (Vector3)Velocity;
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        }
        internal void AddForce(Vector2 force, ForceMode2D forceMode)
        {
            if (forceMode == ForceMode2D.Force)
                force *= 0.02f;
            Velocity += force / Mass;
        }

        internal void AddForce(Vector3 force, ForceMode2D forceMode) => AddForce((Vector2)force, forceMode);

        internal void AddForce(Vector2 force) => AddForce(force, ForceMode2D.Force);

        internal void AddForce(Vector3 force) => AddForce((Vector2)force, ForceMode2D.Force);
    }
}
using Netick;
using UnityEngine;

namespace UnitComponents
{
    [ExecutionOrder(10000)]
    public class PlayerVelocity : PlayerBaseComp
    {
        [SerializeField] private bool _isKinematic;
        [SerializeField] private Transform _tranMove;
        [Networked] public float Mass { get; internal set; }= 100f;
        [Networked] public Vector2 Velocity { get; internal set; }

        public override void NetworkFixedUpdate()
        {
            if (_isKinematic)
            {
                Velocity *= 0f;
                return;
            }
            //Velocity += Vector2.down * (Time.fixedDeltaTime * TimeHandler.timeScale * 20f);
            _tranMove.position += Sandbox.FixedDeltaTime * TimeHandler.timeScale * (Vector3)Velocity;
            _tranMove.position = new Vector3(transform.position.x, transform.position.y, 0f);
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
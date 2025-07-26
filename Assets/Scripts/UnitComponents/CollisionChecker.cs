using System;
using UnityEngine;

namespace UnitComponents
{
    public class CollisionChecker : PlayerBaseComp
    {
        public Action<Collision2D> CollisionAction;
        [SerializeField] private CircleCollider2D _collider;
        [SerializeField] private LayerMask _collisionMask;
        private Collider2D[] _collisions = new Collider2D[10];

        public override void NetworkFixedUpdate()
        {
            var count = Sandbox.Physics2D.OverlapCircle(
                transform.position,
                _collider.radius,
                _collisions,
                _collisionMask);
            for (int i = 0; i < count; i++)
                Collide(_collisions[i]);
        }

        private void Collide(Collider2D collision)
        {
            if (collision == _collider)
                return;
        }
    }
}
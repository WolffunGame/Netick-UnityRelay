using System;
using Netick.Unity;
using UnityEngine;

namespace Tank.Scripts.Projectile
{
    public class Projectile : NetworkBehaviour
    {
        [SerializeField] private LayerMask _hitMask;
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _lifeTime = 2f;
        [SerializeField] private float _damage = 1f;
        [SerializeField] private GameObject _impactEffect;

        public override void NetworkFixedUpdate()
        {
            var tran = transform;
            var position = tran.position;
            var oldPos = position;
            position += tran.forward * (_speed * Sandbox.FixedDeltaTime);
            tran.position = position;
            if (!IsServer) 
                return;
            
            _lifeTime -= Sandbox.FixedDeltaTime;
            if (_lifeTime < 0)
            {
                if(Object.HasValidId)
                    Sandbox.Destroy(Object);
                return;
            }
            RayCast(oldPos);
        }

        private void RayCast(Vector3 oldPos)
        {
            if (Sandbox.Physics.Raycast(oldPos, transform.position - oldPos, out var hit,
                    _speed * Sandbox.FixedDeltaTime, _hitMask) && Object.HasValidId)
                Sandbox.Destroy(Object);
        }

        public override void NetworkDestroy()
        {
            if (IsServer)
                return;
            var hitFx = Instantiate(_impactEffect);
            hitFx.transform.position = transform.position;
        }
    }
}
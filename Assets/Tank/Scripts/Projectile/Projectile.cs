using System.Diagnostics.CodeAnalysis;
using Examples.Tank;
using Netick;
using Netick.Unity;
using UnityEngine;

namespace Tank.Scripts.Projectile
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public class Projectile : NetworkBehaviour
    {
        [SerializeField] private LayerMask _hitMask;
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _lifeTime = 2f;
        //[SerializeField] private float _damage = 1f;
        [SerializeField] private ExplosionFX _impactEffect;
        [Networked] private bool IsDestroyed { get; set; }
        [Networked] private float LifeTime { get; set; }

        public override void NetworkStart()
        {
            LifeTime = _lifeTime;
            IsDestroyed = false;
        }

        public override void NetworkFixedUpdate()
        {
            if (IsDestroyed || !Object.HasValidId)
                return;
            var tran = transform;
            var position = tran.position;
            var oldPos = position;
            position += tran.forward * (_speed * Sandbox.FixedDeltaTime);
            tran.position = position;
            LifeTime -= Sandbox.FixedDeltaTime;
            if (LifeTime < 0)
            {
                if (IsServer && Object.HasValidId)
                    Sandbox.Destroy(Object);
                IsDestroyed = true;
                return;
            }

            RayCast(oldPos);
        }

        private void RayCast(Vector3 oldPos)
        {
            if (!Sandbox.Physics.Raycast(oldPos, transform.position - oldPos, out _,
                    _speed * Sandbox.FixedDeltaTime, _hitMask) || !Object.HasValidId) return;
            if (IsServer && Object.HasValidId)
                Sandbox.Destroy(Object);
            IsDestroyed = true;
        }

        [OnChanged(nameof(IsDestroyed))]
        private void OnProjectileDestroy(OnChangedData onChangedData)
        {
            if (IsServer)
                return;
            LocalObjectPool.Acquire(_impactEffect, transform.position, Quaternion.identity);
        }
    }
}
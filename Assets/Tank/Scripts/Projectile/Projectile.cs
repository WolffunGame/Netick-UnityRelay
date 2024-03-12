using System;
using System.Diagnostics.CodeAnalysis;
using Examples.Tank;
using FusionHelpers;
using Netick;
using Netick.Unity;
using UnityEngine;

namespace Tank.Scripts.Projectile
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    public class Projectile : NetworkBehaviour
    {
        [SerializeField] private LayerMask _hitMask;
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _lifeTime = 2f;
        
        [SerializeField] private Shot _bulletPrefab;
        //[SerializeField] private float _damage = 1f;
        [SerializeField] private ExplosionFX _impactEffect;
        [Networked] private bool IsDestroyed { get; set; }
        [Networked] private float LifeTime { get; set; }
        
        [Networked(size:12)] private NetworkArray<ShotState> _bulletStates = new(12);
        
        private NetworkArray<ShotState> _pevBulletState = new(12);
        
        private SparseCollection<ShotState, Shot> _bullets;

        public void Start() => _bullets = new SparseCollection<ShotState, Shot>(_bulletStates, _bulletPrefab);
        
        public override void NetworkStart()
        {
            LifeTime = _lifeTime;
            IsDestroyed = false;
        }

        public override void NetworkUpdate()
        {
            for (var i = 0; i < _bulletStates.Length; i++)
                _pevBulletState[i] = _bulletStates[i];
        }

        public override void NetworkFixedUpdate()
        {
            if (IsDestroyed || !Object.HasValidId)
                return;
            
            _bullets?.Process(this, (ref ShotState bullet, int _) =>
            {
                if (bullet.Position.y < -.15f)
                {
                    bullet.EndTick = Sandbox.Tick.TickValue;
                    return true;
                }
                
                if (_bulletPrefab.IsHitScan || bullet.EndTick <= Sandbox.Tick.TickValue) return false;
                var dir = bullet.Direction.normalized;
                var length = Mathf.Max(_bulletPrefab.Radius, _bulletPrefab.Speed * Sandbox.FixedDeltaTime);
                if (!Sandbox.Physics.Raycast(bullet.Position - length * dir, dir, out var hitInfo, length,
                        _bulletPrefab.HitMask.value, QueryTriggerInteraction.Ignore)) return false;
                bullet.Position = hitInfo.point;
                bullet.EndTick = Sandbox.Tick.TickValue;
                
                return true;
            });
            
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

        public override void NetworkRender() => _bullets.Render(this, _pevBulletState);

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
        
        public override void NetworkDestroy() => _bullets.Clear();
    }
}
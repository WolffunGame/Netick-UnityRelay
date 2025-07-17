using System.Collections;
using System.Collections.Generic;
using Examples.Tank;
using UnityEngine;

using UnityEngine;
using Netick;
using Netick.Unity;

public abstract class BaseProjectile : NetworkBehaviour, IProjectile
{
    [Header("Base Projectile Settings")]
    [SerializeField] protected LayerMask _hitMask;
    [SerializeField] protected float _speed = 10f;
    [SerializeField] protected float _damage = 25f;
    [SerializeField] protected float _lifeTime = 2f;
    [SerializeField] protected ExplosionFX _impactEffect;

    [Networked] public bool IsDestroyed { get; protected set; }
    [Networked] protected float RemainingLifeTime { get; set; }
    [Networked] protected Vector3 Direction { get; set; }


    protected ShotDataProvider ShotDataProvider;
    
    
    public virtual float Speed => _speed;
    public virtual float Damage => _damage;
    public virtual float LifeTime => _lifeTime;
    public virtual LayerMask HitMask => _hitMask;

    protected Vector3 _initialPosition;

    public override void NetworkStart()
    {
        RemainingLifeTime = _lifeTime;
        IsDestroyed = false;
        _initialPosition = transform.position;
    }

    public virtual void Initialize(Vector3 position, Vector3 direction, float damage = 0)
    {
        transform.position = position;
        Direction = direction.normalized;
        transform.forward = Direction;
        
        if (damage > 0)
            _damage = damage;
    }

    public ShotState CreateShotState(Vector3 position, Vector3 direction, int startTick)
    {
        return new ShotState
        {
            Position = position,
            Direction = direction,
            Speed = _speed,
            Damage = _damage,
            StartTick = startTick
        };
    }

    public override void NetworkFixedUpdate()
    {
        if (IsDestroyed || !Object.HasValidId)
            return;

        ProcessMovement();
        CheckLifeTime();
    }

    protected virtual void ProcessMovement()
    {
        var oldPosition = transform.position;
        var newPosition = oldPosition + Direction * (Speed * Sandbox.FixedDeltaTime);
        
        transform.position = newPosition;
        
        CheckCollision(oldPosition, newPosition);
    }

    protected virtual void CheckCollision(Vector3 oldPosition, Vector3 newPosition)
    {
        var direction = newPosition - oldPosition;
        var distance = direction.magnitude;
        
        if (Sandbox.Physics.Raycast(oldPosition, direction.normalized, out var hitInfo, distance, HitMask))
        {
            OnHit(hitInfo.point, hitInfo.collider);
        }
    }

    protected virtual void CheckLifeTime()
    {
        RemainingLifeTime -= Sandbox.FixedDeltaTime;
        if (RemainingLifeTime <= 0)
        {
            OnLifeTimeExpired();
        }
    }

    public virtual void OnHit(Vector3 hitPoint, Collider hitCollider)
    {
        if (IsDestroyed) return;
        
        IsDestroyed = true;
        transform.position = hitPoint;
        
        // Spawn impact effect
        if (_impactEffect != null)
        {
            LocalObjectPool.Acquire(_impactEffect, hitPoint, Quaternion.identity);
        }
        
        // Handle damage
        ProcessDamage(hitCollider);
        
        // Destroy projectile
        if (Sandbox.IsServer && Object.HasValidId)
        {
            Sandbox.Destroy(Object);
        }
    }

    public virtual void OnLifeTimeExpired()
    {
        if (IsDestroyed) return;
        
        IsDestroyed = true;
        
        if (Sandbox.IsServer && Object.HasValidId)
        {
            Sandbox.Destroy(Object);
        }
    }

    protected virtual void ProcessDamage(Collider hitCollider)
    {
        // Override in derived classes for specific damage logic
        var damageable = hitCollider.GetComponent<IDamageable>();
        damageable?.TakeDamage(Damage);
    }
}
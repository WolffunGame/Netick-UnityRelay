using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectile
{
    float Speed { get; }
    float Damage { get; }
    float LifeTime { get; }
    LayerMask HitMask { get; }
    bool IsDestroyed { get; }
    
    void Initialize(Vector3 position, Vector3 direction, float damage = 0);
    ShotState CreateShotState(Vector3 position, Vector3 direction, int startTick);
    void OnHit(Vector3 hitPoint, Collider hitCollider);
    void OnLifeTimeExpired();
}

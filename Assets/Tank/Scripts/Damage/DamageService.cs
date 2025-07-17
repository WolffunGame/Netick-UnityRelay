using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DamageService : IDamageService
{
    public DamageService()
    {
        Debug.Log("DamageService initialized");
    }

    public void DealDamage(GameObject target, float damage, Vector3 hitPoint, GameObject source = null)
    {
        if (target == null) return;

        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, source);
            Debug.Log($"Dealt {damage} damage to {target.name} at {hitPoint}");
        }
        
        // Spawn damage effect
        SpawnDamageEffect(hitPoint, damage);
    }

    public void DealAreaDamage(Vector3 center, float radius, float damage, LayerMask targetMask, GameObject source = null)
    {
        var colliders = Physics.OverlapSphere(center, radius, targetMask);
        
        foreach (var collider in colliders)
        {
            var distance = Vector3.Distance(center, collider.transform.position);
            var damageFactor = 1f - (distance / radius);
            var actualDamage = damage * damageFactor;
            
            DealDamage(collider.gameObject, actualDamage, collider.transform.position, source);
        }
    }

    public float CalculateDamage(float baseDamage, float distance, float maxDistance = 100f)
    {
        var damageFalloff = Mathf.Clamp01(1f - (distance / maxDistance));
        return baseDamage * damageFalloff;
    }

    private void SpawnDamageEffect(Vector3 position, float damage)
    {
        // TODO: Spawn damage number or effect at position
        Debug.Log($"Damage effect at {position}: {damage}");
    }
}
using UnityEngine;

public interface IDamageService
{
    void DealDamage(GameObject target, float damage, Vector3 hitPoint, GameObject source = null);
    void DealAreaDamage(Vector3 center, float radius, float damage, LayerMask targetMask, GameObject source = null);
    float CalculateDamage(float baseDamage, float distance, float maxDistance = 100f);
}

using System.Collections;
using System.Collections.Generic;
using Netick.Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileConfig", menuName = "Weapons/Projectile Config")]
public class ShotConfig : ScriptableObject
{
    [SerializeField] private List<ShotData> _projectileDatas;
    public List<ShotData> ProjectileDatas => _projectileDatas;

    public ShotData GetProjectileData(PowerupType type, ShotMovementType movementType)
    {
        var projectileData = _projectileDatas?.Find(p => p.Type == type && p.MovementType == movementType);
        return projectileData;
    }
}


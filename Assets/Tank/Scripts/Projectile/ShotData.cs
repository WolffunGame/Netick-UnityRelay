using Examples.Tank;
using UnityEngine;

[System.Serializable]
public class ShotData
{
    [Header("Basic Properties")]
    public PowerupType Type;
    public ShotMovementType MovementType;
    
    [Header("Physics")]
    public float Speed = 20f;
    public float Damage = 25f;
    public float LifeTime = 2f;
    
    [Header("Damage Properties")]
    public float Radius;
    public float Range;
    public float AreaRadius;
    public float AreaImpulse;
    public byte AreaDamage;
    
    [Header("Movement Specific")]
    [SerializeField] private ShotMovementData _movementData;
    
    [Header("Effects")]
    public ExplosionFX DetonationPrefab;
    public MuzzleFlash MuzzleFlash;
    
    public ShotMovementData MovementData => _movementData ?? new ShotMovementData();
}

[System.Serializable]
public class ShotMovementData
{
    [Header("Parabolic Settings")]
    public Vector3 Gravity = new Vector3(0, -9.81f, 0);
    
    [Header("ZigZag Settings")]
    public float ZigZagAmplitude = 2f;
    public float ZigZagFrequency = 3f;
}

public enum ShotMovementType
{
    Linear,
    Parabolic,
    ZigZag
}
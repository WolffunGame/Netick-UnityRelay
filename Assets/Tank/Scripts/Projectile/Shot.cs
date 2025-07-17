using System;
using Examples.Tank;
using Helpers;
using Netick;
using Netick.Unity;
using UnityEngine;

[Serializable]
public struct ShotState : ISparseState<Shot>
{
    private int _startTick;

    public int StartTick
    {
        get => _startTick;
        set
        {
            _startLocalTime = Time.time;
            _startTick = value;
        }
    }

    public int EndTick { get; set; }
    public Vector3 Position;
    public Vector3 Direction;
    public float Speed;
    public float Damage;
    public PowerupType ProjectileType;
    public ShotMovementType MovementType; // Keep this for compatibility with existing Shot class

    [NonSerialized] private float _startLocalTime;
    public float StartLocalTime => _startLocalTime;
    public NetworkArrayStruct32<byte> TBytes { get; set; }

    public void Extrapolate(float t, Shot prefab)
    {
        UnityEngine.Debug.Log("Extrapolating shot state at time: " + t + "MovementType: " + MovementType);
        // Movement based on projectile type
        switch (MovementType)
        {
            case ShotMovementType.Linear:
                Position = GetLinearPositionAt(t);
                break;
            case ShotMovementType.Parabolic:
                Position = GetParabolicPositionAt(t, prefab);
                break;
            case ShotMovementType.ZigZag:
                Position = GetZigZagPositionAt(t, prefab);
                break;
        }

        Direction = GetDirectionAt(t, prefab);
    }

    public Vector3 GetLinearPositionAt(float t) => Position + t * Speed * Direction;

    public Vector3 GetParabolicPositionAt(float t, Shot prefab)
    {
        Vector3 initialVelocity = Speed * (Direction + Vector3.up * 0.8f).normalized;
        Vector3 horizontalMovement = new Vector3(
            initialVelocity.x * t,
            0,
            initialVelocity.z * t
        );

        float verticalPosition = initialVelocity.y * t + 0.5f * prefab.Gravity.y * 1.2f * (t * t);
        Vector3 verticalMovement = Vector3.up * verticalPosition;

        // Freeze direction at t = 0.6
        float directionTime = Mathf.Min(t, 0.6f);
        Vector3 instantVelocity = initialVelocity + prefab.Gravity * directionTime;
        Direction = instantVelocity.normalized;

        return Position + horizontalMovement + verticalMovement;
    }

    public Vector3 GetZigZagPositionAt(float t, Shot prefab)
    {
        var basePos = GetLinearPositionAt(t);
        var amplitude = prefab.ZigZagAmplitude;
        var frequency = prefab.ZigZagFrequency;

        // Simple horizontal zigzag
        var rightDirection = Vector3.Cross(Direction, Vector3.up).normalized;
        var offset = rightDirection * (Mathf.Sin(t * frequency) * amplitude);

        return basePos + offset;
    }

    private Vector3 GetDirectionAt(float t, Shot prefab) =>
        prefab.Speed == 0 ? Direction : (Speed * Direction + t * prefab.Gravity).normalized;

    public Vector3 GetTargetPosition(Shot prefab)
    {
        var a = 0.5f * prefab.Gravity.y;
        var b = Speed * Direction.y;
        var c = Position.y;
        var d = b * b - 4 * a * c;
        var t = (-b - Mathf.Sqrt(d)) / (2 * a);
        var p = GetParabolicPositionAt(t, prefab);
        p.y = 0.05f;
        return p;
    }
}

public class Shot : MonoBehaviour, ISparseVisual<ShotState, Shot>, IProjectile
{
    [Header("Shot Settings")] [SerializeField]
    private LayerMask _hitMask;

    [SerializeField] private bool _serverVisible;
    [SerializeField] private bool _isHitScan;
    private Vector3 _gravity = new Vector3(0, -9.81f, 0);
    private float _speed = 20f;
    private float _damage = 25f;
    private float _radius;
    private float _range;
    private float _areaRadius;
    private float _areaImpulse;
    private byte _areaDamage;
    private float _timeToLive = 2f;
    private PowerupType _projectileType = PowerupType.DEFAULT;
    private ShotMovementType _movementType = ShotMovementType.Linear;

    [Header("ZigZag Settings")] private float _zigZagAmplitude = 2f;
    private float _zigZagFrequency = 3f;

    [Header("Effects")] [SerializeField] private ExplosionFX _detonationPrefab;
    [SerializeField] private MuzzleFlash _muzzleFlash;

    // IProjectile implementation
    public float Speed => _speed;
    public float Damage => _damage;
    public float LifeTime => _timeToLive;
    public LayerMask HitMask => _hitMask;
    public bool IsDestroyed { get; private set; }
    public PowerupType ProjectileType => _projectileType;
    public ShotMovementType MovementType => _movementType;

    // Shot specific properties
    public Vector3 Gravity => _gravity;
    public float Radius => _radius;
    public float Range => _range;
    public float AreaRadius => _areaRadius;
    public float AreaImpulse => _areaImpulse;
    public byte AreaDamage => _areaDamage;
    public float TimeToLive => _timeToLive;
    public bool IsHitScan => _isHitScan;
    public bool IsServerVisible => _serverVisible;
    public float ZigZagAmplitude => _zigZagAmplitude;
    public float ZigZagFrequency => _zigZagFrequency;

    private bool _isFirstRender;
    private Transform _xForm;

    private void Awake() => _xForm = transform;
    private void OnEnable() => _isFirstRender = true;
    private void OnDisable() => _isFirstRender = false;

    public void Initialize(Vector3 position, Vector3 direction, float damage = 0)
    {
        transform.position = position;
        transform.forward = direction;
        if (damage > 0)
            _damage = damage;
    }

    public GameObject GetPrefab() => gameObject;

    public ShotState CreateShotState(Vector3 position, Vector3 direction, int startTick)
    {
        Debug.Log("_timeToLive: " + _timeToLive);
        return new ShotState
        {
            Position = position,
            Direction = direction.normalized,
            StartTick = startTick,
            EndTick = startTick + Mathf.RoundToInt(_timeToLive / Time.fixedDeltaTime),
            Speed = _speed,
            Damage = _damage,
            ProjectileType = _projectileType,
            MovementType = _movementType
        };
    }

    public void OnHit(Vector3 hitPoint, Collider hitCollider)
    {
        IsDestroyed = true;

        // Spawn explosion effect
        if (_detonationPrefab != null)
        {
            LocalObjectPool.Acquire(_detonationPrefab, hitPoint, Quaternion.identity);
        }

        // Process area damage if applicable
        if (_areaRadius > 0)
        {
            ProcessAreaDamage(hitPoint);
        }
        else
        {
            // Single target damage
            var damageable = hitCollider.GetComponent<IDamageable>();
            damageable?.TakeDamage(Damage);
        }
    }

    public void OnLifeTimeExpired()
    {
        IsDestroyed = true;
    }

    private void ProcessAreaDamage(Vector3 center)
    {
        var colliders = Physics.OverlapSphere(center, _areaRadius, _hitMask);
        foreach (var collider in colliders)
        {
            var damageable = collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                var distance = Vector3.Distance(center, collider.transform.position);
                var damageFactor = 1f - (distance / _areaRadius);
                var actualDamage = Damage * damageFactor;

                damageable.TakeDamage(actualDamage);

                // Apply impulse if applicable
                var rigidbody = collider.GetComponent<Rigidbody>();
                if (rigidbody != null && _areaImpulse > 0)
                {
                    var direction = (collider.transform.position - center).normalized;
                    rigidbody.AddForce(direction * _areaImpulse * damageFactor, ForceMode.Impulse);
                }
            }
        }
    }

    public void ApplyStateToVisual(NetworkBehaviour owner, ShotState state, float t, bool isFirstRender, bool isLast)
    {
        if (_isFirstRender && _muzzleFlash && owner.TryGetComponent<BaseWeapon>(out var weapon))
        {
            var muzzle = LocalObjectPool.Acquire(_muzzleFlash);
            muzzle.transform.SetParent(weapon.FirePoint);
            muzzle.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        if (isLast)
        {
            var position = IsHitScan
                ? state.Position + state.Direction
                : state.Position;
            LocalObjectPool.Acquire(_detonationPrefab, position, Quaternion.identity);
        }

        _isFirstRender = false;
        _xForm.forward = state.Direction;
        _xForm.position = state.Position;
    }

    public void Setup(ShotData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        _timeToLive = data.LifeTime;
        _speed = data.Speed;
        _damage = data.Damage;
        _gravity = data.MovementData.Gravity;
        _projectileType = data.Type;
        _movementType = data.MovementType;
        _zigZagAmplitude = data.MovementData.ZigZagAmplitude;
        _zigZagFrequency = data.MovementData.ZigZagFrequency;
        _radius = data.Radius;
        _range = data.Range;
        _areaRadius = data.AreaRadius;
        _areaImpulse = data.AreaImpulse;
        _areaDamage = data.AreaDamage;
        _detonationPrefab = data.DetonationPrefab;
        _muzzleFlash = data.MuzzleFlash;
    }
}
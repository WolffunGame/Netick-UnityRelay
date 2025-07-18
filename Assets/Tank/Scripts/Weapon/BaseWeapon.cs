using Netick;
using Reflex.Attributes;
using Reflex.Core;
using Reflex.Extensions;
using UnityEngine;

public abstract class BaseWeapon : TankComponent, IWeapon
{
    [Header("Base Weapon Settings")] 
    [SerializeField] protected Transform _firePoint;
    [SerializeField] protected float _fireInterval = 0.4f;
    [SerializeField] protected Shot _shotPrefab;
    [SerializeField] protected byte _maxAmmo = 5;
    
    [Header("Shot Configuration")]
    [SerializeField] protected ShotMovementType _shotMovementType = ShotMovementType.Linear;

    protected Vector3 _offset;
    protected IWeaponManager _weaponManager;

    // Reflex injection attributes
    protected IShotFactory _shotFactory;
    protected IDamageService _damageService;

    [Networked] protected float FireTime { get; set; }
    [Networked] protected byte Ammo { get; set; }

    public abstract PowerupType PowerupType { get; protected set; }
    public abstract WeaponInstallationType InstallationType { get; protected set; }
    public Transform FirePoint => _firePoint;

    protected virtual void Awake()
    {
        _offset = _firePoint.position - transform.position;
    }


    public override void NetworkStart()
    {
        base.NetworkStart();
        SetupShotData();
    }

    public override void NetworkDestroy()
    {
        base.NetworkDestroy();
    }

    public override void NetworkFixedUpdate()
    {
        if (FireTime > 0)
            FireTime -= Sandbox.FixedDeltaTime;

        ProcessWeaponLogic();
    }
    private void SetupShotData()
    {
        if (_shotPrefab == null) return;
        if(_shotFactory == null)
        {
            Debug.LogWarning("ShotFactory is not set, using default shot prefab settings.");
            return;
        }
        _shotPrefab.Setup(_shotFactory.GetShotData(PowerupType, _shotMovementType));
    }
    protected virtual void ProcessWeaponLogic()
    {
        SwitchToDefault();
    }
    protected abstract void SwitchToDefault();
    public void InjectDependencies(IDamageService damageService, IShotFactory shotFactory)
    {
        _damageService = damageService;
        _shotFactory = shotFactory;
    }

    public virtual void Initialize(IWeaponManager weaponManager, WeaponData weaponData = null)
    {
        _weaponManager = weaponManager;

        if (weaponData == null)
            return;

        _fireInterval = weaponData.FireRate;
        _maxAmmo = (byte)weaponData.MaxAmmo;
    }

    public abstract void Fire(Vector3 aimDirection);

    public virtual bool CanFire()
    {
        return FireTime <= 0 && Ammo > 0;
    }

    public virtual void Reload()
    {
        Ammo = _maxAmmo;
        FireTime = 0;
    }

    public virtual void Activate()
    {
        Reload();
        gameObject.SetActive(true);
    }

    public virtual void Deactivate()
    {
        Debug.Log("Deactivating weapon: " + PowerupType);
        gameObject.SetActive(false);
    }

    protected Vector3 GetFirePosition(Vector3 aimDirection)
    {
        return transform.position + Quaternion.LookRotation(aimDirection) * _offset;
    }

    /// <summary>
    /// Creates shot state using ShotFactory with config data
    /// </summary>
    protected ShotState CreateShotState(Vector3 position, Vector3 direction, ShotMovementType movementType = ShotMovementType.Linear)
    {
        // Try to use ShotFactory first (with config data)
        if (_shotFactory != null)
        {
            return _shotFactory.CreateShotState(PowerupType, _shotMovementType, position, direction, Sandbox.Tick.TickValue);
        }

        // Fallback to shot prefab
        if (_shotPrefab != null)
        {
            return _shotPrefab.CreateShotState(position, direction, Sandbox.Tick.TickValue);
        }

        // Ultimate fallback - default values
        return CreateDefaultShotState(position, direction);
    }

    private ShotState CreateDefaultShotState(Vector3 position, Vector3 direction)
    {
        return new ShotState
        {
            Position = position,
            Direction = direction.normalized,
            StartTick = Sandbox.Tick.TickValue,
            EndTick = Sandbox.Tick.TickValue + 100,
            Speed = 20f,
            Damage = 25f,
            ProjectileType = PowerupType,
            MovementType = _shotMovementType
        };
    }

}
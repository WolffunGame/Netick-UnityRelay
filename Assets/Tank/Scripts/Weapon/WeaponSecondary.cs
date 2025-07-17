using Examples.Tank;
using Helpers;
using Netick;
using Netick.Unity;
using Tank.Scripts.Utility;
using UnityEngine;

public class WeaponSecondary : BaseWeapon
{
    private const byte MaxAmmo = 32; // Smaller for secondary weapons

    [SerializeField] private PowerupType _powerupType = PowerupType.DEFAULT;

    [Networked] private byte ProjectileID { get; set; }

    [Networked(size: MaxAmmo)] [Smooth(false)]
    private readonly NetworkArray<ShotState> _projectileStates = new(MaxAmmo);

    private SparseCollection<ShotState, Shot> _projectiles;

    public override PowerupType PowerupType
    {
        get => _powerupType;
        protected set => _powerupType = value;
    }

    public override WeaponInstallationType InstallationType
    {
        get => WeaponInstallationType.SECONDARY;
        protected set => throw new System.NotImplementedException("Primary weapon installation type cannot be set.");
    }

    public void Start()
    {
        _projectiles = new SparseCollection<ShotState, Shot>(_projectileStates, _shotPrefab);
    }

    protected override void ProcessWeaponLogic()
    {
        base.ProcessWeaponLogic();
        ProcessProjectiles();

        var input = Tank.InputDelayHandle.InputData;
        if (input.IsDown(InputData.BUTTON_FIRE_SECONDARY) && CanFire())
        {
            Fire(input.GetAimDirection().XOY());
        }
    }

    private void ProcessProjectiles()
    {
        _projectiles?.Process(this, (ref ShotState projectile, int _) =>
        {
            if (_shotPrefab.IsHitScan || projectile.EndTick <= Sandbox.Tick.TickValue)
                return false;

            var dir = projectile.Direction.normalized;
            var length = Mathf.Max(_shotPrefab.Radius, _shotPrefab.Speed * Sandbox.FixedDeltaTime);

            if (!Sandbox.Physics.Raycast(projectile.Position - length * dir, dir, out var hitInfo, length,
                    _shotPrefab.HitMask.value, QueryTriggerInteraction.Ignore))
                return false;

            projectile.Position = hitInfo.point;
            projectile.EndTick = Sandbox.Tick.TickValue;

            // Secondary weapons might have area damage
            if (_shotPrefab.AreaRadius > 0)
            {
                _damageService?.DealAreaDamage(hitInfo.point, _shotPrefab.AreaRadius, projectile.Damage,
                    _shotPrefab.HitMask, gameObject);
            }
            else
            {
                _damageService?.DealDamage(hitInfo.collider.gameObject, projectile.Damage, hitInfo.point, gameObject);
            }

            Debug.DrawLine(hitInfo.point, hitInfo.point + Vector3.up * 5, Color.blue, 1f);
            return true;
        });
    }

    public override void NetworkRender()
    {
        if (!IsServer)
            _projectiles?.Render(this, _projectileStates);
    }

    public override void Fire(Vector3 aimDirection)
    {
        if (_projectiles == null)
        {
            _projectiles = new SparseCollection<ShotState, Shot>(_projectileStates, _shotPrefab);
        }

        if (PowerupType != _weaponManager.CurrentSecondaryPowerupType) return;

        FireTime = _fireInterval;
        Ammo--;
        ProjectileID++;

        var position = GetFirePosition(aimDirection);
        Debug.Log($"Firing secondary projectile from position: {position} with direction: {aimDirection}");

        // Create shot state - secondary weapons might use different movement types
        var shotState = CreateShotState(position, aimDirection);

        _projectiles.Add(Sandbox, shotState, _shotPrefab.LifeTime);
    }

    protected override void SwitchToDefault()
    {
        var currentType = _weaponManager.CurrentSecondaryPowerupType;
        if (currentType != PowerupType || currentType == PowerupType.EMPTY) return;
        if (Ammo > 0) return;
        _weaponManager.SetPowerupType(PowerupType.EMPTY);
    }
    
    public override void NetworkDestroy()
    {
        _projectiles?.Clear();
        base.NetworkDestroy();
    }

    public int GetAmmo() => Ammo;
}
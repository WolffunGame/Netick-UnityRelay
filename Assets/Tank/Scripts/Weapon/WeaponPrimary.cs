    using UnityEngine;
using Helpers;
using Netick;
using Netick.Unity;
using Tank.Scripts.Utility;

public class WeaponPrimary : BaseWeapon
{
    private const byte MaxAmmo = 254;
    
    [SerializeField] private PowerupType _powerupType = PowerupType.DEFAULT;
    
    [Networked] private byte BulletID { get; set; }

    [Networked(size: MaxAmmo)] [Smooth(false)]
    private readonly NetworkArray<ShotState> _bulletStates = new(MaxAmmo);

    private SparseCollection<ShotState, Shot> _bullets;

    public override PowerupType PowerupType { get => _powerupType; protected set => _powerupType = value; }

    public override WeaponInstallationType InstallationType
    {
        get => WeaponInstallationType.PRIMARY;
        protected set => throw new System.NotImplementedException("Primary weapon installation type cannot be set.");
    }

    public void Start() 
    {
        _bullets = new SparseCollection<ShotState, Shot>(_bulletStates, _shotPrefab);
    }

    protected override void ProcessWeaponLogic()
    {
        base.ProcessWeaponLogic();
        ProcessBullets();
        
        var input = Tank.InputDelayHandle.InputData;
        if (input.IsDown(InputData.BUTTON_FIRE_PRIMARY) && CanFire())
        {
            Fire(input.GetAimDirection().XOY());
        }
    }

    private void ProcessBullets()
    {
        var input = Tank.InputDelayHandle.InputData;
        if (input.IsDown(InputData.BUTTON_FIRE_SECONDARY))
        {
            for (var i = 0; i < _bulletStates.Length; i++)
            {
                var temp = _bulletStates[i];
                var t = (Sandbox.Tick.TickValue - temp.StartTick + 1) * Sandbox.FixedDeltaTime;
                
                // Update position based on movement type
                switch (temp.MovementType)
                {
                    case ShotMovementType.Linear:
                        temp.Position = temp.GetLinearPositionAt(t);
                        break;
                    case ShotMovementType.Parabolic:
                        temp.Position = temp.GetParabolicPositionAt(t, _shotPrefab);
                        break;
                    case ShotMovementType.ZigZag:
                        temp.Position = temp.GetZigZagPositionAt(t, _shotPrefab);
                        break;
                }
                
                temp.StartTick = Sandbox.Tick.TickValue;
                temp.Speed = temp.Speed == 0 ? _shotPrefab.Speed : 0;
                _bulletStates[i] = temp;
            }
            return;
        }

        _bullets?.Process(this, (ref ShotState bullet, int _) =>
        {
            if (_shotPrefab.IsHitScan || bullet.EndTick <= Sandbox.Tick.TickValue)
                return false;
                
            var dir = bullet.Direction.normalized;
            var length = Mathf.Max(_shotPrefab.Radius, _shotPrefab.Speed * Sandbox.FixedDeltaTime);
            
            if (!Sandbox.Physics.Raycast(bullet.Position - length * dir, dir, out var hitInfo, length,
                    _shotPrefab.HitMask.value, QueryTriggerInteraction.Ignore)) 
                return false;
                
            bullet.Position = hitInfo.point;
            bullet.EndTick = Sandbox.Tick.TickValue;
            
            // Use damage service for hit processing
            _damageService?.DealDamage(hitInfo.collider.gameObject, bullet.Damage, hitInfo.point, gameObject);
            
            Debug.DrawLine(hitInfo.point, hitInfo.point + Vector3.up * 5, Color.red, 1f);
            return true;
        });
    }

    public override void NetworkRender()
    {
        if (!IsServer)
            _bullets?.Render(this, _bulletStates);
    }

    protected override void SwitchToDefault()
    {
        if (_weaponManager.CurrentPrimaryPowerupType != PowerupType) return;
        if (Ammo > 0) return;
        _weaponManager.SetPowerupType(PowerupType.DEFAULT);
    }

    public override void Fire(Vector3 aimDirection)
    {
        if (_bullets == null)
            _bullets = new SparseCollection<ShotState, Shot>(_bulletStates, _shotPrefab);

        if (PowerupType != _weaponManager.CurrentPrimaryPowerupType) return;
        
        var position = GetFirePosition(aimDirection);

        var shotState = CreateShotState(position, aimDirection);
        
        Debug.Log($"Firing primary shot from position: {position} with direction: {aimDirection} lifetime: {_shotPrefab.LifeTime}");
        
        FireTime = _fireInterval;
        Ammo--;
        BulletID++;
        _bullets.Add(Sandbox, shotState, _shotPrefab.LifeTime);
    }
    
    

    public override void NetworkDestroy() 
    {
        _bullets?.Clear();
        base.NetworkDestroy();
    }
}

using Examples.Tank;
using Helpers;
using Netick;
using Netick.Unity;
using Tank.Scripts.Utility;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [SerializeField] private Transform _turret;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _reloadTime;
    [SerializeField] private byte _maxAmmo = 5;
    [SerializeField] private float _fireInterval = .4f;
    [SerializeField] private Tank.Scripts.Tank _tank;
    [SerializeField] private NetworkObject _bulletPrefab;
    [SerializeField] private MuzzleFlash _muzzleFlash;
    [SerializeField] private Shot _bulletShotPrefab;

    [Networked] public byte Ammo { get; set; }
    [Networked] public float CurrentReloadTime { get; set; }
    [Networked] public float FireTime { get; set; }
    [Networked] public byte BulletID { get; set; }

    [Networked(size: 32)] [Smooth(false)] private readonly NetworkArray<ShotState> _bulletStates = new(32);

    private readonly NetworkArray<ShotState> _fromStates = new(32);

    private SparseCollection<ShotState, Shot> _bullets;

    private Interpolator _interpolation;

    public void Start() => _bullets = new SparseCollection<ShotState, Shot>(_bulletStates, _bulletShotPrefab);

    public override void NetworkStart()
    {
        base.NetworkStart();
        Sandbox.InitializePool(_bulletPrefab.gameObject, 20);
        _interpolation = FindInterpolator(nameof(_bulletStates));
    }

    public override void NetworkFixedUpdate()
    {
        AutoReloadAmmo();

        ProcessBullets();

        if (FireTime > 0)
            FireTime -= Sandbox.FixedDeltaTime;

        if (!FetchInput(out InputData input))
            return;
        if (!input.IsDown(InputData.BUTTON_FIRE_PRIMARY) || FireTime > 0 || Ammo <= 0)
            return;
        Fire(input.GetAimDirection().XOY());
    }

    private void ProcessBullets()
    {
        _bullets?.Process(this, (ref ShotState bullet, int _) =>
        {
            if (bullet.Position.y < -.15f)
            {
                bullet.EndTick = Sandbox.Tick.TickValue;
                return true;
            }

            if (_bulletShotPrefab.IsHitScan || bullet.EndTick <= Sandbox.Tick.TickValue) return false;
            var dir = bullet.Direction.normalized;
            var length = Mathf.Max(_bulletShotPrefab.Radius, _bulletShotPrefab.Speed * Sandbox.FixedDeltaTime);
            if (!Sandbox.Physics.Raycast(bullet.Position - length * dir, dir, out var hitInfo, length,
                    _bulletShotPrefab.HitMask.value, QueryTriggerInteraction.Ignore)) return false;
            bullet.Position = hitInfo.point;
            bullet.EndTick = Sandbox.Tick.TickValue;
            return true;
        });
    }

    public override void NetworkRender()
    {
        for (var i = 0; i < _bulletStates.Length; i++)
        {
            if (!_interpolation.GetInterpolationData<ShotState>(InterpolationSource.Auto, i, out var from, out _,
                    out _))
                continue;
            _fromStates[i] = from;
        }

        _bullets?.Render(this, _fromStates);
    }

    private void AutoReloadAmmo()
    {
        if (Ammo >= _maxAmmo)
            return;
        CurrentReloadTime += Sandbox.FixedDeltaTime;
        if (!(CurrentReloadTime >= _reloadTime))
            return;
        Ammo++;
        CurrentReloadTime = 0;
    }

    private void Fire(Vector3 aimDir)
    {
        FireTime = _fireInterval;
        Ammo--;
        BulletID++;
        var position = _firePoint.position;
        Draw.DrawArrow(position, position + aimDir * 10, Color.blue, 2);
        _bullets.Add(Sandbox, new ShotState(position, aimDir), _bulletShotPrefab.TimeToLive);
    }

    public override void NetworkDestroy() => _bullets.Clear();
}
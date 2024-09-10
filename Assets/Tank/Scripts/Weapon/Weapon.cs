using Helpers;
using Netick;
using Netick.Unity;
using Tank.Scripts.Utility;
using UnityEngine;

public class Weapon  : TankComponent
{
     private const byte MaxAmmo = 254;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _reloadTime;
    [SerializeField] private byte _maxAmmo = 5;
    [SerializeField] private float _fireInterval = .4f;
    [SerializeField] private NetworkObject _bulletPrefab;
    [SerializeField] private Shot _bulletShotPrefab;
    private Vector3 _offset;

    [Networked] private byte Ammo { get; set; }
    [Networked] private float CurrentReloadTime { get; set; }
    [Networked] private float FireTime { get; set; }
    [Networked] private byte BulletID { get; set; }


    [Networked(size: MaxAmmo)] [Smooth(false)] private readonly NetworkArray<ShotState> _bulletStates = new(MaxAmmo);


    private SparseCollection<ShotState, Shot> _bullets;

    public Transform FirePoint =>_firePoint;

    private void Awake() => _offset = _firePoint.position - transform.position;
    public void Start() => _bullets = new SparseCollection<ShotState, Shot>(_bulletStates, _bulletShotPrefab);

    public override void NetworkStart()
    {
        base.NetworkStart();
        Sandbox.InitializePool(_bulletPrefab.gameObject, 20);
    }

    public override void NetworkFixedUpdate()
    {
        AutoReloadAmmo();
        ProcessBullets();
        if (FireTime > 0)
            FireTime -= Sandbox.FixedDeltaTime;
        var input = Tank.InputDelayHandle.InputData;
        if (!input.IsDown(InputData.BUTTON_FIRE_PRIMARY) || FireTime > 0 || Ammo <= 0)
            return;
        Fire(input.GetAimDirection().XOY());
    }

    private void ProcessBullets()
    {
        if (FetchInput(out InputData input) && input.IsDown(InputData.BUTTON_FIRE_SECONDARY))
        {
            for (var i = 0; i < _bulletStates.Length; i++)
            {
                var temp = _bulletStates[i];
                var t = (Sandbox.Tick.TickValue - temp.StartTick + 1) * Sandbox.FixedDeltaTime;
                temp.Position = temp.GetPositionAt(t);
                temp.StartTick = Sandbox.Tick.TickValue;
                temp.Speed = temp.Speed == 0 ? _bulletShotPrefab.Speed : 0;
                _bulletStates[i] = temp;
            }

            return;
        }

        _bullets?.Process(this, (ref ShotState bullet, int _) =>
        {
            if (_bulletShotPrefab.IsHitScan || bullet.EndTick <= Sandbox.Tick.TickValue)
                return false;
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
        if (!IsServer)
            _bullets?.Render(this, _bulletStates);
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
        var position = transform.position + Quaternion.LookRotation( aimDir) * _offset;
        _bullets.Add(Sandbox, new ShotState
        {
            Position = position,
            Direction = aimDir,
            StartTick = Sandbox.Tick.TickValue,
            EndTick = Sandbox.Tick.TickValue + 100,
            Speed = _bulletShotPrefab.Speed,
        }, _bulletShotPrefab.TimeToLive);
    }

    public override void NetworkDestroy() => _bullets.Clear();
}
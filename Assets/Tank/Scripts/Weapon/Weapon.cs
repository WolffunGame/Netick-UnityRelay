using Examples.Tank;
using Netick;
using Netick.Unity;
using UnityEngine;
using UnityEngine.Serialization;

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

    [Networked] public byte Ammo { get; set; }
    [Networked] public float CurrentReloadTime { get; set; }

    [Networked] public float FireTime;

    public override void NetworkStart()
    {
        base.NetworkStart();
        Sandbox.InitializePool(_bulletPrefab.gameObject, 20);
    }

    public override void NetworkFixedUpdate()
    {
        AutoReloadAmmo();

        if (FireTime > 0)
            FireTime -= Time.fixedDeltaTime;

        if (!FetchInput(out InputData input))
            return;
        if (!input.IsDown(InputData.BUTTON_FIRE_PRIMARY) || FireTime > 0 || Ammo <= 0)
            return;
        Fire();
    }

    private void AutoReloadAmmo()
    {
        if (Ammo >= _maxAmmo) return;
        CurrentReloadTime += Time.fixedDeltaTime;
        if (!(CurrentReloadTime >= _reloadTime)) return;
        Ammo++;
        CurrentReloadTime = 0;
    }

    private void Fire()
    {
        if (!IsServer)
        {
            if (Sandbox.IsResimulating) 
                return;
            var tran = transform;
            Debug.LogError($"Fire {Sandbox.Tick} {Sandbox.IsResimulating}");
            LocalObjectPool.Acquire(_muzzleFlash, _firePoint.position, _firePoint.rotation, tran);
            return;
        }
        FireTime = _fireInterval;
        Ammo--;
        Sandbox.NetworkInstantiate(_bulletPrefab.gameObject, _firePoint.position, _turret.rotation);
        RpcShot();
    }

    [Rpc(target: RpcPeers.Everyone)]
    private void RpcShot()
    {
        if(IsOwner)
            return;
        var tran = transform;
        LocalObjectPool.Acquire(_muzzleFlash, _firePoint.position, _firePoint.rotation, tran);
    }
}
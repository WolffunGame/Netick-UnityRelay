using Netick;
using Netick.Unity;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [SerializeField] private Transform _turret;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _reloadTime;
    [SerializeField] private byte _maxAmmo = 5;
    [SerializeField] private float _fireInterval = .4f;
    [SerializeField] Tank.Scripts.Tank _tank;
    [SerializeField] private NetworkObject _bulletPrefab;

    [Networked] public byte Ammo { get; set; }
    [Networked] public float CurrentReloadTime { get; set; }

    [Networked(relevancy:Relevancy.InputSource)] public float _fireTime;

    public override void NetworkStart()
    {
        base.NetworkStart();
        Sandbox.InitializePool(_bulletPrefab.gameObject, 20);
    }


    public override void NetworkFixedUpdate()
    {
        AutoReloadAmmo();

        if (_fireTime > 0)
            _fireTime -= Time.fixedDeltaTime;
        
        if (!FetchInput(out InputData input))
            return;
        if (!input.IsDown(InputData.BUTTON_FIRE_PRIMARY) || _fireTime > 0 || Ammo <= 0)
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
        if(!IsServer)
            return;
        _fireTime = _fireInterval;
        Ammo--;
        Sandbox.NetworkInstantiate(_bulletPrefab.gameObject, _firePoint.position, _turret.rotation);
    }
}
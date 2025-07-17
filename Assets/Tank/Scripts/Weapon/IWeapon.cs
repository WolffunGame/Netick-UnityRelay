using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeapon
{
    PowerupType PowerupType { get; }
    WeaponInstallationType InstallationType { get; }
    Transform FirePoint { get; }
    
    void InjectDependencies(IWeaponService weaponService, IDamageService damageService, IShotFactory shotFactory);
    void Initialize(IWeaponManager weaponManager, WeaponData weaponData);
    void Activate();
    void Deactivate();
    void Fire(Vector3 aimDirection);
    bool CanFire();
    void Reload();

}

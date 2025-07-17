using UnityEngine;

public interface IWeaponService
{
    void RegisterWeapon(BaseWeapon weapon);
    void UnregisterWeapon(BaseWeapon weapon);
    BaseWeapon GetWeapon(PowerupType type, WeaponInstallationType installationType);
    void SwitchWeapon(PowerupType type);
    void ReloadAllWeapons();
}
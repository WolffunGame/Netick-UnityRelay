using System.Collections;
using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

public class WeaponService : IWeaponService
{
    [Inject] private WeaponDataProvider _weaponDataProvider;
    
    private readonly Dictionary<(PowerupType, WeaponInstallationType), BaseWeapon> _weapons;

    public WeaponService()
    {
        _weapons = new Dictionary<(PowerupType, WeaponInstallationType), BaseWeapon>();
        Debug.Log("WeaponService initialized");
    }

    public void RegisterWeapon(BaseWeapon weapon)
    {
        if (weapon == null) return;
        
        var key = (weapon.PowerupType, weapon.InstallationType);
        _weapons[key] = weapon;
        Debug.Log($"Registered weapon: {weapon.PowerupType} - {weapon.InstallationType}");
    }

    public void UnregisterWeapon(BaseWeapon weapon)
    {
        if (weapon == null) return;
        
        var key = (weapon.PowerupType, weapon.InstallationType);
        if (_weapons.Remove(key))
        {
            Debug.Log($"Unregistered weapon: {weapon.PowerupType} - {weapon.InstallationType}");
        }
    }

    public BaseWeapon GetWeapon(PowerupType type, WeaponInstallationType installationType)
    {
        var key = (type, installationType);
        return _weapons.TryGetValue(key, out var weapon) ? weapon : null;
    }

    public void SwitchWeapon(PowerupType type)
    {
        var weaponData = _weaponDataProvider?.GetWeaponData(type);
        if (weaponData != null)
        {
            Debug.Log($"Switching to weapon: ({type})");
        }
        else
        {
            Debug.LogWarning($"No weapon data found for type: {type}");
        }
    }

    public void ReloadAllWeapons()
    {
        int reloadedCount = 0;
        foreach (var weapon in _weapons.Values)
        {
            weapon.Reload();
            reloadedCount++;
        }
        Debug.Log($"Reloaded {reloadedCount} weapons");
    }

    public List<BaseWeapon> GetAllWeapons()
    {
        return new List<BaseWeapon>(_weapons.Values);
    }

    public List<BaseWeapon> GetWeaponsByType(PowerupType type)
    {
        var result = new List<BaseWeapon>();
        foreach (var kvp in _weapons)
        {
            if (kvp.Key.Item1 == type)
            {
                result.Add(kvp.Value);
            }
        }
        return result;
    }
}
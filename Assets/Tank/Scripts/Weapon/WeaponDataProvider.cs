using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

public class WeaponDataProvider
{
    private readonly WeaponConfig _config;
    private readonly Dictionary<PowerupType, WeaponData> _weaponDataCache;

    public WeaponDataProvider(WeaponConfig config)
    {
        _config = config;
        _weaponDataCache = new Dictionary<PowerupType, WeaponData>();
        InitializeCache();
    }

    public WeaponData GetWeaponData(PowerupType type)
    {
        return _weaponDataCache.TryGetValue(type, out var data) ? data : null;
    }

    public List<WeaponData> GetAllWeaponData()
    {
        return new List<WeaponData>(_weaponDataCache.Values);
    }

    public bool HasWeaponData(PowerupType type)
    {
        return _weaponDataCache.ContainsKey(type);
    }

    private void InitializeCache()
    {
        if (_config?.WeaponDatas != null)
        {
            _weaponDataCache.Clear();
            foreach (var weaponData in _config.WeaponDatas)
            {
                _weaponDataCache[weaponData.Type] = weaponData;
            }
        }

        Debug.Log($"WeaponDataProvider: Cached {_weaponDataCache.Count} weapons");
    }
}
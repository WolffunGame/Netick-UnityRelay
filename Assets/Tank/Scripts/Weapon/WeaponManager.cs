using System;
using System.Collections.Generic;
using System.Linq;
using Netick;
using Netick.Unity;
using Reflex.Attributes;
using Reflex.Extensions;
using UnityEngine;

public class WeaponManager : NetworkBehaviour, IWeaponManager
{
    [Serializable]
    public class WeaponVisual
    {
        public PowerupType Type;
        public WeaponInstallationType InstallationType;
        public BaseWeapon Weapon;
    }

    [Header("Weapon Setup")] public WeaponVisual[] WeaponVisuals;

    private Dictionary<PowerupType, BaseWeapon> _primaryWeapons;
    private Dictionary<PowerupType, BaseWeapon> _secondaryWeapons;

    [Networked] public PowerupType CurrentPrimaryPowerupType { get; private set; } = PowerupType.DEFAULT;
    [Networked] public PowerupType CurrentSecondaryPowerupType { get; private set; } = PowerupType.DEFAULT;

    private BaseWeapon _currentPrimaryWeapon;
    private BaseWeapon _currentSecondaryWeapon;

    [Inject] protected IDamageService _damageService;
    [Inject] protected WeaponDataProvider _weaponDataProvider;
    [Inject] protected IShotFactory _shotFactory;


    public override void NetworkAwake()
    {
        base.NetworkAwake();
        ResolveDependencies();
        InitializeWeapons();
        SetupWeapons();
    }

    public override void NetworkStart()
    {
        SetPowerupType(PowerupType.DEFAULT);
    }

    private void SetupWeapons()
    {
        foreach (var weapon in _primaryWeapons.Values.Concat(_secondaryWeapons.Values))
        {
            if (weapon == null)
            {
                Debug.LogWarning($"Weapon is null.");
                continue;
            }

            // Inject dependencies into weapons
            var weaponData = _weaponDataProvider.GetWeaponData(weapon.PowerupType);
            weapon.InjectDependencies(_damageService, _shotFactory);
            weapon.Initialize(this, weaponData);
            weapon.gameObject.SetActive(false);
        }
    }


    public void InitializeWeapons()
    {
        if (WeaponVisuals == null || WeaponVisuals.Length == 0)
        {
            Debug.LogWarning("WeaponVisuals array is not set or empty.");
            return;
        }

        _primaryWeapons ??= new Dictionary<PowerupType, BaseWeapon>();
        _secondaryWeapons ??= new Dictionary<PowerupType, BaseWeapon>();

        foreach (var visual in WeaponVisuals)
        {
            if (visual.Weapon == null)
            {
                Debug.LogWarning($"WeaponVisual prefab for type {visual.Type} is not set.");
                continue;
            }

            switch (visual.InstallationType)
            {
                case WeaponInstallationType.PRIMARY:
                    _primaryWeapons[visual.Type] = visual.Weapon;
                    break;
                case WeaponInstallationType.SECONDARY:
                    Debug.Log("Adding secondary weapon: " + visual.Type);
                    _secondaryWeapons[visual.Type] = visual.Weapon;
                    break;
            }
        }
    }

    public void SetPowerupType(PowerupType powerupType)
    {
        Debug.Log("Setting powerup type: " + powerupType);
        // Handle primary weapons
        if (_primaryWeapons.ContainsKey(powerupType))
        {
            var nextPrimaryWeapon = _primaryWeapons[powerupType];
            if (nextPrimaryWeapon != null)
            {
                _currentPrimaryWeapon?.Deactivate();
                _currentPrimaryWeapon = nextPrimaryWeapon;
                _currentPrimaryWeapon.Activate();
                CurrentPrimaryPowerupType = powerupType;
            }
        }
        // Handle secondary weapons
        if (_secondaryWeapons.ContainsKey(powerupType))
        {
            var nextSecondaryWeapon = _secondaryWeapons[powerupType];
            if (nextSecondaryWeapon != null)
            {
                _currentSecondaryWeapon?.Deactivate();
                _currentSecondaryWeapon = nextSecondaryWeapon;
                _currentSecondaryWeapon.Activate();
                CurrentSecondaryPowerupType = powerupType;
            }
        }
        if(powerupType == PowerupType.EMPTY)
        {
            _currentSecondaryWeapon?.Deactivate();
            _currentSecondaryWeapon = null;
            CurrentSecondaryPowerupType = PowerupType.EMPTY;
        }
    }


    public void ForceDeactivateWeaponByType(PowerupType powerupType)
    {
        Debug.Log($"Force deactivating weapon of type: {powerupType}");
        if (_currentPrimaryWeapon != null && _currentPrimaryWeapon.PowerupType == powerupType)
        {
            _currentPrimaryWeapon.Deactivate();
            _currentPrimaryWeapon = null;
            CurrentPrimaryPowerupType = PowerupType.DEFAULT;
        }

        if (_currentSecondaryWeapon != null && _currentSecondaryWeapon.PowerupType == powerupType)
        {
            _currentSecondaryWeapon.Deactivate();
            _currentSecondaryWeapon = null;
            CurrentSecondaryPowerupType = PowerupType.EMPTY;
        }
    }


    public BaseWeapon GetCurrentPrimaryWeapon() => _currentPrimaryWeapon;
    public BaseWeapon GetCurrentSecondaryWeapon() => _currentSecondaryWeapon;

    // Helper methods for laser weapons
    public WeaponSecondaryLaser GetCurrentLaserWeapon()
    {
        return _currentSecondaryWeapon as WeaponSecondaryLaser;
    }

    public bool HasLaserWeapon()
    {
        return _currentSecondaryWeapon is WeaponSecondaryLaser;
    }

    public void ToggleLaser()
    {
        var laserWeapon = GetCurrentLaserWeapon();
        laserWeapon?.ToggleLaserSight();
    }

    private void ResolveDependencies()
    {
        var sceneContainer = gameObject.scene.GetSceneContainer();
        if (sceneContainer != null)
        {
            try
            {
                _damageService = sceneContainer.Resolve<IDamageService>();
                _weaponDataProvider = sceneContainer.Resolve<WeaponDataProvider>();
                _shotFactory = sceneContainer.Resolve<IShotFactory>();
                if (_weaponDataProvider == null || _damageService == null ||
                    _shotFactory == null)
                {
                    Debug.LogWarning($"{name}: Some dependencies are not resolved properly. " +
                                     $"WeaponDataProvider: {_weaponDataProvider != null}, " +
                                     $"DamageService: {_damageService != null}, " +
                                     $"ShotFactory: {_shotFactory != null}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{name}: Failed to resolve from scene container: {e.Message}");
            }
        }
    }
}
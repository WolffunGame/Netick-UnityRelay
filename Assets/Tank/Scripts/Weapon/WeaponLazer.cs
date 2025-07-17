using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Examples.Tank;
using Helpers;
using Netick;
using Netick.Unity;
using Tank.Scripts.Utility;
using UnityEngine;

public class WeaponSecondaryLaser : WeaponSecondary
{
    [SerializeField] private LaserSightLine _laserSight;
    [SerializeField] private float _laserDuration = 0.5f;
    [SerializeField] private bool _autoActivateLaser = true;

    private bool _isLaserSightActive;

    public override void Fire(Vector3 aimDirection)
    {
        if (PowerupType != _weaponManager.CurrentSecondaryPowerupType) return;

        // Laser-specific fire behavior
        if (_isLaserSightActive && _laserSight != null)
            _laserSight.Recharge();

        // Call base fire method
        base.Fire(aimDirection);
    }


    public override void Activate()
    {
        base.Activate();
        
        if (_autoActivateLaser)
        {
            ActivateLaserSight(true);
        }
    }

    public override void Deactivate()
    {
        DeactivateLaserSight();
        base.Deactivate();
    }

    public void ActivateLaserSight(bool active)
    {
        if (_laserSight == null) return;
        
        _isLaserSightActive = active;
        if (active)
        {
            _laserSight.SetDuration(_laserDuration);
            _laserSight.Activate();
        }
        else
        {
            _laserSight.Deactivate();
        }
    }

    public void DeactivateLaserSight()
    {
        if (_laserSight != null)
            _laserSight.Deactivate();
        _isLaserSightActive = false;
    }

    public void ToggleLaserSight()
    {
        ActivateLaserSight(!_isLaserSightActive);
    }

    public bool IsLaserActive() => _isLaserSightActive;
    public LaserSightLine GetLaserSight() => _laserSight;
}

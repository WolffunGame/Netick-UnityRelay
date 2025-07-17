using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeaponManager
{
    void InitializeWeapons();
    void SetPowerupType(PowerupType powerupType);
    void ForceDeactivateWeaponByType(PowerupType powerupType);
    PowerupType CurrentPrimaryPowerupType { get; }
    PowerupType CurrentSecondaryPowerupType { get; }
}

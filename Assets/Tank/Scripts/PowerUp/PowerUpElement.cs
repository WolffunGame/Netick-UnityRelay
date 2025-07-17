using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerupType
{
    DEFAULT = 0,
    MINIGUN = 1,
    GIGAVOLT = 2,
    GRENADES = 3,
    HEALTH = 4,
    EMPTY = 5
}
	
public enum WeaponInstallationType
{
    PRIMARY,
    SECONDARY,
    BUFF
};

[CreateAssetMenu(fileName = "PE_", menuName = "ScriptableObjects/PowerupElement")]
public class PowerupElement : ScriptableObject
{
    public WeaponInstallationType weaponInstallationType;
    public PowerupType powerupType;
    public Mesh powerupSpawnerMesh;
}

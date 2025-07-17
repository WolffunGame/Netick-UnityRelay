using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "WeaponConfig", menuName = "Weapons/Weapon Config")]
public class WeaponConfig : ScriptableObject
{
    [SerializeField] private List<WeaponData> _weaponDatas;
    public List<WeaponData> WeaponDatas => _weaponDatas;
}

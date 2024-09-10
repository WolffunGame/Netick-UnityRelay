using Netick.Unity;
using UnityEngine;

public abstract class TankComponent : NetworkBehaviour
{
    [SerializeField] protected Tank.Scripts.Tank Tank;

    protected void OnValidate()=>Tank??= GetComponentInChildren<Tank.Scripts.Tank>();
}
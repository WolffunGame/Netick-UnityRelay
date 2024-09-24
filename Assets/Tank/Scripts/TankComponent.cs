using Netick;
using Netick.Unity;
using UnityEngine;

[ExecutionOrder(0)]
public abstract class TankComponent : NetworkBehaviour
{
    [SerializeField] protected Tank.Scripts.Tank Tank;

    protected void OnValidate()=>Tank??= GetComponentInChildren<Tank.Scripts.Tank>();
}
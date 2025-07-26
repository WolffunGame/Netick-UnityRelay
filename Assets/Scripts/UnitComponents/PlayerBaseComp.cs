using Netick.Unity;
using UnityEngine;

namespace UnitComponents
{
    public abstract class PlayerBaseComp : NetworkBehaviour
    {
       [SerializeField] protected Player Player;

       protected void OnValidate()
       {
           Player??= GetComponent<Player>();
           Player??= GetComponentInParent<Player>();
       }
    }
}
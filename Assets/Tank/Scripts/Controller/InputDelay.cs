using Netick;
using Netick.Unity;
using UnityEngine;

[ExecuteBefore(typeof(TankMoveControl))]
public class InputDelay : NetworkBehaviour
{
    [SerializeField] private byte _delayTick = 2;
    [Networked] private NetworkArrayStruct32<InputData> InputData { get; set; }
    
    public override void NetworkFixedUpdate()
    {
        if(!FetchInput(out InputData data))
            return;
        var index = (Sandbox.Tick.TickValue + _delayTick) % InputData.Length;
        InputData = InputData.Set(index, data);
    }

    public InputData GetInput()
    {
        var index = Sandbox.Tick.TickValue % InputData.Length;
        return index < InputData.Length ? InputData[index] : default;
    } 
}
using Netick;
using Netick.Unity;
using UnityEngine;

public class InputDelayHandle : NetworkBehaviour
{
    [SerializeField] private int _tickDelay30Fps = 3;
    [Networked] private NetworkArrayStruct16<InputData> QueueInput { get; set; }
    [Networked] private int Index { get; set; }
    [Networked] public InputData InputData { get; set; }

    [SerializeField] private int _tickDelay;
    public override void NetworkStart() => _tickDelay = _tickDelay30Fps * (int)Sandbox.Config.TickRate / 30;

    public override void NetworkFixedUpdate()
    {
        if (FetchInput(out InputData input))
        {
            Index++;
            if (QueueInput.Length <= Index)
                Index = 0;
            QueueInput = QueueInput.Set(Index, input);
        }
            
        foreach (var i in QueueInput)
        {
            if (Sandbox.Tick.TickValue - i.Tick != _tickDelay) continue;
            InputData = i;
            break;
        }
    }
}
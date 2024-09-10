using Netick;
using Netick.Unity;
using UnityEngine;

public class InputDelayHandle : NetworkBehaviour
{
    [SerializeField] private int _tickDelay = 3;
    [Networked(size: 15)] private readonly NetworkQueue<InputData> _queueInput = new(15);
    [Networked] public InputData InputData { get; private set; }

    public override void NetworkFixedUpdate()
    {
        if (FetchInput(out InputData input))
            _queueInput.Enqueue(input);

        foreach (var i in _queueInput)
        {
            if (Sandbox.Tick.TickValue - i.Tick != _tickDelay) continue;
            InputData = i;
            _queueInput.Dequeue();
            break;
        }
    }
}
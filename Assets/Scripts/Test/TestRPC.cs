using System.Globalization;
using Netick;
using Netick.Unity;
using UnityEngine;

public class TestRPC : NetworkBehaviour
{
    [SerializeField] private int _delayTick = 30;

    public override void NetworkFixedUpdate()
    {
        if (IsServer || Sandbox.Tick.TickValue % _delayTick != 0)
            return;
        RpcNumber(Random.value);
    }

    [Rpc(isReliable: true, target: RpcPeers.Owner)]
    private void RpcNumber(float number) => RpcBackNumber(number);

    [Rpc(source: RpcPeers.Owner,isReliable: true,  target: RpcPeers.Everyone)]
    private void RpcBackNumber(float number) => Debug.LogError(number.ToString(CultureInfo.InvariantCulture));
}
using Netick;
using Netick.Unity;
using UnityEngine;

public class TempWeapon : NetworkBehaviour
{
    [Networked] [Smooth(false)] private NetworkArrayStruct32<ShotState> _bulletState { get; set; }

    private Interpolator _interpolation;

    public override void NetworkStart()
    {
        base.NetworkStart();
        _interpolation = FindInterpolator(nameof(_bulletState));
    }

    public override void NetworkFixedUpdate()
    {
        for (var i = 0; i < _bulletState.Length; i++)
        {
            _bulletState = _bulletState.Set( i, new ShotState
            {
                StartTick = Sandbox.Tick.TickValue + i
            });
        }
    }


    public override void NetworkRender()
    {
        if (!_interpolation.GetInterpolationData<NetworkArrayStruct32<ShotState>>(InterpolationSource.Auto, out var from, out _, out _))
            return;
    }
}
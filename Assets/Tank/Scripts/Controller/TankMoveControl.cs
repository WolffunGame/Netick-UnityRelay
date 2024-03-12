using Netick.Unity;
using Tank.Scripts.Utility;
using UnityEngine;

public class TankMoveControl : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private CharacterController _cc;
    [SerializeField] private Transform _turret;


    public override void NetworkStart()
    {
        base.NetworkStart();
        if (!IsClient || InputSource.Engine.LocalPlayer != Sandbox.Engine.LocalPlayer)
            return;
        if (!Sandbox.TryGetComponent<InputHandler>(out var inputHandler))
            return;
        inputHandler.SetPlayer(transform);
    }

    public override void NetworkFixedUpdate()
    {
        if (!FetchInput(out InputData input))
            return;
        var moveDir = input.GetMoveDirection().XOY();
        var aimDir = input.GetAimDirection().XOY();

        if (!_cc.isGrounded)
            moveDir.y += Physics.gravity.y;

        _cc.Move(moveDir * _moveSpeed * Sandbox.FixedDeltaTime);
        if (aimDir == Vector3.zero) return;
        var rot = _turret.rotation;
        var rotation = Quaternion.LookRotation(aimDir);
        rot = Quaternion.Lerp(rot, rotation, 10 * Sandbox.FixedDeltaTime);
        _turret.rotation = rot;
    }
}
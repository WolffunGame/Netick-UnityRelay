using Netick;
using Tank.Scripts.Utility;
using UnityEngine;

public class TankMoveControl : TankComponent
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _hullRotationSpeed = 5;
    [SerializeField] private float _turretRotationSpeed = 10;

    [SerializeField] private CharacterController  _cc;
    [SerializeField] private Transform _turret;
    [SerializeField] private Transform _null;

    [Networked] [Smooth(false)] private Quaternion TurretDir { get; set; }
    [Networked] [Smooth(false)] private Quaternion HullDir { get; set; }

    private Interpolator _interpolationDir;
    private Interpolator _interpolationHub;


    public override void NetworkStart()
    {
        _interpolationDir = FindInterpolator(nameof(TurretDir));
        _interpolationHub = FindInterpolator(nameof(HullDir));
        base.NetworkStart();
        if (!IsClient || InputSource?.Engine == null || Sandbox.Engine.LocalPlayer == null ||
            InputSource.Engine.LocalPlayer != Sandbox.Engine.LocalPlayer)
            return;
        if (!Sandbox.TryGetComponent<InputHandler>(out var inputHandler))
            return;
        inputHandler.SetPlayer(transform);
    }

    public override void NetworkFixedUpdate()
    {
        var moveDir = EncodeDir.DecodeDirection(Tank.InputDelayHandle.InputData.EncodedMoveDir).XOY();
        var aimDir = EncodeDir.DecodeDirection(Tank.InputDelayHandle.InputData.EncodedAimDir).XOY();
        RotateTurret(aimDir);
        RotateHull(moveDir);
        if (!_cc.isGrounded)
            moveDir.y += Physics.gravity.y;
        _cc.Move(moveDir * _moveSpeed * Sandbox.FixedDeltaTime);
        TurretDir = _turret.rotation;
        HullDir = _null.rotation;
    }

    public override void NetworkRender()
    {
        if (_interpolationDir.GetInterpolationData<Quaternion>(InterpolationSource.Auto, out var from, out var to,
                out var alpha))
            _turret.rotation = Quaternion.Slerp(from, to, alpha);
        if (_interpolationHub.GetInterpolationData(InterpolationSource.Auto, out from, out to, out alpha))
            _null.rotation = Quaternion.Slerp(from, to, alpha);
    }

    private void RotateTurret(Vector3 aimDir)
    {
        if (aimDir == default)
            return;
        var turretRotation = _turret.rotation;
        turretRotation = Quaternion.Slerp(turretRotation, Quaternion.LookRotation(aimDir),
            _turretRotationSpeed * Sandbox.FixedDeltaTime);
        _turret.rotation = turretRotation;
    }

    private void RotateHull(Vector3 moveDir)
    {
        if (moveDir == default)
            return;
        var hullRotation = _null.rotation;
        hullRotation = Quaternion.Lerp(hullRotation, Quaternion.LookRotation(moveDir),
            _hullRotationSpeed * Sandbox.FixedDeltaTime);
        _null.rotation = hullRotation;
    }
}
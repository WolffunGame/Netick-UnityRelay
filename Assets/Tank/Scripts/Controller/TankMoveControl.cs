using Netick;
using Netick.Unity;
using Tank.Scripts.Utility;
using UnityEngine;

public class TankMoveControl : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _hullRotationSpeed = 5;
    [SerializeField] private float _turretRotationSpeed = 10;

    [SerializeField] private CharacterController _cc;
    [SerializeField] private Transform _turret;
    [SerializeField] private Transform _null;

    [Networked] private float TurretDir { get; set; }
    [Networked] private float HullDir { get; set; }

    
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
   
        RotateTurret(aimDir);
        RotateHull(moveDir);
        
        if (!_cc.isGrounded)
            moveDir.y += Physics.gravity.y;
        _cc.Move(moveDir * _moveSpeed * Sandbox.FixedDeltaTime);
        TurretDir = _turret.rotation.eulerAngles.y;
        HullDir = _null.rotation.eulerAngles.y;
    }

    public override void NetworkRender()
    {
        if(IsOwner)
            return;
        var rotation = _turret.rotation;
        var turretRotation = Quaternion.Euler(0, TurretDir, 0);
        rotation = Quaternion.Slerp(rotation, turretRotation, 20 * Time.deltaTime);
        _turret.rotation = rotation;
        
        var hullRotation = _null.rotation;
        hullRotation = Quaternion.Slerp(hullRotation, Quaternion.Euler(0, HullDir, 0), 20 * Time.deltaTime);
        _null.rotation = hullRotation;
    }

    private void RotateTurret(Vector3 aimDir)
    {
        if(aimDir==default)
            return;
        var turretRotation = _turret.rotation;
        turretRotation = Quaternion.Slerp(turretRotation,  Quaternion.LookRotation(aimDir), _turretRotationSpeed * Sandbox.FixedDeltaTime);
        _turret.rotation = turretRotation;
    }
    
    private void RotateHull(Vector3 moveDir)
    {
        if(moveDir==default)
            return;
        var hullRotation = _null.rotation;
        hullRotation = Quaternion.Lerp(hullRotation, Quaternion.LookRotation(moveDir),
            _hullRotationSpeed * Sandbox.FixedDeltaTime);
        _null.rotation = hullRotation;
    }
}
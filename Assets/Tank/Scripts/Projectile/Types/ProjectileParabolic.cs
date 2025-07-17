using System.Collections;
using System.Collections.Generic;
using Netick;
using UnityEngine;

public class ProjectileParabolic : BaseProjectile
{
    [Header("Parabolic Projectile Settings")] [SerializeField]
    private Vector3 _gravity = new Vector3(0, -9.81f, 0);

    [SerializeField] private float _initialVerticalVelocity = 5f;
    [SerializeField] private bool _autoCalculateAngle = true;
    [SerializeField] private float _launchAngle = 45f;
    [SerializeField] private bool _rotateWithTrajectory = true;

    [Networked] private Vector3 Velocity { get; set; }
    [Networked] private float ElapsedTime { get; set; }

    private Vector3 _initialVelocity;

    public override void Initialize(Vector3 position, Vector3 direction, float damage = 0)
    {
        base.Initialize(position, direction, damage);
        CalculateInitialVelocity(direction);
        Velocity = _initialVelocity;
        ElapsedTime = 0f;
    }

    private void CalculateInitialVelocity(Vector3 direction)
    {
        if (_autoCalculateAngle)
        {
            var horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;
            var angle = _launchAngle * Mathf.Deg2Rad;

            _initialVelocity = horizontalDirection * (Speed * Mathf.Cos(angle));
            _initialVelocity.y = Speed * Mathf.Sin(angle);
        }
        else
        {
            var horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;
            _initialVelocity = horizontalDirection * Speed;
            _initialVelocity.y = _initialVerticalVelocity;
        }
    }

    protected override void ProcessMovement()
    {
        var oldPosition = transform.position;

        ElapsedTime += Sandbox.FixedDeltaTime;
        Velocity += _gravity * Sandbox.FixedDeltaTime;

        var newPosition = _initialPosition + _initialVelocity * ElapsedTime +
                          0.5f * _gravity * (ElapsedTime * ElapsedTime);

        transform.position = newPosition;

        if (_rotateWithTrajectory && Velocity.magnitude > 0.1f)
        {
            transform.forward = Velocity.normalized;
        }

        CheckCollision(oldPosition, newPosition);
    }

    public Vector3 GetTrajectoryPoint(float time)
    {
        return _initialPosition + _initialVelocity * time + 0.5f * _gravity * (time * time);
    }

    public void SetLaunchAngle(float angleDegrees)
    {
        _launchAngle = angleDegrees;
    }
}
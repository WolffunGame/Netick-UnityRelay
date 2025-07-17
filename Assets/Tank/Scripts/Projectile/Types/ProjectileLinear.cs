using UnityEngine;
using Netick;

public class ProjectileLinear : BaseProjectile
{
    [Header("Linear Projectile Settings")] [SerializeField]
    private bool _maintainDirection = true;

    [SerializeField] private float _accelerationRate = 0f;

    private Vector3 _currentVelocity;

    public override void Initialize(Vector3 position, Vector3 direction, float damage = 0)
    {
        base.Initialize(position, direction, damage);
        _currentVelocity = Direction * Speed;
    }

    protected override void ProcessMovement()
    {
        var oldPosition = transform.position;

        if (_accelerationRate != 0)
        {
            var accelerationVector = Direction * (_accelerationRate * Sandbox.FixedDeltaTime);
            _currentVelocity += accelerationVector;
        }

        var deltaMovement = _currentVelocity * Sandbox.FixedDeltaTime;
        var newPosition = oldPosition + deltaMovement;

        transform.position = newPosition;

        if (_maintainDirection)
        {
            transform.forward = _currentVelocity.normalized;
        }

        CheckCollision(oldPosition, newPosition);
    }

    public void SetAcceleration(float acceleration)
    {
        _accelerationRate = acceleration;
    }
}
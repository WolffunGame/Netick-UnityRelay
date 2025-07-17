using System.Collections;
using System.Collections.Generic;
using Netick;
using UnityEngine;

public class ProjectileZigZag : BaseProjectile
{
    [Header("ZigZag Projectile Settings")] [SerializeField]
    private float _zigzagAmplitude = 2f;

    [SerializeField] private float _zigzagFrequency = 3f;
    [SerializeField] private ZigZagPattern _pattern = ZigZagPattern.Horizontal;
    [SerializeField] private bool _randomizePhase = true;
    [SerializeField] private float _forwardSpeed = 10f;
    [SerializeField] private AnimationCurve _amplitudeCurve = AnimationCurve.Constant(0, 1, 1);

    [Networked] private float TravelTime { get; set; }
    [Networked] private float PhaseOffset { get; set; }

    private Vector3 _forwardDirection;
    private Vector3 _rightDirection;
    private Vector3 _upDirection;
    private Vector3 _startPosition;

    public enum ZigZagPattern
    {
        Horizontal,
        Vertical,
        Spiral,
        Figure8,
        Random
    }

    public override void Initialize(Vector3 position, Vector3 direction, float damage = 0)
    {
        base.Initialize(position, direction, damage);

        _startPosition = position;
        _forwardDirection = direction.normalized;
        _rightDirection = Vector3.Cross(_forwardDirection, Vector3.up).normalized;
        _upDirection = Vector3.Cross(_rightDirection, _forwardDirection).normalized;

        TravelTime = 0f;
        PhaseOffset = _randomizePhase ? Random.Range(0f, 2f * Mathf.PI) : 0f;
    }

    protected override void ProcessMovement()
    {
        var oldPosition = transform.position;

        TravelTime += Sandbox.FixedDeltaTime;
        var newPosition = CalculateZigZagPosition(TravelTime);

        transform.position = newPosition;

        var movementDirection = (newPosition - oldPosition).normalized;
        if (movementDirection.magnitude > 0.1f)
        {
            transform.forward = movementDirection;
        }

        CheckCollision(oldPosition, newPosition);
    }

    private Vector3 CalculateZigZagPosition(float time)
    {
        var forwardProgress = _forwardSpeed * time;
        var basePosition = _startPosition + _forwardDirection * forwardProgress;

        var amplitudeMultiplier = _amplitudeCurve.Evaluate(time / LifeTime);
        var currentAmplitude = _zigzagAmplitude * amplitudeMultiplier;

        Vector3 offset = Vector3.zero;

        switch (_pattern)
        {
            case ZigZagPattern.Horizontal:
                offset = CalculateHorizontalZigZag(time, currentAmplitude);
                break;
            case ZigZagPattern.Vertical:
                offset = CalculateVerticalZigZag(time, currentAmplitude);
                break;
            case ZigZagPattern.Spiral:
                offset = CalculateSpiralPattern(time, currentAmplitude);
                break;
            case ZigZagPattern.Figure8:
                offset = CalculateFigure8Pattern(time, currentAmplitude);
                break;
            case ZigZagPattern.Random:
                offset = CalculateRandomPattern(time, currentAmplitude);
                break;
        }

        return basePosition + offset;
    }

    private Vector3 CalculateHorizontalZigZag(float time, float amplitude)
    {
        var zigzagValue = Mathf.Sin((time * _zigzagFrequency + PhaseOffset) * 2f * Mathf.PI);
        return _rightDirection * (zigzagValue * amplitude);
    }

    private Vector3 CalculateVerticalZigZag(float time, float amplitude)
    {
        var zigzagValue = Mathf.Sin((time * _zigzagFrequency + PhaseOffset) * 2f * Mathf.PI);
        return _upDirection * (zigzagValue * amplitude);
    }

    private Vector3 CalculateSpiralPattern(float time, float amplitude)
    {
        var angle = (time * _zigzagFrequency + PhaseOffset) * 2f * Mathf.PI;
        var spiralRadius = amplitude * (1f - time / LifeTime);

        var horizontalOffset = _rightDirection * (Mathf.Cos(angle) * spiralRadius);
        var verticalOffset = _upDirection * (Mathf.Sin(angle) * spiralRadius);

        return horizontalOffset + verticalOffset;
    }

    private Vector3 CalculateFigure8Pattern(float time, float amplitude)
    {
        var angle = (time * _zigzagFrequency + PhaseOffset) * 2f * Mathf.PI;

        var horizontalOffset = _rightDirection * (Mathf.Sin(angle) * amplitude);
        var verticalOffset = _upDirection * (Mathf.Sin(2f * angle) * amplitude * 0.5f);

        return horizontalOffset + verticalOffset;
    }

    private Vector3 CalculateRandomPattern(float time, float amplitude)
    {
        var seed = PhaseOffset + time * _zigzagFrequency;

        var horizontalNoise = Mathf.PerlinNoise(seed, 0f) * 2f - 1f;
        var verticalNoise = Mathf.PerlinNoise(0f, seed) * 2f - 1f;

        var horizontalOffset = _rightDirection * (horizontalNoise * amplitude);
        var verticalOffset = _upDirection * (verticalNoise * amplitude);

        return horizontalOffset + verticalOffset;
    }

    public void SetZigZagParameters(float amplitude, float frequency, ZigZagPattern pattern)
    {
        _zigzagAmplitude = amplitude;
        _zigzagFrequency = frequency;
        _pattern = pattern;
    }
}
﻿using Examples.Tank;
using Helpers;
using Netick.Unity;
using Tank.Scripts.Utility;
using UnityEngine;

public struct ShotState : ISparseState<Shot>
{
    /// <summary>
    /// Generic sparse state properties required by the interface
    /// </summary>
    public int StartTick { get; set; }

    public int EndTick { get; set; }

    /// <summary>
    /// Shot specific sparse properties
    /// </summary>
    public Vector3 Position;

    public Vector3 Direction;

    public ShotState(Vector3 startPosition, Vector3 direction)
    {
        StartTick = 0;
        EndTick = 0;
        Position = startPosition;
        Direction = direction;
    }

    public void Extrapolate(float t, Shot prefab)
    {
        Position = GetPositionAt(t, prefab);
        Direction = GetDirectionAt(t, prefab);
    }

    public Vector3 GetTargetPosition(Shot prefab)
    {
        var a = 0.5f * prefab.Gravity.y;
        var b = prefab.Speed * Direction.y;
        var c = Position.y;
        var d = b * b - 4 * a * c;
        var t = (-b - Mathf.Sqrt(d)) / (2 * a);
        var p = GetPositionAt(t, prefab);
        p.y = 0.05f; // Return the position with a slight y offset to avoid placing target where it will end up z-fighting with the ground;
        return p;
    }

    private Vector3 GetPositionAt(float t, Shot prefab) =>
        Position + t * (prefab.Speed * Direction + 0.5f * t * prefab.Gravity);

    private Vector3 GetDirectionAt(float t, Shot prefab) =>
        prefab.Speed == 0 ? Direction : (prefab.Speed * Direction + t * prefab.Gravity).normalized;
}

public class Shot : MonoBehaviour, ISparseVisual<ShotState, Shot>
{
    [Header("Settings")] [SerializeField] private LayerMask _hitMask;
    [SerializeField] private bool _serverVisible;

    [SerializeField] private Vector3 _gravity;
    [SerializeField] private float _speed;
    [SerializeField] private float _radius;
    [SerializeField] private float _range;
    [SerializeField] private float _areaRadius;
    [SerializeField] private float _areaImpulse;
    [SerializeField] private byte _areaDamage;
    [SerializeField] private float _timeToLive;
    [SerializeField] private bool _isHitScan;
    [SerializeField] private ExplosionFX _detonationPrefab;
    [SerializeField] private MuzzleFlash _muzzleFlash;

    public Vector3 Gravity => _gravity;
    public float Speed => _speed;
    public float Radius => _radius;
    public LayerMask HitMask => _hitMask;
    public float Range => _range;
    public float AreaRadius => _areaRadius;
    public float AreaImpulse => _areaImpulse;
    public byte AreaDamage => _areaDamage;
    public float TimeToLive => _timeToLive;
    public bool IsHitScan => _isHitScan;

    private Transform _xForm;
    private ISparseVisual<ShotState, Shot> _sparseVisualImplementation;

    private void Awake() => _xForm = transform;

    public bool IsServerVisible => _serverVisible;

    public void ApplyStateToVisual(NetworkBehaviour owner, ShotState state, float t, bool isFirstRender,
        bool isLastRender)
    {
        if (isLastRender)
        {
            // Slightly hacky, but we never move the hitScan so its current position is always the muzzle, and target is start + direction
            if (IsHitScan)
                LocalObjectPool.Acquire(_detonationPrefab, state.Position + state.Direction, Quaternion.identity);
            else
                LocalObjectPool.Acquire(_detonationPrefab, state.Position, Quaternion.identity);
        }

        if (isFirstRender && _muzzleFlash)
            LocalObjectPool.Acquire(_muzzleFlash, state.Position, Quaternion.LookRotation(state.Direction),
                owner.transform);
        if(owner.IsClient)
            Debug.DrawLine(state.Position, state.Position + Vector3.up * 2, Color.red, 4);
        _xForm.forward = state.Direction;
        _xForm.position = state.Position;
    }
}
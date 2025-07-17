using System;
using System.Collections;
using System.Collections.Generic;
using Netick;
using Netick.Unity;
using UnityEngine;

public class PowerUpSpawner : NetworkBehaviour
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private MeshRenderer _rechargeCircle;

    [Header("Colors")] [SerializeField] private Color _mainPowerupColor;
    [SerializeField] private Color _specialPowerupColor;
    [SerializeField] private Color _buffPowerupColor;

    public struct NetworkState
    {
        public int lastTick;
        public int activePowerupIndex;
    }

    const float RESPAWN_TIME = 3f;

    private static readonly int Recharge = Shader.PropertyToID("_Recharge");

    [Networked] public NetworkState State { get; set; }

    private Dictionary<PowerupType, PowerupElement> _powerups;
    private PowerupElement[] _powerupsArray;
    
    
    public override void NetworkStart()
    {
        base.NetworkStart();
        if (Sandbox.IsServer)
            SetNextPowerup();
        ChangePowerVisual();
    }

    public override void NetworkRender()
    {
        base.NetworkRender();
        
        float progress = 0;
        
        var deltaTime = Sandbox.TickToTime(Sandbox.Tick - State.lastTick);
        if (State.lastTick > 0 && deltaTime < RESPAWN_TIME)
        {
            progress = 1.0f - deltaTime / RESPAWN_TIME;
            _renderer.transform.localScale = Vector3.Lerp(_renderer.transform.localScale, Vector3.zero, Time.deltaTime * 5f);
        }
        else
            _renderer.transform.localScale = Vector3.Lerp(_renderer.transform.localScale, Vector3.one, Time.deltaTime * 5f);
        _rechargeCircle.material.SetFloat(Recharge, progress);
    }
    
    private void OnTriggerStay(Collider other)
    {
        if(Sandbox.Tick - State.lastTick < Sandbox.TimeToTick(RESPAWN_TIME))
            return;
        
        other.TryGetComponent<IWeaponManager>(out var weaponManager);
        if(weaponManager == null)
        {
            Debug.LogWarning("PowerUpSpawner: No IWeaponManager found on the collided object.");
            return;
        }
        
        Sandbox.Log("PowerUpSpawner: Player picked up powerup." + (PowerupType)State.activePowerupIndex);
        weaponManager.SetPowerupType((PowerupType)State.activePowerupIndex);
        
        SetNextPowerup();
    }

    [OnChanged(nameof(State))]
    private void UpdateVisual(OnChangedData data) => ChangePowerVisual();

    private void SetNextPowerup()
    {
        if (!IsServer) return;
        if (_powerups == null || _powerups.Count == 0)
            LoadPowerup();

        NetworkState initState = new NetworkState
        {
            lastTick = Sandbox.Tick,
            activePowerupIndex = (int)_powerupsArray[UnityEngine.Random.Range(0, _powerupsArray.Length)].powerupType
        };
    
        State = initState;
    }

    private void LoadPowerup()
    {
        _powerupsArray = Resources.LoadAll<PowerupElement>("Powerups");
        _powerups = new Dictionary<PowerupType, PowerupElement>();

        foreach (var powerup in _powerupsArray)
        {
            Sandbox.Log(powerup.name);
            if (!_powerups.ContainsKey(powerup.powerupType))
                _powerups.Add(powerup.powerupType, powerup);
            else
                Debug.LogWarning($"Powerup with ID {powerup.powerupType} already exists. Skipping duplicate.");
        }
    }

    private PowerupElement GetPowerup(int index)
    {
        if (_powerups == null || _powerups.Count == 0)
            LoadPowerup();

        return _powerups[(PowerupType)index];
    }


    private void ChangePowerVisual()
    {
        PowerupElement powerup = GetPowerup(State.activePowerupIndex);
        if(_rechargeCircle == null || powerup == null || _renderer == null || _meshFilter == null)
        {
            Debug.LogError("Powerup visuals are not set up correctly.");
            return;
        }

        _renderer.transform.localScale = Vector3.zero;
        UpdateVisuals(powerup);
    }

    private void UpdateVisuals(PowerupElement powerup)
    {
        _meshFilter.mesh = powerup.powerupSpawnerMesh;
        _rechargeCircle.material.color = GetPowerupColor(powerup.weaponInstallationType);
    }

    private Color GetPowerupColor(WeaponInstallationType weaponType)
    {
        switch (weaponType)
        {
            default:
            case WeaponInstallationType.PRIMARY: return _mainPowerupColor;
            case WeaponInstallationType.SECONDARY: return _specialPowerupColor;
            case WeaponInstallationType.BUFF: return _buffPowerupColor;
        }
    }
}
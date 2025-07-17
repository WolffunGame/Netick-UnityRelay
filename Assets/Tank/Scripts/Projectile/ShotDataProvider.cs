using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

public class ShotDataProvider
{
    private readonly ShotConfig _config;
    private readonly Dictionary<(PowerupType, ShotMovementType), ShotData> _shotDataCache;

    // Constructor injection - Reflex will inject ShotConfig here
    public ShotDataProvider(ShotConfig config)
    {
        _config = config;
        _shotDataCache = new Dictionary<(PowerupType, ShotMovementType), ShotData>();
        InitializeCache();
    }

    public ShotData GetShotData(PowerupType type, ShotMovementType movementType)
    {
        var key = (type, movementType);
        return _shotDataCache.TryGetValue(key, out var data) ? data : null;
    }

    public List<ShotData> GetShotDataByType(PowerupType type)
    {
        var result = new List<ShotData>();
        foreach (var kvp in _shotDataCache)
        {
            if (kvp.Key.Item1 == type)
            {
                result.Add(kvp.Value);
            }
        }
        return result;
    }

    public List<ShotData> GetAllShotData()
    {
        return new List<ShotData>(_shotDataCache.Values);
    }

    public bool HasShotData(PowerupType type, ShotMovementType movementType)
    {
        var key = (type, movementType);
        return _shotDataCache.ContainsKey(key);
    }

    private void InitializeCache()
    {
        if (_config?.ProjectileDatas != null)
        {
            _shotDataCache.Clear();
            foreach (var shotData in _config.ProjectileDatas)
            {
                var key = (shotData.Type, shotData.MovementType);
                _shotDataCache[key] = shotData;
            }
            
            Debug.Log($"ShotDataProvider: Cached {_shotDataCache.Count} shots");
        }
        else
        {
            Debug.LogError("ShotDataProvider: ShotConfig is null or ProjectileDatas is null!");
        }
    }
}
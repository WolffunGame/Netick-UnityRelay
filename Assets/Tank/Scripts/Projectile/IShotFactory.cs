using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShotFactory
{
    /// <summary>
    /// Creates a shot state for networked projectiles
    /// </summary>
    ShotState CreateShotState(PowerupType type, ShotMovementType movementType, Vector3 position, Vector3 direction, int startTick);
    
    /// <summary>
    /// Creates a shot state with custom parameters
    /// </summary>
    ShotState CreateShotState(ShotData shotData, Vector3 position, Vector3 direction, int startTick);
    
    /// <summary>
    /// Gets shot data for specific type and movement
    /// </summary>
    ShotData GetShotData(PowerupType type, ShotMovementType movementType);
    
    /// <summary>
    /// Spawns a physical shot GameObject (if needed for local effects)
    /// </summary>
    Shot SpawnShot(PowerupType type, ShotMovementType movementType, Vector3 position, Vector3 direction);
}
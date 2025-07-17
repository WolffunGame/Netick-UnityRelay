using Netick;
using UnityEngine;

public class ShotFactory : IShotFactory
{
    private readonly ShotDataProvider _shotDataProvider;
    private readonly ShotConfig _shotConfig;

    // Constructor injection
    public ShotFactory(ShotDataProvider shotDataProvider, ShotConfig shotConfig)
    {
        _shotDataProvider = shotDataProvider;
        _shotConfig = shotConfig;
    }

    public ShotState CreateShotState(PowerupType type, ShotMovementType movementType, Vector3 position,
        Vector3 direction, int startTick)
    {
        var shotData = _shotDataProvider.GetShotData(type, movementType);
        if (shotData == null)
        {
            Debug.LogWarning($"ShotFactory: No shot data found for {type} with movement {movementType}");
            return CreateDefaultShotState(position, direction, startTick);
        }

        return CreateShotState(shotData, position, direction, startTick);
    }

    public ShotState CreateShotState(ShotData shotData, Vector3 position, Vector3 direction, int startTick)
    {
        var endTick = startTick + Mathf.RoundToInt(shotData.LifeTime / Time.fixedDeltaTime);

        var shotState = new ShotState
        {
            Position = position,
            Direction = direction.normalized,
            StartTick = startTick,
            EndTick = endTick,
            Speed = shotData.Speed,
            Damage = shotData.Damage,
            ProjectileType = shotData.Type,
            MovementType = shotData.MovementType,
        };

        // Apply movement-specific data to ShotState
        ApplyMovementData(ref shotState, shotData);

        return shotState;
    }

    private void ApplyMovementData(ref ShotState shotState, ShotData shotData)
    {
        var movementData = shotData.MovementData;

        switch (shotData.MovementType)
        {
            case ShotMovementType.Parabolic:
                // Store gravity data in TBytes for parabolic movement
                StoreParabolicData(ref shotState, movementData);
                break;

            case ShotMovementType.ZigZag:
                // Store zigzag data in TBytes for zigzag movement
                StoreZigZagData(ref shotState, movementData);
                break;

            case ShotMovementType.Linear:
                // Linear doesn't need extra data
                break;
        }
    }

    private void StoreParabolicData(ref ShotState shotState, ShotMovementData movementData)
    {
        // Pack parabolic data into TBytes
        var bytes = new byte[32];
        var gravity = movementData.Gravity;

        // Pack gravity vector (12 bytes)
        System.BitConverter.GetBytes(gravity.x).CopyTo(bytes, 0);
        System.BitConverter.GetBytes(gravity.y).CopyTo(bytes, 4);
        System.BitConverter.GetBytes(gravity.z).CopyTo(bytes, 8);

        // Set the bytes
        shotState.TBytes = new NetworkArrayStruct32<byte>();
        for (int i = 0; i < 32; i++)
        {
            var tBytes = shotState.TBytes;
            tBytes[i] = bytes[i];
            shotState.TBytes = tBytes;
        }
    }

    private void StoreZigZagData(ref ShotState shotState, ShotMovementData movementData)
    {
        // Pack zigzag data into TBytes
        var bytes = new byte[32];

        // Pack amplitude (4 bytes)
        System.BitConverter.GetBytes(movementData.ZigZagAmplitude).CopyTo(bytes, 0);

        // Pack frequency (4 bytes)
        System.BitConverter.GetBytes(movementData.ZigZagFrequency).CopyTo(bytes, 4);

        // Set the bytes
        shotState.TBytes = new NetworkArrayStruct32<byte>();
        for (int i = 0; i < 32; i++)
        {
            var tBytes = shotState.TBytes;
            tBytes[i] = bytes[i];
            shotState.TBytes = tBytes;
        }
    }

    public ShotData GetShotData(PowerupType type, ShotMovementType movementType)
    {
        return _shotDataProvider.GetShotData(type, movementType);
    }

    public Shot SpawnShot(PowerupType type, ShotMovementType movementType, Vector3 position, Vector3 direction)
    {
        var shotData = _shotDataProvider.GetShotData(type, movementType);
        if (shotData == null)
        {
            Debug.LogWarning($"ShotFactory: No shot data found for {type} with movement {movementType}");
            return null;
        }

        // This would need a shot prefab registry or similar system
        // For now, return null as we're focusing on state-based shots
        Debug.LogWarning("ShotFactory: Physical shot spawning not implemented yet");
        return null;
    }

    private ShotState CreateDefaultShotState(Vector3 position, Vector3 direction, int startTick)
    {
        return new ShotState
        {
            Position = position,
            Direction = direction.normalized,
            StartTick = startTick,
            EndTick = startTick + 100, // Default 100 ticks
            Speed = 20f,
            Damage = 25f,
            ProjectileType = PowerupType.DEFAULT,
            MovementType = ShotMovementType.Linear
        };
    }
}
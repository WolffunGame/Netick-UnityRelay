using Netick;
using Netick.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tank.Scripts
{
    public class Spawner : NetworkEventsListener
    {
        [SerializeField] private GameObject _playerPrefab;
        public override void OnClientConnected(NetworkSandbox sandbox, NetworkConnection client)
        {
            Debug.LogError( $"Client {client.Id} connected to the server. Spawning player...");
            var position = Random.insideUnitCircle * 4;
            var player = sandbox.NetworkInstantiate(_playerPrefab, position, Quaternion.identity, client);
            if (player.TryGetComponent(out Tank tank))
                tank.TankIndex = (byte)Sandbox.Players.Count;
        }
    }
}
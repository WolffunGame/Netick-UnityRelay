using Netick;
using Netick.Unity;
using UnityEngine;

namespace Tank.Scripts
{
    public class Spawner : NetworkEventsListener
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform[] _spawnPositions = new Transform[4];
        
        public override void OnClientConnected(NetworkSandbox sandbox, NetworkConnection client)
        {
            var player = sandbox.NetworkInstantiate(_playerPrefab, _spawnPositions[Sandbox.ConnectedClients.Count].position, Quaternion.identity, client);
            client.PlayerObject = player.gameObject;
            if (player.TryGetComponent(out Tank tank))
                tank.TankIndex = (byte) Sandbox.ConnectedClients.Count;
        }
    }
}
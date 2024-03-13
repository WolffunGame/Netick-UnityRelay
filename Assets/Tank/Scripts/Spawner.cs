using Netick;
using Netick.Unity;
using Tank.Scripts.Utility;
using UnityEngine;

namespace Tank.Scripts
{
    public class Spawner : NetworkEventsListener
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform[] _spawnPositions = new Transform[4];
        
        public override void OnClientConnected(NetworkSandbox sandbox, NetworkConnection client)
        {
            var position = Random.insideUnitCircle * 4;
            var player = sandbox.NetworkInstantiate(_playerPrefab,position.XOY() , Quaternion.identity, client);
            client.PlayerObject = player.gameObject;
            if (player.TryGetComponent(out Tank tank))
                tank.TankIndex = (byte) Sandbox.ConnectedClients.Count;
        }
    }
}
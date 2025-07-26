using Netick.Unity;
using UnityEngine;
using NetworkPlayer = Netick.NetworkPlayer;
using Random = UnityEngine.Random;

namespace Tank.Scripts
{
    public class Spawner : NetworkEventsListener
    {
        [SerializeField] private GameObject _playerPrefab;
        public override void OnPlayerConnected(NetworkSandbox sandbox, NetworkPlayer player)
        {
            var position = Random.insideUnitCircle * 4;
            sandbox.NetworkInstantiate(_playerPrefab, position, Quaternion.identity, player);
        }
    }
}
using Cysharp.Threading.Tasks;
using UnityEngine;
using Netick.Unity;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Network = Netick.Unity.Network;

namespace Netick.Samples
{
    public class RelayNetworkLauncher : NetworkEventsListener
    {
        public int MaxPlayer = 4;
        private string _playerId;
        public StringSo JoinCode;
        public BoolSo IsHostReady;
        public GameObject SandboxPrefab;
        public UnityTransportProvider Transport;
        [Header("Network")] [Range(0, 65535)] public int Port = 34567;

        private async void Start() => await Initialize();

        private async UniTask Initialize()
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsAuthorized)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            _playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log($" Authentication ID: {_playerId}");
        }

        public async void StartHost()
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayer);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            JoinCode.SetValue(joinCode);
            Transport.SetAllocation(allocation);
            var result = Network.Launch(StartMode.MultiplePeers, new LaunchData()
            {
                Port = Port,
                SandboxPrefab = SandboxPrefab,
                TransportProvider = Transport,
                NumberOfServers = 1,
                NumberOfClients = 1,
                RunServersAsHosts = true
            });
            await UniTask.WaitUntil(() => IsHostReady.Value);
            result.Clients[0].Connect(default, default);
        }

        public void JoinHost()
        {
            var sandbox = Network.StartAsClient(Transport, Port, SandboxPrefab);
            sandbox.Connect(default, default);
        }
    }
}
using Cysharp.Threading.Tasks;
using UnityEngine;
using Netick.Unity;
using Unity.Services.Relay;
using Network = Netick.Unity.Network;

namespace Netick.Samples
{
    public class NetworkLauncher : NetworkEventsListener
    {
        public int MaxPlayer = 4;
        public StringSo JoinCode;
        public BoolSo IsHostReady;
        public GameObject SandboxPrefab;
        public UnityTransportProvider Transport;
        [Header("Network")] [Range(0, 65535)] public int Port = 34567;
        public string IP;
        public async void StartHost()
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayer);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            JoinCode.SetValue(joinCode);
            Debug.LogError(JoinCode.Value);
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
            result.Clients[0].Connect(Port, IP);
        }
        
        public async void JoinHost()
        {
            var sandbox = Network.StartAsClient(Transport, Port, SandboxPrefab);
            sandbox.Connect(Port, IP);
        }
    }
}
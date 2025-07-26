using UnityEngine;
using Network = Netick.Unity.Network;

public class SinglePlayAutoStart: MonoBehaviour
{
    [SerializeField] private string _serverIP = "127.0.0.1";
    [SerializeField] private int _serverPort = 7777;
    [SerializeField] private GameObject _sandboxPrefab;

    private void Start()
    {
        if (Network.Instance != null && Network.IsRunning)
            return;
        Network.StartAsSinglePlayer(_sandboxPrefab);
    }
}
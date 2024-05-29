using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class NetworkManager:MonoBehaviour
{
    private string _playerId;
    private async void Start() => await Initialize();
    private async UniTask Initialize()
    {
        await UnityServices.InitializeAsync(); 
        if(!AuthenticationService.Instance.IsAuthorized)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        _playerId = AuthenticationService.Instance.PlayerId;
        Debug.Log( $" Authentication ID: {_playerId}");
    }
}
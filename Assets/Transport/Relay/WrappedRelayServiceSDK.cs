using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class WrappedRelayServiceSDK : IRelayServiceSDK
{
    public Task<Allocation> CreateAllocationAsync(int maxConnections, string region = null)
        => Relay.Instance.CreateAllocationAsync(maxConnections, region);

    public Task<string> GetJoinCodeAsync(Guid allocationId) => Relay.Instance.GetJoinCodeAsync(allocationId);

    public Task<JoinAllocation> JoinAllocationAsync(string joinCode) => Relay.Instance.JoinAllocationAsync(joinCode);

    public Task<List<Region>> ListRegionsAsync() => Relay.Instance.ListRegionsAsync();


    public async void AllocationFromJoinCode(string joinCode, Action<JoinAllocation> onSuccess, Action onFailure)
    {
        var joinAllocation = JoinAllocationAsync(joinCode);
        await joinAllocation;
        if (joinAllocation.IsFaulted)
        {
            joinAllocation.Exception?.Flatten().Handle(err =>
            {
                Debug.LogError(
                    $"Unable to get Relay allocation from join code, encountered an error: {err.Message}.");
                return true;
            });
            onFailure?.Invoke();
        }
        onSuccess?.Invoke(joinAllocation.Result);
    }
}
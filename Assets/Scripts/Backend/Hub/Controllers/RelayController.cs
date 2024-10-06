using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataStorage;
using Managers;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Backend.Hub.Controllers
{
    public class RelayController : MonoBehaviour
    {
        public static RelayController Instance { get; private set; }

        private Allocation _createdAllocation;
        private RelayServerData _relayServerData;

        private string _joinCode;

        public void Initialize()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }


        public async Task<string> GetRelayCode(int maxClientsNum)
        {
            var isAnyAllocationFound = await AssignAllocation("europe-central2");
            if (!isAnyAllocationFound) // If default region has failed
            {
                var (europeRegions, regionsWithoutEurope) = await GetEuropeAndTheRestRegions();
                if (europeRegions == null || regionsWithoutEurope == null) return ErrorCodes.JoinRelayErrorCode;
                foreach (var region in europeRegions)
                {
                    isAnyAllocationFound = await AssignAllocation(region);
                    if (isAnyAllocationFound) break;
                }

                if (!isAnyAllocationFound) // If europe regions have failed
                {
                    foreach (var region in regionsWithoutEurope)
                    {
                        isAnyAllocationFound = await AssignAllocation(region);
                        if (isAnyAllocationFound) break;
                    }
                }
            }

            if (!isAnyAllocationFound) return ErrorCodes.JoinRelayErrorCode; // If all allocations have failed
            try
            {
                _joinCode = await RelayService.Instance.GetJoinCodeAsync(_createdAllocation.AllocationId);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return ErrorCodes.JoinRelayErrorCode;
            }

            return _joinCode;

            async Task<bool> AssignAllocation(string region)
            {
                try
                {
                    _createdAllocation =
                        await RelayService.Instance.CreateAllocationAsync(maxClientsNum, region);
                    _relayServerData = new RelayServerData(_createdAllocation, "wss");
                    Debug.Log($"Successfully created allocation at {region}.");
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(
                        $"Cannot create allocation to the {region} region. Trying another region.\nError: {e}");
                    return false;
                }
            }

            async Task<Tuple<List<string>, List<string>>> GetEuropeAndTheRestRegions()
            {
                var regions = await GetAvailableRegions();
                if (regions == null)
                {
                    return new Tuple<List<string>, List<string>>(null, null);
                }

                List<string> europeRegions = new();
                List<string> regionsWithoutEurope = new();
                foreach (var region in regions)
                {
                    if (region.Id.ToLower().Contains("europe"))
                    {
                        europeRegions.Add(region.Id);
                    }
                    else
                    {
                        regionsWithoutEurope.Add(region.Id);
                    }
                }

                return new Tuple<List<string>, List<string>>(europeRegions, regionsWithoutEurope);
            }

            async Task<List<Region>> GetAvailableRegions()
            {
                List<Region> regions = null;
                try
                {
                    regions = await RelayService.Instance.ListRegionsAsync();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Cannot find any regions.\nError: {e}");
                }

                return regions;
            }
        }

        public bool CreateRelay()
        {
            try
            {
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(_relayServerData);
                return NetworkCommunicationManager.StartHost();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return false;
            }
        }

        public static async Task<bool> JoinRelay(string joinCode)
        {
            for (var i = 0; i < 5; i++)
            {
                try
                {
                    var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                    var relayServerData = new RelayServerData(joinAllocation, "wss");
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                    return NetworkCommunicationManager.StartClient();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error during joining. Retrying.\nError: {e}");
                }
            }

            return false;
        }
    }
}
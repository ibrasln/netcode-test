using System;
using NetcodeTest.Networking.Client;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace NetcodeTest.UI
{
    public class LobbiesList : MonoBehaviour
    {
        [SerializeField] private Transform lobbyItemParent;
        [SerializeField] private LobbyItem lobbyItemPrefab;
        
        private bool _isJoining;
        private bool _isRefreshing;

        private void OnEnable()
        {
            RefreshList();
        }

        public async void RefreshList()
        {
            if (_isRefreshing) return; 
                
            _isRefreshing = true;

            try
            {
                QueryLobbiesOptions options = new()
                {
                    Count = 25,
                    Filters = new()
                    {
                        new QueryFilter(field: QueryFilter.FieldOptions.AvailableSlots, op: QueryFilter.OpOptions.GT, value: "0"),
                        new QueryFilter(field: QueryFilter.FieldOptions.IsLocked, op: QueryFilter.OpOptions.EQ, value: "0")
                    }
                };

                QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

                foreach (Transform child in lobbyItemParent)
                {
                    Destroy(child.gameObject);
                }
                
                foreach (Lobby lobby in lobbies.Results)
                {
                    LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                    lobbyItem.Initialize(this, lobby);
                }
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError(ex);
            }
            
            _isRefreshing = false;
        }
        
        public async void JoinAsync(Lobby lobby)
        {
            if (_isJoining) return; 
                
            _isJoining = true;
            
            try
            {
                Lobby joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
                string joinCode = joiningLobby.Data["JoinCode"].Value;

                await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError(ex);
            }

            _isJoining = false;
        }
    }
}
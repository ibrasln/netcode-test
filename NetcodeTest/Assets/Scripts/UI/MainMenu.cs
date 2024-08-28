using System;
using NetcodeTest.Networking.Client;
using NetcodeTest.Networking.Host;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace NetcodeTest.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private TMP_InputField joinCodeField;
        [SerializeField] private TMP_Text queueStatusText;
        [SerializeField] private TMP_Text queueTimerText;
        [SerializeField] private TMP_Text findMatchButtonText;

        private bool _isMatchmaking;
        private bool _isCancelling;
        private bool _isBusy;
        private float _timeInQueue;
        
        private void Start()
        {
            if (ClientSingleton.Instance == null) return;
            
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            
            queueStatusText.text = string.Empty;
            queueTimerText.text = string.Empty;
        }

        private void Update()
        {
            if (_isMatchmaking)
            {
                _timeInQueue += Time.deltaTime;
                TimeSpan ts = TimeSpan.FromSeconds(_timeInQueue);

                queueTimerText.text = $"{ts.Minutes:00}:{ts.Seconds:00}";
            }
        }

        public async void FindMatch()
        {
            if (_isCancelling) return;
            
            if (_isMatchmaking)
            {
                queueStatusText.text = "Cancelling...";
                _isCancelling = true;

                await ClientSingleton.Instance.GameManager.CancelMatchmaking();
                
                _isCancelling = false;
                _isMatchmaking = false;
                _isBusy = false;
                findMatchButtonText.text = "Find Match";
                queueStatusText.text = string.Empty;
                queueTimerText.text = string.Empty;
                return;
            }

            if (_isBusy) return;
            
            ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade);
                
            findMatchButtonText.text = "Cancel";
            queueStatusText.text = "Searching...";
            _timeInQueue = 0;
            _isMatchmaking = true;
            _isBusy = true;
        }

        private void OnMatchMade(MatchmakerPollingResult result)
        {
            switch (result)
            {
                case MatchmakerPollingResult.Success:
                    queueStatusText.text = "Connecting...";
                    break;
                
                case MatchmakerPollingResult.TicketCreationError:
                    queueStatusText.text = "TicketCreationError";
                    break;
                
                case MatchmakerPollingResult.TicketCancellationError:
                    queueStatusText.text = "TicketCancellationError";
                    break;
                
                case MatchmakerPollingResult.TicketRetrievalError:
                    queueStatusText.text = "TicketRetrievalError";
                    break;
                
                case MatchmakerPollingResult.MatchAssignmentError:
                    queueStatusText.text = "MatchAssignmentError";
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }
        
        public async void StartHost()
        {
            if (_isBusy) return;
            
            _isBusy = true;
            
            await HostSingleton.Instance.GameManager.StartHostAsync();

            _isBusy = false;
        }

        public async void StartClient()
        {
            if (_isBusy) return;
            
            _isBusy = true;
            
            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);

            _isBusy = false;
        }
        
        public async void JoinAsync(Lobby lobby)
        {
            if (_isBusy) return; 
                
            _isBusy = true;
            
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

            _isBusy = false;
        }
    }
}
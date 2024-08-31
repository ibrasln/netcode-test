using NetcodeTest.Networking.Client;
using NetcodeTest.Networking.Host;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.UI
{
    public class GameHUD : NetworkBehaviour
    {
        [SerializeField] private TMP_Text lobbyCodeText;

        private NetworkVariable<FixedString32Bytes> _lobbyCode = new("");
        
        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                _lobbyCode.OnValueChanged += HandleLobbyCodeChanged;
                HandleLobbyCodeChanged("", _lobbyCode.Value);
            }
            
            if (!IsHost) return;
            
            _lobbyCode.Value = HostSingleton.Instance.GameManager.JoinCode;
        }
        
        public override void OnNetworkDespawn()
        {
            if (IsClient) _lobbyCode.OnValueChanged -= HandleLobbyCodeChanged;
        }

        public void LeaveGame()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                HostSingleton.Instance.GameManager.Shutdown();
            }

            ClientSingleton.Instance.GameManager.Disconnect();
        }
        
        private void HandleLobbyCodeChanged(FixedString32Bytes oldCode, FixedString32Bytes newCode)
        {
            lobbyCodeText.text = newCode.ToString();
        }
    }
}

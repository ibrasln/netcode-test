using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.UI.Leaderboard
{
    public class LeaderboardEntityDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text displayText;
        [SerializeField] private Color myColor;

        public ulong ClientId { get; private set; }
        private FixedString32Bytes _playerName;
        public int Coins { get; private set; }
        
        public void Initialize(ulong clientId, FixedString32Bytes playerName, int coins)
        {
            ClientId = clientId;
            _playerName = playerName.Value;

            if (clientId == NetworkManager.Singleton.LocalClientId) displayText.color = myColor;
            
            UpdateCoins(coins);
        }

        public void UpdateCoins(int coins)
        {
            Coins = coins;
            
            UpdateText();
        }
        
        public void UpdateText() => displayText.text = $"{transform.GetSiblingIndex() + 1}. {_playerName} ({Coins})"; 
    }
}
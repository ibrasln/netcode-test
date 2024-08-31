using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.UI.Leaderboard
{
    public class LeaderboardEntityDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text displayText;

        public ulong ClientId { get; private set; }
        public int Coins { get; private set; }
        public int TeamIndex { get; private set; }
        
        private FixedString32Bytes _displayName;
        
        public void Initialize(ulong clientId, FixedString32Bytes displayName, int coins)
        {
            ClientId = clientId;
            _displayName = displayName.Value;
            
            UpdateCoins(coins);
        }
        
        public void Initialize(int teamIndex, FixedString32Bytes displayName, int coins)
        {
            TeamIndex = teamIndex;
            _displayName = displayName.Value;
            
            UpdateCoins(coins);
        }

        public void SetColor(Color color) => displayText.color = color;
        
        public void UpdateCoins(int coins)
        {
            Coins = coins;
            
            UpdateText();
        }
        
        public void UpdateText() => displayText.text = $"{transform.GetSiblingIndex() + 1}. {_displayName} ({Coins})"; 
    }
}
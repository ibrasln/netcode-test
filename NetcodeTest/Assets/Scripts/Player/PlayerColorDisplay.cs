using System;
using Unity.Collections;
using UnityEngine;

namespace NetcodeTest.Player
{
    public class PlayerColorDisplay : MonoBehaviour
    {
        [SerializeField] private TankPlayer player;
        [SerializeField] private TeamColorLookup teamColorLookup;
        [SerializeField] private SpriteRenderer[] tankPartsSpriteRenderers;

        private void Start()
        {
            HandleTeamChanged(-1, player.TeamIndex.Value);
            
            player.TeamIndex.OnValueChanged += HandleTeamChanged;
        }
        
        private void OnDestroy()
        {
            player.TeamIndex.OnValueChanged -= HandleTeamChanged;
        }
        
        private void HandleTeamChanged(int oldTeamIndex, int newTeamIndex)
        {
            Color teamColor = teamColorLookup.GetTeamColor(player.TeamIndex.Value);
            
            foreach (SpriteRenderer spriteRenderer in tankPartsSpriteRenderers)
            {
                spriteRenderer.color = teamColor;
            }
        }
    }
}
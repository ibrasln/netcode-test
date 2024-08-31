using UnityEngine;

namespace NetcodeTest.Player
{
    [CreateAssetMenu(fileName = "NewTeamColorLookup", menuName = "Team Color Lookup")]
    public class TeamColorLookup : ScriptableObject
    {
        [SerializeField] private Color[] teamColors;

        public Color GetTeamColor(int teamIndex)
        {
            if (teamIndex < 0 || teamIndex >= teamColors.Length) return Random.ColorHSV(0f, 1f, 1f, 1f, .5f, 1f);
            
            return teamColors[teamIndex];
        }
    }
}
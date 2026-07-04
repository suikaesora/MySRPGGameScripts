using UnityEngine;

namespace MySRPGGame.Core
{
    /// <summary>
    /// ŒR‘àƒf[ƒ^
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObject/Troop")]
    public class TroopData : ScriptableObject
    {
        [SerializeField]
        private string troopName;

        [SerializeField]
        private Color troopColor = Color.white;

        [SerializeField]
        private bool isPlayer;

        public string TroopName => troopName;

        public Color TroopColor => troopColor;

        public bool IsPlayer => isPlayer;
    }
}

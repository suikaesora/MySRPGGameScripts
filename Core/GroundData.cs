using UnityEngine;

namespace MySRPGGame.Core
{
    /// <summary>
    /// 地形データ
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObject/Ground")]
    public class GroundData : ScriptableObject
    {
        [SerializeField]
        private string groundID;

        [SerializeField]
        private Ground groundPrefab;

        [SerializeField]
        private bool canPass;

        [SerializeField]
        private int weight;

        public string GroundId => groundID;

        public Ground GroundPrefab => groundPrefab;

        /// <summary>
        /// 通行可能かどうか
        /// </summary>
        public bool CanPass => canPass;

        /// <summary>
        /// 地形の重み
        /// </summary>
        public int Weight => weight;
    }
}

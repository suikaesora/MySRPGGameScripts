using UnityEngine;

namespace MySRPGGame.Core
{
    /// <summary>
    /// 地形クラス
    /// </summary>
    public class Ground : MonoBehaviour
    {
        /// <summary>
        /// 通行可能かどうか
        /// </summary>
        public bool CanPass { get; private set; }

        /// <summary>
        /// 地形の重み
        /// </summary>
        public int Weight { get; private set; }

        public void Init(GroundData groundData)
        {
            CanPass = groundData.CanPass;
            Weight = groundData.Weight;
        }
    }
}

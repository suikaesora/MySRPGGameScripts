using MySRPGGame.Core;
using MySRPGGame.Grid;
using UnityEngine;

namespace MySRPGGame.Core
{
    /// <summary>
    /// グリッド上の一地点を示すカーソル表示
    /// </summary>
    public class GridCursor : MonoBehaviour
    {
        /// <summary>
        /// グリッド座標
        /// </summary>
        public Vector2Int GridPosition { get; private set; }

        /// <summary>
        /// ある位置にカーソルを移動
        /// </summary>
        /// <param name="gridPosition">グリッド座標</param>
        public void SetPosition(GridWorldConverter<Spot> gwConverter, Vector2Int gridPosition)
        {
            GridPosition = gridPosition;
            Vector2 worldPos = gwConverter.GetGridToWorldPosition(gridPosition);
            transform.position = new Vector3(worldPos.x, 0f, worldPos.y);
        }
    }
}

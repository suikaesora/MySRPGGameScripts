using UnityEngine;

namespace MySRPGGame.Grid
{
    /// <summary>
    /// グリッドをワールド上の実体として扱いやすくします。
    /// </summary>
    /// <typeparam name="TCellType">グリッドのセルの型です。</typeparam>
    public class GridWorldConverter<TCellType>
    {
        public Grid<TCellType> GridRef { get; private set; }
        public float CellSize { get; private set; }

        public GridWorldConverter(Grid<TCellType> gridRef, float cellSize)
        {
            GridRef = gridRef;
            CellSize = cellSize;
        }

        /// <summary>
        /// グリッド座標からワールド座標を求めます。
        /// </summary>
        public Vector2 GetGridToWorldPosition(Vector2Int gridPosition)
        {
            return (Vector2)gridPosition * CellSize;
        }

        /// <summary>
        /// グリッド座標から3D空間上のワールド座標を求めます。
        /// </summary>
        public Vector3 GetGridTo3DWorldPosition(Vector2Int gridPosition)
        {
            Vector2 worldPosition = GetGridToWorldPosition(gridPosition);
            return new Vector3(worldPosition.x, 0f, worldPosition.y);
        }

        /// <summary>
        /// ワールド座標からグリッド座標を求めます。
        /// </summary>
        public Vector2Int GetWorldToGridPosition(Vector2 worldPosition)
        {
            Vector2 gridPositionApproximate = worldPosition / CellSize;
            return Vector2Int.RoundToInt(gridPositionApproximate);
        }

        /// <summary>
        /// ワールド座標からセルを得ます。
        /// </summary>
        public TCellType GetCell(Vector2 worldPosition)
        {
            return GridRef.GetCell(GetWorldToGridPosition(worldPosition));
        }

        /// <summary>
        /// ワールド座標からセルをセットします。
        /// </summary>
        public void SetCell(Vector2 worldPosition, TCellType cell)
        {
            GridRef.SetCell(GetWorldToGridPosition(worldPosition), cell);
        }

        /// <summary>
        /// グリッドの真ん中のワールド座標を求めます。
        /// </summary>
        public Vector2 GetWorldCenterPosition()
        {
            return new Vector2((GridRef.Width - 1) / 2f * CellSize, (GridRef.Height - 1) / 2f * CellSize);
        }

        /// <summary>
        /// グリッドの範囲にワールド座標が収まっているかを返す
        /// </summary>
        public bool IsInRange(Vector2 worldPosition)
        {
            return GridRef.IsInRange(GetWorldToGridPosition(worldPosition));
        }
    }
}
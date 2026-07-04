using System;
using UnityEngine;

namespace MySRPGGame.Grid
{
    /// <summary>
    /// 抽象的な2次元グリッドを表します。
    /// グリッドの一つ一つのマスをセル(Cell)と呼称します。
    /// </summary>
    /// <typeparam name="TCellType">セルの型です。</typeparam>
    public class Grid<TCellType>
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        protected TCellType[] cells;

        public Grid(int width, int height)
        {
            Width = width;
            Height = height;
            cells = new TCellType[width * height];
        }

        public TCellType GetCell(Vector2Int position)
        {
            if (!IsInRange(position)) return default(TCellType);
            return cells[position.y * Width + position.x];
        }

        public void SetCell(Vector2Int position, TCellType cell)
        {
            if (!IsInRange(position)) return;
            cells[position.y * Width + position.x] = cell;
        }

        /// <summary>
        /// 全セルに対して処理を行います。
        /// </summary>
        /// <param name="cellAction">セルごとに実行します。セルの座標を引数に取り、新しいセルの値を返すようにします。</param>
        public void ExecuteAllCells(Func<Vector2Int, TCellType> cellAction)
        {
            for (int i = 0; i < Width * Height; ++i)
            {
                cells[i] = cellAction(new Vector2Int(i % Width, i / Width));
            }
        }

        /// <summary>
        /// グリッドの範囲に座標が収まっているかを返す
        /// </summary>
        public bool IsInRange(Vector2Int position)
        {
            return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
        }
    }
}

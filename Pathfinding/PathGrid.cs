using MySRPGGame.Grid;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MySRPGGame.Pathfinding
{
    /// <summary>
    /// 経路探索可能なグリッド
    /// </summary>
    /// <typeparam name="TCellType">セルの型</typeparam>
    public class PathGrid<TCellType> : Grid<TCellType> where TCellType : IPathItem
    {
        public PathGrid(int width, int height) : base(width, height)
        {

        }

        /*
        private List<Vector2Int> FindPathBFS(Vector2Int begin, Vector2Int end)
        {
            List<Vector2Int> path = new List<Vector2Int>();

            Vector2Int tmpPos = begin;
            begin = end;
            end = tmpPos;

            List<Vector2Int> around = new List<Vector2Int>()
            {
                new Vector2Int(1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
            };

            Dictionary<Vector2Int, Vector2Int> nodeToParentMap = new Dictionary<Vector2Int, Vector2Int>();

            bool isPathFound = false;
            Queue<Vector2Int> checkQueue = new Queue<Vector2Int>();
            checkQueue.Enqueue(begin);

            while (checkQueue.Count != 0)
            {
                Vector2Int current = checkQueue.Dequeue();
                if (current == end)
                {
                    isPathFound = true;
                    break;
                }

                foreach (Vector2Int dir in around)
                {
                    Vector2Int target = current + dir;

                    if (nodeToParentMap.ContainsKey(target)) continue;
                    if (target == begin) continue;
                    nodeToParentMap.Add(target, current);
                    checkQueue.Enqueue(target);
                }
            }

            if (isPathFound)
            {
                Vector2Int current = end;
                while (current != begin)
                {
                    path.Add(current);
                    current = nodeToParentMap[current];
                }
                path.Add(current);
            }
            return path;
        }
        */

        // (アルゴリズム内でGetCell()を用いていないのは少しでも軽量化するためです)

        /// <summary>
        /// ヒューリスティック値を計算
        /// </summary>
        public float ComputeHeuristic(Vector2Int begin, Vector2Int end)
        {
            return Mathf.Abs(end.x - begin.x) + Mathf.Abs(end.y - begin.y);
        }

        /// <summary>
        /// A*アルゴリズムで用いる一時データ
        /// </summary>
        class AStarScratch
        {
            public Vector2Int Parent; // -(1, 1)の場合、親がないとみなす
            public float Heuristic;
            public bool IsInOpenSet;
            public bool IsInClosedSet;
            public float ActualFromStart; // スタートからの実コスト

            public AStarScratch(Vector2Int parent, float heuristic, bool isInOpenSet, bool isInClosedSet, float actualFromStart)
            {
                Parent = parent;
                Heuristic = heuristic;
                IsInOpenSet = isInOpenSet;
                IsInClosedSet = isInClosedSet;
                ActualFromStart = actualFromStart;
            }
        }

        /// <summary>
        /// A*アルゴリズムで最短経路を求める
        /// </summary>
        private List<Vector2Int> FindPathAStar(Vector2Int start, Vector2Int goal, bool isIncludeStart = false, HashSet<Vector2Int> cannotPass = null)
        {
            // 最短経路
            List<Vector2Int> path = new List<Vector2Int>();

            // 最後に後ろから経路を求めていくため、最初に逆転させておく
            Vector2Int tmpPos = start;
            start = goal;
            goal = tmpPos;

            // 周囲に移動可能な方向
            List<Vector2Int> around = new List<Vector2Int>()
            {
                new Vector2Int(1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
            };

            // セルごとのデータ
            Dictionary<Vector2Int, AStarScratch> dataDictionary = new Dictionary<Vector2Int, AStarScratch>();

            // オープンセット（検査対象）
            List<Vector2Int> openSet = new List<Vector2Int>();

            Vector2Int current = start;
            // 開始セルをすぐにクローズセットに追加しておく
            dataDictionary.Add(current, new AStarScratch(-Vector2Int.one, 0.0f, false, true, 0f));

            do
            {
                foreach (Vector2Int dir in around)
                {
                    Vector2Int target = current + dir;

                    bool isContainCannotPass = cannotPass != null ? cannotPass.Contains(target) : false;
                    if (!IsInRange(target) || !cells[target.y * Width + target.x].CanPass || isContainCannotPass) continue;

                    // データが生成されていて既にクローズセットに入っているなら見る必要はない
                    if (dataDictionary.ContainsKey(target) && dataDictionary[target].IsInClosedSet) continue;

                    AStarScratch data;
                    if (!dataDictionary.ContainsKey(target))
                    {
                        // データがないなら生成
                        data = new AStarScratch(-Vector2Int.one, 0.0f, false, false, 0f);
                        dataDictionary.Add(target, data);
                    }
                    else
                    {
                        data = dataDictionary[target];
                    }
                   
                    if (!data.IsInClosedSet)
                    {
                        if (!data.IsInOpenSet)
                        {
                            // オープンセットに追加
                            data.Parent = current;
                            data.Heuristic = ComputeHeuristic(target, goal);

                            data.ActualFromStart = dataDictionary[current].ActualFromStart + cells[target.y * Width + target.x].Weight;
                            data.IsInOpenSet = true;
                            openSet.Add(target);
                        }
                        else
                        {
                            // 新しいコストを求めて、それが今までのコストより低ければ更新
                            float newCost = dataDictionary[current].ActualFromStart + cells[target.y * Width + target.x].Weight;
                            if (newCost < data.ActualFromStart)
                            {
                                data.Parent = current;
                                data.ActualFromStart = newCost;
                            }
                        }
                    }
                }

                if (openSet.Count == 0) break;

                // ヒューリスティック値と実コストを足した値が最小なセルを次の対象にしてクローズセットに移動
                current = openSet.OrderBy(x => dataDictionary[x].Heuristic + dataDictionary[x].ActualFromStart).First();
                openSet.Remove(current);
                AStarScratch currentData;
                currentData = dataDictionary[current];
                currentData.IsInOpenSet = false;
                currentData.IsInClosedSet = true;
            } while (current != goal);

            if (current == goal)
            {
                while (current != start)
                {
                    if (isIncludeStart || current != goal)
                    {
                        path.Add(current);
                    }
                    current = dataDictionary[current].Parent;
                }
                path.Add(current);
            }
            return path;
        }

        /// <summary>
        /// ダイクストラ法で移動可能範囲を求める
        /// </summary>
        private List<Vector2Int> FindMovableCellsDijkstra(Vector2Int begin, int maxCost, bool isIncludeStart = false, HashSet<Vector2Int> cannotPass = null)
        {
            // 周囲に移動可能な方向
            List<Vector2Int> around = new List<Vector2Int>()
            {
                new Vector2Int(1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
            };

            HashSet<Vector2Int> visitedMovableCells = new HashSet<Vector2Int>();
            Dictionary<Vector2Int, int> costFromStart = new Dictionary<Vector2Int, int>();

            costFromStart.Add(begin, 0);

            Vector2Int current = begin;

            while (current != -Vector2Int.one)
            {
                visitedMovableCells.Add(current);
                foreach (Vector2Int dir in around)
                {
                    Vector2Int target = current + dir;

                    bool isContainCannotPass = cannotPass != null ? cannotPass.Contains(target) : false;
                    if (!IsInRange(target) || !cells[target.y * Width + target.x].CanPass || isContainCannotPass) continue;

                    int cost = cells[target.y * Width + target.x].Weight;

                    // そのセルに移動するコストが最大コスト以上なら移動不可
                    if (cost + costFromStart[current] > maxCost) continue;

                    if (!costFromStart.ContainsKey(target))
                    {
                        costFromStart.Add(target, int.MaxValue);
                    }

                    // そのセルに初めて移動する、または移動するコストが以前のコストより低ければ更新
                    if (cost + costFromStart[current] < costFromStart[target])
                    {
                        costFromStart[target] = cost + costFromStart[current];
                    }
                }

                int cheapestCellCost = int.MaxValue;
                current = -Vector2Int.one; // -(1, 1)を無の意味で用いる

                // 訪れていないセルの中で最低コストのものを求める
                foreach (Vector2Int cost in costFromStart.Keys)
                {
                    if (!visitedMovableCells.Contains(cost) && costFromStart[cost] < cheapestCellCost)
                    {
                        cheapestCellCost = costFromStart[cost];
                        current = cost;
                    }
                }
            }

            if (!isIncludeStart)
            {
                visitedMovableCells.Remove(begin);
            }

            return new List<Vector2Int>(visitedMovableCells);
        }

        public List<Vector2Int> FindMovableCells(Vector2Int begin, int maxCost, bool isIncludeStart = false, HashSet<Vector2Int> cannotPass = null)
        {
            return FindMovableCellsDijkstra(begin, maxCost, isIncludeStart, cannotPass);
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, bool isIncludeStart = false, HashSet<Vector2Int> cannotPass = null)
        {
            return FindPathAStar(start, goal, isIncludeStart, cannotPass);
        }

        /// <summary>
        /// 全ての攻撃可能範囲を取得
        /// </summary>
        /// <param name="movableCells">移動可能範囲</param>
        /// <param name="attackRange">アクターの攻撃範囲</param>
        /// <param name="cannotAttack">攻撃できないセル</param>
        /// <returns></returns>
        public List<Vector2Int> GetAttackableCells(HashSet<Vector2Int> movableCells, List<Vector2Int> attackRange, HashSet<Vector2Int> cannotAttack = null)
        {
            List<Vector2Int> attackableCells = new List<Vector2Int>();
            
            foreach (Vector2Int pos in movableCells)
            {
                foreach (Vector2Int attackDir in attackRange)
                {
                    Vector2Int target = pos + attackDir;

                    bool isContainCannotAttack = cannotAttack != null ? cannotAttack.Contains(target) : false;
                    if (isContainCannotAttack || !IsInRange(target) || movableCells.Contains(target)) continue;

                    attackableCells.Add(target);
                }
            }

            return attackableCells;
        }
    }
}

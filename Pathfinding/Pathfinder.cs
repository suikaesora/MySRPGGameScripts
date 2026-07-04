using UnityEngine;
using MySRPGGame.Grid;
using System.Collections.Generic;

public static class Pathfinder
{
    public static List<Vector2Int> PathFindAStar<TCellType>(Grid<TCellType> grid, Vector2Int begin, Vector2Int end)
    {
        return new List<Vector2Int>() { Vector2Int.zero, Vector2Int.one };
    }
}

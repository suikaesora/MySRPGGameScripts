using UnityEngine;

public class PathCell : IPathItem
{
    public bool CanPass { get; set; }
    public int Weight { get; set; }

    public PathCell(bool canPass, int weight)
    {
        CanPass = canPass;
        Weight = weight;
    }
}

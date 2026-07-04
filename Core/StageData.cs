using UnityEngine;

/// <summary>
/// ひとつのステージのデータ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/Stage")]
public class StageData : ScriptableObject
{
    [SerializeField]
    private int width;

    [SerializeField]
    private int height;

    [SerializeField]
    private TextAsset groundTextAsset;

    [SerializeField]
    private TextAsset actorTextAsset;

    public int Width => width;

    public int Height => height;

    // 地形
    public TextAsset GroundTextAsset => groundTextAsset;

    // アクター
    public TextAsset ActorTextAsset => actorTextAsset;
}

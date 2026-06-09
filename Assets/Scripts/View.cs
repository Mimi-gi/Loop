using UnityEngine;
using Puzzle;

public class View : MonoBehaviour
{
    [SerializeField] Map map;
    WholeBoardData wholeMapData;
    void Awake()
    {
        // Viewの初期化等に合わせてプレイヤーの初期位置を決定して渡す
        wholeMapData = new WholeBoardData(map.EffectiveGridSize, map.miniGridSize, new Puzzle.Point(0, 0)); // Pointは仮の値
    }
}
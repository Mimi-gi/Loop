using UnityEngine;
using Puzzle;

public class View : MonoBehaviour
{
    [SerializeField] Map map;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject pillarPrefab;
    [SerializeField] GameObject LPrefab;
    [SerializeField] GameObject PPrefab;
    [SerializeField] GameObject OPrefab;
    [SerializeField] GameObject LpPrefab;
    [SerializeField] GameObject PpPrefab;
    [SerializeField] GameObject OpPrefab;
    [SerializeField] GameObject blockPrefab;

    [Header("カメラ")]
    [SerializeField] Camera mainCamera;
    WholeBoardData wholeMapData;
    GameObject playerObject;
    void Awake()
    {
        // Viewの初期化等に合わせてプレイヤーの初期位置を決定して渡す
        wholeMapData = new WholeBoardData(map.EffectiveGridSize, map.miniGridSize, map,map.InitialPlayerPosition); // Pointは仮の値
        BuildMap();
        mainCamera.transform.SetParent(playerObject.transform);
        mainCamera.transform.localPosition = new Vector3(0, 0, -10);
    }

    void BuildMap()
    {
        for (int i = 0; i < map.GridSize; i++)
        {
            for (int j = 0; j < map.GridSize; j++)
            {
                GameObject prefabToInstantiate = map.mapData[i, j] switch
                {
                    Puzzle.Component.Block => blockPrefab,
                    Puzzle.Component.Wall => wallPrefab,
                    Puzzle.Component.Pillar => pillarPrefab,
                    Puzzle.Component.L => LPrefab,
                    Puzzle.Component.P => PPrefab,
                    Puzzle.Component.O => OPrefab,
                    Puzzle.Component.Lp => LpPrefab,
                    Puzzle.Component.Pp => PpPrefab,
                    Puzzle.Component.Op => OpPrefab,
                    Puzzle.Component.InitialPos => playerPrefab,
                    _ => null,
                };
                if(prefabToInstantiate == null) continue;
                var obj = Instantiate(prefabToInstantiate, new Vector3(i, j, 0), Quaternion.identity);
                if (map.mapData[i, j] == Puzzle.Component.InitialPos)
                {
                    playerObject = obj;
                }
            }
        }
    }
}
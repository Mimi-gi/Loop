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
                GameObject prefabToInstantiate = map.mapData[i, j].Type switch
                {
                    Puzzle.PuzzleComponent.Block => blockPrefab,
                    Puzzle.PuzzleComponent.Wall => wallPrefab,
                    Puzzle.PuzzleComponent.Pillar => pillarPrefab,
                    Puzzle.PuzzleComponent.L => LPrefab,
                    Puzzle.PuzzleComponent.P => PPrefab,
                    Puzzle.PuzzleComponent.O => OPrefab,
                    Puzzle.PuzzleComponent.Lp => LpPrefab,
                    Puzzle.PuzzleComponent.Pp => PpPrefab,
                    Puzzle.PuzzleComponent.Op => OpPrefab,
                    Puzzle.PuzzleComponent.InitialPos => playerPrefab,
                    _ => null,
                };
                if(prefabToInstantiate == null) continue;
                var obj = Instantiate(prefabToInstantiate, new Vector3(i, j, 0), Quaternion.identity);
                if (map.mapData[i, j].Type == Puzzle.PuzzleComponent.InitialPos)
                {
                    playerObject = obj;
                }
            }
        }
    }
}
using UnityEngine;
using Puzzle;

public class View : MonoBehaviour
{
    [SerializeField] Map map;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject heightWallPrefab;
    [SerializeField] GameObject widthWallPrefab;
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
    [Header("Presenter")]
    [SerializeField] PuzzlePresenter presenter;
    [Header("コンフィグ")]
    [SerializeField] float moveDuration = 0.1f;
    GameObject playerObject;
    [SerializeField] int pixelizeUnit = 8;
    void Awake()
    {
        // Viewの初期化等に合わせてプレイヤーの初期位置を決定して渡す
        ViewUtility.GridSize = map.GridSize;
        ViewUtility.MiniGridSize = map.miniGridSize;
        wholeMapData = new WholeBoardData(map.GridSize, map.miniGridSize, map, map.InitialPlayerPosition); // Pointは仮の値
        presenter.WholeMapData = wholeMapData;
        BuildMap(wholeMapData);
        // カメラの位置を調整
        presenter.SetCamera(mainCamera);
    }

    bool isHeight(int i, int j)
    {
        return i % 2 == 0 && j % 2 == 1;
    }

    void BuildMap(WholeBoardData wholeMapData)
    {
        for (int i = 0; i < wholeMapData.Size; i++)
        {
            for (int j = 0; j < wholeMapData.Size; j++)
            {
                GameObject prefabToInstantiate = map.mapData[i, j].Type switch
                {
                    Puzzle.PuzzleComponent.Block => blockPrefab,
                    Puzzle.PuzzleComponent.Wall => isHeight(i, j) ? heightWallPrefab : widthWallPrefab,
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
                if (prefabToInstantiate == null) continue;
                var obj = Instantiate(prefabToInstantiate, ViewUtility.PointToVector3(new Point(i, j)), Quaternion.identity);
                if (map.mapData[i, j].Type == PuzzleComponent.InitialPos)
                {
                    playerObject = obj;
                    var playerViewer = playerObject.GetComponent<PlayerViewer>();
                    presenter.SetPlayerViewer(playerObject.GetComponent<PlayerViewer>());
                    playerViewer.pixelizeUnit = pixelizeUnit;
                    playerViewer.MoveDuration = moveDuration;

                    Debug.Log($"Player initial position: ({i}, {j})");
                }
                if (obj.TryGetComponent<IMovable>(out var movable))
                {
                    movable.MoveDuration = moveDuration;
                    movable.PixelizeUnit = pixelizeUnit;
                }
                presenter.componentToViewer[map.mapData[i, j]] = obj.GetComponent<IMovable>();
            }
        }
    }
}
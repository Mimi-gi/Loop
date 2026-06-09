using UnityEngine;
using Puzzle;

[System.Serializable]
[CreateAssetMenu(fileName = "Map", menuName = "Scriptable Objects/Map")]
public class Map : ScriptableObject
{
    [SerializeField]
    public int miniGridSize = 4;
    [SerializeField] private int gridNum = 3;
    public int EffectiveGridSize => miniGridSize*gridNum;
    public int GridSize => EffectiveGridSize * 2 + 1;
    public int MiniGridSize => 2*miniGridSize;

    [SerializeField, HideInInspector]
    Puzzle.Component[] serializedMapData;

    public Puzzle.Component[,] mapData;

    private void OnEnable()
    {
        // ロード時に serializedMapData から mapData を復元
        if (serializedMapData != null && serializedMapData.Length > 0)
        {
            To2D(serializedMapData);
        }
        else
        {
            ResetMapData(miniGridSize, gridNum);
        }
    }

    public void ResetMapData(int miniGridSize=4, int gridNum=3)
    {
        this.miniGridSize = miniGridSize;
        this.gridNum = gridNum;
        mapData = new Puzzle.Component[GridSize, GridSize];
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                mapData[i, j] = Puzzle.Component.Empty;
            }
        }
        serializedMapData = To1D();
    }

    public void To2D(Puzzle.Component[] data)
    {
        if (data.Length != GridSize * GridSize)
        {
            Debug.LogError("Data length does not match grid size.");
            return;
        }
        mapData = new Puzzle.Component[GridSize, GridSize];
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                mapData[i, j] = data[i * GridSize + j];
            }
        }
        serializedMapData = To1D();
    }
    public Puzzle.Component[] To1D()
    {
        Puzzle.Component[] data = new Puzzle.Component[GridSize * GridSize];
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                data[i * GridSize + j] = mapData[i, j];
            }
        }
        return data;
    }
}

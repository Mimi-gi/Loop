using UnityEngine;
using UnityEditor;
using Puzzle;

[CustomEditor(typeof(Map))]
public class MapMaker : Editor
{
    SerializedProperty miniGridSizeProp;
    SerializedProperty gridNumProp;
    Map map;
    Puzzle.Component[] mapData;
    int mapSize;
    int miniGridSize;
    void OnEnable()
    {
        map = (Map)target;
        miniGridSizeProp = serializedObject.FindProperty("miniGridSize");
        gridNumProp = serializedObject.FindProperty("gridNum");

        //map.ResetMapData();
        mapData = map.To1D();
        mapSize = map.GridSize;
        miniGridSize = map.MiniGridSize;
    }
    public override void OnInspectorGUI()
    {
        string hoverText = null;
        if (miniGridSizeProp == null)
        {
            miniGridSizeProp = serializedObject.FindProperty("miniGridSize");
        }
        if (gridNumProp == null)
        {
            gridNumProp = serializedObject.FindProperty("gridNum");
        }
        if (mapData == null || map.GridSize != mapSize)
        {
            mapData = map.To1D();
            mapSize = map.GridSize;
            miniGridSize = map.MiniGridSize;
        }
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(miniGridSizeProp);
        EditorGUILayout.PropertyField(gridNumProp);
        if (EditorGUI.EndChangeCheck())
        {
            map.ResetMapData(miniGridSizeProp.intValue, gridNumProp.intValue);
            mapData = map.To1D();
        }
        serializedObject.ApplyModifiedProperties();
        for (int y = 0; y < map.GridSize; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < map.GridSize; x++)
            {
                Puzzle.Component currentComponent = mapData[y * map.GridSize + x];
                GUI.backgroundColor = currentComponent.Type switch
                {
                    Puzzle.PuzzleComponent.Empty => Color.white,
                    Puzzle.PuzzleComponent.Wall => new Color(0f, 8.6f, 10.0f),
                    Puzzle.PuzzleComponent.Block => new Color(2f, 2f, 0.1f),
                    Puzzle.PuzzleComponent.L => new Color(0.2f, 0.3f, 1f),
                    Puzzle.PuzzleComponent.O => new Color(0.2f, 0.3f, 1f),
                    Puzzle.PuzzleComponent.P => new Color(0.2f, 0.3f, 1f),
                    Puzzle.PuzzleComponent.Lp => new Color(1f, 0.2f, 1f),
                    Puzzle.PuzzleComponent.Op => new Color(1f, 0.2f, 1f),
                    Puzzle.PuzzleComponent.Pp => new Color(1f, 0.2f, 1f),
                    Puzzle.PuzzleComponent.Player => new Color(2f, 2f, 2f),
                    Puzzle.PuzzleComponent.Pillar => new Color(1f, 0.2f, 0.0f),
                    Puzzle.PuzzleComponent.InitialPos => new Color(10f, 10f, 10f),
                    _ => Color.white,
                };
                if (currentComponent.Type != Puzzle.PuzzleComponent.Wall)
                {
                    if (x % 2 == 0 || y % 2 == 0)
                    {
                        GUI.backgroundColor *= new Color(0.3f, 0.3f, 0.3f);
                    }
                    if (x % (2 * miniGridSize) == 0 || y % (2 * miniGridSize) == 0)
                    {
                        GUI.backgroundColor *= Color.black;
                    }
                }
                if (x % 2 == 0 && y % 2 == 0)
                {
                    GUI.backgroundColor = Color.black;
                }
                int index = y * map.GridSize + x;
                var selectedType = (Puzzle.PuzzleComponent)EditorGUILayout.EnumPopup(mapData[index].Type, GUILayout.Width(20), GUILayout.Height(20));

                // 追加: 種類が変更された場合のみ、新しいインスタンス（新しいID）で上書きする
                if (selectedType != mapData[index].Type)
                {
                    mapData[index] = new Puzzle.Component(selectedType);
                }
                Rect popupRect = GUILayoutUtility.GetLastRect();
                if (popupRect.Contains(Event.current.mousePosition) && (x % 2) == 1 && (y % 2) == 1)
                {
                    // クリックされたセルの座標を計算
                    int clickedX = (x + 1) / 2 - 1;
                    int clickedY = map.EffectiveGridSize - ((y + 1) / 2 - 1);
                    GUI.Label(new Rect(popupRect.x, popupRect.y - 20, 100, 20), $"({clickedX}, {clickedY})");
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space(15);
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("Bake Map Data (保存)", GUILayout.Height(35)))
        {
            Undo.RecordObject(map, "Bake Map");

            // ★必須: 一時配列(mapData) を SO本体に送って 2D配列＆保存用1D配列 に書き込む
            map.To2D(mapData);

            // ★必須: Unityに「ファイルに保存して！」と要求する
            EditorUtility.SetDirty(map);

            Debug.Log($"マップデータをBakeしました！ (実サイズ: {map.GridSize}x{map.GridSize})");
        }

        GUI.backgroundColor = Color.white; // 色を戻す
    }
}
using Puzzle;
using System.Collections.Generic;
using UnityEngine;
using static ViewUtility;


public class PuzzlePresenter : MonoBehaviour
{
    List<IEvent> events = new List<IEvent>();
    WholeBoardData wholeMapData;
    public PuzzlePresenter(Map map)
    {
        //this.wholeMapData = new WholeBoardData(map.EffectiveGridSize, map.miniGridSize, map, map.InitialPlayerPosition);
        
    }
}
using UnityEngine;
using Puzzle;
using static ViewUtility;

public class PlayerViewer : MonoBehaviour, IViewer
{
    [SerializeField] Map map;
    GameObject playerObject;

    void Awake()
    {

    }
    public void SetGameObject(GameObject obj)
    {
        playerObject = obj;
    }
    public void Connect()
    {

    }
    public void Process(IEvent events)
    {
        switch (events.Type)
        {
            case Puzzle.EventType.PLMove:
                var moveEvent = events as PLMoveEvent;
                break;
            case Puzzle.EventType.PLConnect:
                break;
            case Puzzle.EventType.PLDisConnect:
                break;
            default:
                break;
        }
    }
    public void Return(IEvent events)
    {

    }
}
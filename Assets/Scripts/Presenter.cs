using Puzzle;
using System.Collections.Generic;
using UnityEngine;
using static Utility;
using static ViewUtility;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

public class PuzzlePresenter : MonoBehaviour
{
    List<IEvent> events = new List<IEvent>();
    public WholeBoardData WholeMapData;
    PlayerViewer playerViewer;
    public Dictionary<Puzzle.Component, IMovable> componentToViewer = new Dictionary<Puzzle.Component, IMovable>();

    public void SetPlayerViewer(PlayerViewer playerViewer)
    {
        this.playerViewer = playerViewer;
    }

    public void SetCamera(Camera camera)
    {
        camera.transform.position = TagToVector3(WholeMapData.GetPlayerTag()) + new Vector3(0, 0, -10);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            var direction = context.ReadValue<Vector2>();
            InputType dir;
            if (direction == Vector2.up) dir = InputType.Up;
            else if (direction == Vector2.down) dir = InputType.Down;
            else if (direction == Vector2.left) dir = InputType.Left;
            else if (direction == Vector2.right) dir = InputType.Right;
            else return;

            events = WholeMapData.Process(dir);
            EventSolver();
        }
    }

    public void OnConnect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            events = WholeMapData.Process(InputType.Connect);
            EventSolver();
        }
    }

    public async UniTask EventSolver()
    {
        if (events.Count == 0) return;
        if (events.Count == 1)
        {
            if (events[0] is PLMoveEvent moveEvent)
            {
                SetCamera(Camera.main);
                await MoveView(moveEvent);

            }
            else if (events[0] is PLConnectEvent connectEvent)
            {
                await playerViewer.Process(connectEvent);
            }
            else if (events[0] is PLDisConnectEvent disconnectEvent)
            {
                await playerViewer.Process(disconnectEvent);
            }
        }

    }

    public async UniTask MoveView(PLMoveEvent moveEvent)
    {
        List<UniTask> tasks = new List<UniTask>();
        if (moveEvent.ConnectComponents.Item1 != null)
        {
            var taskN = componentToViewer[moveEvent.ConnectComponents.Item1].PropagateMove(WholeMapData.GetNextPoint(moveEvent.From, Direction.North), WholeMapData.GetNextPoint(moveEvent.To, Direction.North));
            tasks.Add(taskN);
        }
        if (moveEvent.ConnectComponents.Item2 != null)
        {
            var taskS = componentToViewer[moveEvent.ConnectComponents.Item2].PropagateMove(WholeMapData.GetNextPoint(moveEvent.From, Direction.South), WholeMapData.GetNextPoint(moveEvent.To, Direction.South));
            tasks.Add(taskS);
        }
        if (moveEvent.ConnectComponents.Item3 != null)
        {
            var taskW = componentToViewer[moveEvent.ConnectComponents.Item3].PropagateMove(WholeMapData.GetNextPoint(moveEvent.From, Direction.West), WholeMapData.GetNextPoint(moveEvent.To, Direction.West));
            tasks.Add(taskW);
        }
        if (moveEvent.ConnectComponents.Item4 != null)
        {
            var taskE = componentToViewer[moveEvent.ConnectComponents.Item4].PropagateMove(WholeMapData.GetNextPoint(moveEvent.From, Direction.East), WholeMapData.GetNextPoint(moveEvent.To, Direction.East));
            tasks.Add(taskE);
        }
        Debug.Log($"{tasks.Count} components to move.");
        var taskP = playerViewer.Process(moveEvent);
        tasks.Add(taskP);
        await UniTask.WhenAll(tasks);
    }

}
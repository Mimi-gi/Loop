using UnityEngine;
using Puzzle;
public interface IViewer
{
    public void SetGameObject(GameObject obj);
    public void Process(Puzzle.IEvent component);
    public void Return(Puzzle.IEvent component);
}

public interface IMovable
{
    public void PropagateMove(Direction direction);
}

public static class ViewUtility
{
    public static int GridSize;
    public static int MiniGridSize;
    public static Vector3 PointToVector3(Point point)
    {
        return new Vector3(point.X / 2f, point.Y / 2f, 0);
    }

    public static Vector3 TagToVector3(Tag tag)
    {
        return PointToVector3(new Point(tag.First * MiniGridSize, tag.Second * MiniGridSize)) * 2f + PointToVector3(new Point(MiniGridSize, MiniGridSize));
    }
}
using System.Collections.Generic;
using System;
using System.Linq;

namespace Puzzle
{


    public struct Point
    {
        public int X { get; }
        public int Y { get; }
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }
        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }
        public static Point operator *(Point a, int scalar)
        {
            return new Point(a.X * scalar, a.Y * scalar);
        }
        public static Point operator *(int scalar, Point a)
        {
            return new Point(a.X * scalar, a.Y * scalar);
        }
        public static Point operator *(Point a, Point b)
        {
            return new Point(a.X * b.X, a.Y * b.Y);
        }

    }



    public struct LoopSquare
    {
        Turn loopTurn;
        public Turn LoopTurn => loopTurn;
        public bool IsLoop => loopTurn != Turn.None;
        public enum Turn
        {
            None,
            Left,
            Right
        }

        (Point, Component)[] loop; //ループを構成する4点とそのコンポーネント

        public (Point, Component) L => loop[0];
        public (Point, Component) O1 => loop[1];
        public (Point, Component) O2 => loop[2];
        public (Point, Component) P => loop[3];

        public LoopSquare((Point, Component) p1, (Point, Component) p2, (Point, Component) p3, (Point, Component) p4)
        {
            loopTurn = Turn.None;

            (Point, Component)[] sorted = new (Point, Component)[] { p1, p2, p3, p4 }.OrderBy(p => p.Item1.X).ThenBy(p => p.Item1.Y).ToArray();
            Point[] points = sorted.Select(p => p.Item1).Distinct().ToArray();
            int[] pointXs = points.Select(p => p.X).Distinct().ToArray();
            int[] pointYs = points.Select(p => p.Y).Distinct().ToArray();

            if (points.Length != 4 || pointXs.Length != 2 || pointYs.Length != 2)
            {
                loop = sorted;
            }

            (Point, Component)[] rightTurnAllay = new (Point, Component)[] { sorted[0], sorted[1], sorted[3], sorted[2] }; //右上、右下、左上、左下
            for (int i = 0; i < 4; i++)
            {
                var L = new ModInt(i, 4);
                var O1 = new ModInt(i + 1, 4);
                var O2 = new ModInt(i + 2, 4);
                var P = new ModInt(i + 3, 4);
                if (rightTurnAllay[L].Item2 == Component.L && rightTurnAllay[O1].Item2 == Component.O && rightTurnAllay[O2].Item2 == Component.O && rightTurnAllay[P].Item2 == Component.P)
                {
                    loopTurn = Turn.Right;
                    loop = new (Point, Component)[] { rightTurnAllay[L], rightTurnAllay[O1], rightTurnAllay[O2], rightTurnAllay[P] };
                }
            }
            for (int i = 0; i < 4; i++)
            {
                var L = new ModInt(i, 4);
                var O1 = new ModInt(i - 1, 4);
                var O2 = new ModInt(i - 2, 4);
                var P = new ModInt(i - 3, 4);
                if (rightTurnAllay[L].Item2 == Component.L && rightTurnAllay[O1].Item2 == Component.O && rightTurnAllay[O2].Item2 == Component.O && rightTurnAllay[P].Item2 == Component.P)
                {
                    loopTurn = Turn.Left;
                    loop = new (Point, Component)[] { rightTurnAllay[L], rightTurnAllay[O1], rightTurnAllay[O2], rightTurnAllay[P] };
                }
            }

            loop = sorted;
        }

    }

    public class TaggedGrid<T>
    {
        int gridSize;
        (int, int) tagNum;
        T[,] grid; //全体のグリッド
        List<(int, int)>[,] tagGrid; //座標ごとのタグを管理するグリッド
        Func<(int, int), List<Point>> TagFunc; //タグからPointを生成する関数
        public TaggedGrid(int gridSize, (int, int) tagNum, Func<(int, int), List<Point>> TagFunc)
        {
            this.gridSize = gridSize;
            this.tagNum = tagNum;
            grid = new T[gridSize, gridSize];
            tagGrid = new List<(int, int)>[gridSize, gridSize];
            this.TagFunc = TagFunc;
            for (int i = 0; i < tagNum.Item1; i++)
            {
                for (int j = 0; j < tagNum.Item2; j++)
                {
                    var points = TagFunc((i, j));
                    foreach (var p in points)
                    {
                        if (p.X >= 0 && p.X < gridSize && p.Y >= 0 && p.Y < gridSize)
                        {
                            if (tagGrid[p.X, p.Y] == null)
                                tagGrid[p.X, p.Y] = new List<(int, int)>();
                            tagGrid[p.X, p.Y].Add((i, j));
                        }
                    }
                }
            }
        }
        public void Set(int x, int y, T value)
        {
            grid[x, y] = value;
        }
        public void Set(Point p, T value)
        {
            Set(p.X, p.Y, value);
        }
        public T GetComponent(int x, int y)
        {
            return grid[x, y];
        }
        public T Get(Point p)
        {
            return GetComponent(p.X, p.Y);
        }
        public void Exchange(int x1, int y1, int x2, int y2)
        {
            T temp = grid[x1, y1];
            grid[x1, y1] = grid[x2, y2];
            grid[x2, y2] = temp;
        }
        public void Exchange(Point p1, Point p2)
        {
            Exchange(p1.X, p1.Y, p2.X, p2.Y);
        }

        public List<(int, int)> GetTag(int x, int y)
        {
            return tagGrid[x, y];
        }
        public List<(int, int)> GetTag(Point p)
        {
            return GetTag(p.X, p.Y);
        }
        public (int, int) GetPrimaryTag(Point p)
        {
            var tags = GetTag(p.X, p.Y);
            return tags.Count > 0 ? tags[0] : (-1, -1); //タグがない場合は(-1, -1)を返す
        }

        public HashSet<(Point, T)> GetPointsByTag((int, int) tag)
        {
            var points = TagFunc(tag);
            HashSet<(Point, T)> result = new HashSet<(Point, T)>();
            foreach (var p in points)
            {
                if (p.X >= 0 && p.X < gridSize && p.Y >= 0 && p.Y < gridSize)
                {
                    result.Add((p, Get(p)));
                }
            }
            return result;
        }
    }
    public class WholeBoardData
    {
        int size;
        int smallSize;
        TaggedGrid<Component> components;
        (bool, bool, bool, bool) isConnected; //上下左右のつながりを管理
        Point playerPosition;
        public int Size { get => size; }
        public int SmallSize { get => smallSize; }
        public TaggedGrid<Component> Components { get => components; }
        public WholeBoardData(int size, int smallSize)
        {
            this.size = size;
            this.smallSize = smallSize;
            components = new TaggedGrid<Component>(size, (smallSize, smallSize), TagSolver);
        }

        readonly Direction[] directions = new Direction[] { Direction.North, Direction.South, Direction.East, Direction.West };


        public List<Point> TagSolver((int, int) tag)
        {
            List<Point> points = new List<Point>();
            for (int i = 2 * smallSize * tag.Item1; i <= 2 * smallSize * (tag.Item1 + 1); i++)
            {
                for (int j = 2 * smallSize * tag.Item2; j < 2 * smallSize * (tag.Item2 + 1); j++)
                {
                    points.Add(new Point(i, j));
                }
            }
            return points;
        }

        bool IsConnecting()
        {
            return isConnected.Item1 || isConnected.Item2 || isConnected.Item3 || isConnected.Item4;
        }

        public void ExchangeComponents(Point p1, Point p2)
        {
            components.Exchange(p1, p2);
        }
        Point GetNextPoint(Point point, Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    return new Point(point.X, point.Y - 2);
                case Direction.South:
                    return new Point(point.X, point.Y + 2);
                case Direction.East:
                    return new Point(point.X + 2, point.Y);
                case Direction.West:
                    return new Point(point.X - 2, point.Y);
                default:
                    throw new ArgumentException("Invalid direction");
            }
        }

        Point GetNextWallPoint(Point point, Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    return new Point(point.X, point.Y - 1);
                case Direction.South:
                    return new Point(point.X, point.Y + 1);
                case Direction.East:
                    return new Point(point.X + 1, point.Y);
                case Direction.West:
                    return new Point(point.X - 1, point.Y);
                default:
                    throw new ArgumentException("Invalid direction");
            }
        }
        Component GetNextComponent(Point point, Direction dir)
        {
            return components.Get(GetNextPoint(point, dir));
        }
        Component GetNextWall(Point point, Direction dir)
        {
            return components.Get(GetNextWallPoint(point, dir));
        }

        bool IsConnectable(Point p)
        {
            Component c = components.Get(p);
            switch (c)
            {
                case Component.Block:
                case Component.L:
                case Component.O:
                case Component.P:
                case Component.Lp:
                case Component.Op:
                case Component.Pp:
                    return true; //仮実装
                default:
                    return false;
            }
        }

        Direction[] ConnectDirections()
        {
            List<Direction> connectDirs = new List<Direction>();
            if (isConnected.Item1) connectDirs.Add(Direction.North);
            if (isConnected.Item2) connectDirs.Add(Direction.South);
            if (isConnected.Item3) connectDirs.Add(Direction.West);
            if (isConnected.Item4) connectDirs.Add(Direction.East);
            return connectDirs.ToArray();
        }

        public bool CanPLMove(Point point, Direction dir)
        {
            if (GetNextComponent(point, dir) != Component.Empty || GetNextWall(point, dir) != Component.Empty)
            {
                return false;
            }

            foreach (var d in ConnectDirections())
            {
                Point p = GetNextPoint(point, d);
                Point NextP = GetNextPoint(p, dir);
                var preTag = components.GetTag(p);
                var nextTag = components.GetTag(NextP);
                if (GetNextComponent(p, dir) != Component.Empty || GetNextWall(p, dir) != Component.Empty || preTag.Count != 1 || nextTag.Count != 1 || preTag[0] != nextTag[0]) //タグの重複は無し。
                {
                    return false;
                }
            }
            return true;
        }

        public void MovePL(Point point, Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    if (ConnectDirections().Contains(Direction.North))
                    {
                        Point p = GetNextPoint(point, Direction.North);
                        Point NextP = GetNextPoint(p, Direction.North);
                        components.Exchange(p, NextP);
                        components.Exchange(point, p);
                    }
                    if (ConnectDirections().Contains(Direction.South))
                    {
                        Point p = GetNextPoint(point, Direction.South);
                        components.Exchange(point, p);
                    }
                    if (ConnectDirections().Contains(Direction.East))
                    {
                        Point p = GetNextPoint(point, Direction.East);
                        Point NextP = GetNextPoint(p, Direction.North);
                        components.Exchange(p, NextP);
                    }
                    if (ConnectDirections().Contains(Direction.West))
                    {
                        Point p = GetNextPoint(point, Direction.West);
                        Point NextP = GetNextPoint(p, Direction.North);
                        components.Exchange(p, NextP);
                    }
                    break;
                case Direction.South:
                    if (ConnectDirections().Contains(Direction.South))
                    {
                        Point p = GetNextPoint(point, Direction.South);
                        Point NextP = GetNextPoint(p, Direction.South);
                        components.Exchange(p, NextP);
                        components.Exchange(point, p);
                    }
                    if (ConnectDirections().Contains(Direction.North))
                    {
                        Point p = GetNextPoint(point, Direction.North);
                        components.Exchange(p, point);
                    }
                    if (ConnectDirections().Contains(Direction.East))
                    {
                        Point p = GetNextPoint(point, Direction.East);
                        Point NextP = GetNextPoint(p, Direction.South);
                        components.Exchange(p, NextP);
                    }
                    if (ConnectDirections().Contains(Direction.West))
                    {
                        Point p = GetNextPoint(point, Direction.West);
                        Point NextP = GetNextPoint(p, Direction.South);
                        components.Exchange(p, NextP);
                    }
                    break;
                case Direction.East:
                    if (ConnectDirections().Contains(Direction.East))
                    {
                        Point p = GetNextPoint(point, Direction.East);
                        Point NextP = GetNextPoint(p, Direction.East);
                        components.Exchange(p, NextP);
                        components.Exchange(point, p);
                    }
                    if (ConnectDirections().Contains(Direction.West))
                    {
                        Point p = GetNextPoint(point, Direction.West);
                        components.Exchange(point, p);
                    }
                    if (ConnectDirections().Contains(Direction.North))
                    {
                        Point p = GetNextPoint(point, Direction.North);
                        Point NextP = GetNextPoint(p, Direction.East);
                        components.Exchange(p, NextP);
                    }
                    if (ConnectDirections().Contains(Direction.South))
                    {
                        Point p = GetNextPoint(point, Direction.South);
                        Point NextP = GetNextPoint(p, Direction.East);
                        components.Exchange(p, NextP);
                    }
                    break;
                case Direction.West:
                    if (ConnectDirections().Contains(Direction.West))
                    {
                        Point p = GetNextPoint(point, Direction.West);
                        Point NextP = GetNextPoint(p, Direction.West);
                        components.Exchange(p, NextP);
                        components.Exchange(point, p);
                    }
                    if (ConnectDirections().Contains(Direction.East))
                    {
                        Point p = GetNextPoint(point, Direction.East);
                        components.Exchange(point, p);
                    }
                    if (ConnectDirections().Contains(Direction.North))
                    {
                        Point p = GetNextPoint(point, Direction.North);
                        Point NextP = GetNextPoint(p, Direction.West);
                        components.Exchange(p, NextP);
                    }
                    if (ConnectDirections().Contains(Direction.South))
                    {
                        Point p = GetNextPoint(point, Direction.South);
                        Point NextP = GetNextPoint(p, Direction.West);
                        components.Exchange(p, NextP);
                    }
                    break;
                default:
                    throw new ArgumentException("Invalid direction");
            }
        }

        public (bool, bool, bool, bool) CanConnect()
        {
            bool north = false;
            bool south = false;
            bool east = false;
            bool west = false;

            foreach (var d in directions)
            {
                Point p = GetNextPoint(playerPosition, d);
                Point wallP = GetNextWallPoint(playerPosition, d);
                var tag = components.GetTag(playerPosition);
                var tag2 = components.GetTag(p);
                if (IsConnectable(p) && (tag[0] == tag2[0]) && components.Get(wallP) == Component.Empty) //接続可能なコンポーネントは仮実装
                {
                    switch (d)
                    {
                        case Direction.North:
                            north = true;
                            break;
                        case Direction.South:
                            south = true;
                            break;
                        case Direction.East:
                            east = true;
                            break;
                        case Direction.West:
                            west = true;
                            break;
                    }
                }
            }
            return (north, south, east, west); //仮実装
        }
        public void Connect()
        {
            isConnected = CanConnect();

        }

        public void DisConnect()
        {
            isConnected = (false, false, false, false);
        }


        void LoopProcess()
        {
            //盤面の走査
        }

    }

    public enum Direction
    {
        North,
        South,
        East,
        West,
        Up,
        Down
    }

    public enum Component
    {
        Empty,
        Wall, //壁
        L,
        O,
        P,
        Lp, //強化L
        Op, //強化O
        Pp, //強化P
        Block, //通常
        Player,
        Pillar
    }
    public enum EventType
    {
        PLMove,
        PLConnect,
        PLDisConnect,
        WallMove,
        LOOPDetect,
    }

    public interface IEvent
    {
        public EventType Type { get; }
    }

}
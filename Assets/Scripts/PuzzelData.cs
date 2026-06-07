using System.Collections.Generic;
using System;

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
        public T Get(int x, int y)
        {
            return grid[x, y];
        }
        public T Get(Point p)
        {
            return Get(p.X, p.Y);
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
        public List<(int, int)> GetGroupIndex(Point p)
        {
            return GetTag(p.X, p.Y);
        }
        public (int, int) GetPrimaryTag(Point p)
        {
            var tags = GetTag(p.X, p.Y);
            return tags.Count > 0 ? tags[0] : (-1, -1); //タグがない場合は(-1, -1)を返す
        }
    }
    public class WholeBoardData
    {
        int size;
        int smallSize;
        TaggedGrid<Component> components;
        (bool, bool, bool, bool) isConnected; //上下左右のつながりを管理
        public int Size { get => size; }
        public int SmallSize { get => smallSize; }
        public TaggedGrid<Component> Components { get => components; }
        public WholeBoardData(int size, int smallSize)
        {
            this.size = size;
            this.smallSize = smallSize;
            components = new TaggedGrid<Component>(size, (smallSize, smallSize), TagSolver); // Initialize with a simple tag function
        }


        public List<Point> TagSolver((int, int) tag)
        {
            List<Point> points = new List<Point>();
            for(int i=2*smallSize*tag.Item1; i<=2*smallSize*(tag.Item1+1); i++)
            {
                for(int j=2*smallSize*tag.Item2; j<2*smallSize*(tag.Item2+1); j++)
                {
                    points.Add(new Point(i, j));
                }
            }
            return points;
        }

        public void ExchangeComponents(Point p1, Point p2)
        {
            components.Exchange(p1, p2);
        }

        public bool CanPLMove(Point point, Direction dir, bool isConnect)
        {
            //プレイヤーが(point)にいるとき、dirの方向に移動できるか
            //壁がないか、範囲外でないかなどを確認する
            //判定のみ。
            //例: if (dir == Direction.North) { return components[point.X, point.Y - 1] != Component.Wall; }
            return true; //仮実装
        }

        public void MovePL(Point point, Direction dir)
        {
            //プレイヤーを(point)からdirの方向に移動させる
            //実際の移動処理。CanPLMoveで確認した後に呼び出すべき。
            //例: if (dir == Direction.North) { components[point.X, point.Y] = Component.Empty; components[point.X, point.Y - 1] = Component.Player; }
        }

        public bool TryConnect()
        {
            //接続可能なLとPを探して接続する
            //接続できたらtrue、できなかったらfalseを返す
            return true; //仮実装
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
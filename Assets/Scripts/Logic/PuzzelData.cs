using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace Puzzle
{
    public struct LoopSquare : IEquatable<LoopSquare>
    {
        Turn loopTurn;
        public Turn LoopTurn => loopTurn;
        public bool IsLoop => loopTurn != Turn.None;

        public int Surface
        {
            get
            {
                if (loopTurn == Turn.None) return 0;

                // 距離の二乗を使って面積を計算（長方形の面積 = 縦の長さ × 横の長さ）
                // PuzzelDataでは座標系が違うかもしれないが、辺の長さを掛ける
                float norm1 = (O1.p - L.p).norm;
                float norm2 = (O2.p - O1.p).norm;
                return (int)(norm1 * norm2);
            }
        }

        public enum Turn
        {
            None,
            Left,
            Right
        }

        public int MaxX => loop.Max(p => p.Item1.X);
        public int MinX => loop.Min(p => p.Item1.X);
        public int MaxY => loop.Max(p => p.Item1.Y);
        public int MinY => loop.Min(p => p.Item1.Y);


        (Point, Component)[] loop; //ループを構成する4点とそのコンポーネント

        public (Point p, Component c) L => loop[0];
        public (Point p, Component c) O1 => loop[1];
        public (Point p, Component c) O2 => loop[2];
        public (Point p, Component c) P => loop[3];

        public override string ToString()
        {
            if (!IsLoop) return "Not a Loop";
            return $"LoopSquare: L({L.p.X}, {L.p.Y}), O1({O1.p.X}, {O1.p.Y}), O2({O2.p.X}, {O2.p.Y}), P({P.p.X}, {P.p.Y}), Turn: {loopTurn}, Surface: {Surface}";
        }



        public LoopSquare((Point, Component) p1, (Point, Component) p2, (Point, Component) p3, (Point, Component) p4)
        {
            loopTurn = Turn.None;
            bool isL(Component c) => c.Type == PuzzleComponent.L || c.Type == PuzzleComponent.Lp;
            bool isO(Component c) => c.Type == PuzzleComponent.O || c.Type == PuzzleComponent.Op;
            bool isP(Component c) => c.Type == PuzzleComponent.P || c.Type == PuzzleComponent.Pp;

            (Point, Component)[] sorted = new (Point, Component)[] { p1, p2, p3, p4 }.OrderBy(p => p.Item1.X).ThenBy(p => p.Item1.Y).ToArray();
            Point[] points = sorted.Select(p => p.Item1).Distinct().ToArray();
            int[] pointXs = points.Select(p => p.X).Distinct().ToArray();
            int[] pointYs = points.Select(p => p.Y).Distinct().ToArray();

            if (points.Length != 4 || pointXs.Length != 2 || pointYs.Length != 2)
            {
                loop = sorted;
                return; // ★長方形でない場合は、Turn.Noneのまま終了する
            }

            // OrderBy(X).ThenBy(Y) のため、以下のような順番になります。
            // sorted[0]: 左下 (MinX, MinY)
            // sorted[1]: 左上 (MinX, MaxY)
            // sorted[2]: 右下 (MaxX, MinY)
            // sorted[3]: 右上 (MaxX, MaxY)
            // これらを時計回り(左下→左上→右上→右下)に並べ替えます。
            (Point, Component)[] rightTurnAllay = new (Point, Component)[] { sorted[0], sorted[1], sorted[3], sorted[2] };
            for (int i = 0; i < 4; i++)
            {
                var L = new ModInt(i, 4);
                var O1 = new ModInt(i + 1, 4);
                var O2 = new ModInt(i + 2, 4);
                var P = new ModInt(i + 3, 4);
                if (isL(rightTurnAllay[L].Item2) && isO(rightTurnAllay[O1].Item2) && isO(rightTurnAllay[O2].Item2) && isP(rightTurnAllay[P].Item2))
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
                if (isL(rightTurnAllay[L].Item2)
                && isO(rightTurnAllay[O1].Item2)
                && isO(rightTurnAllay[O2].Item2)
                && isP(rightTurnAllay[P].Item2))
                {
                    loopTurn = Turn.Left;
                    loop = new (Point, Component)[] { rightTurnAllay[L], rightTurnAllay[O1], rightTurnAllay[O2], rightTurnAllay[P] };
                }
            }
            loop = sorted;
        }

        public bool Equals(LoopSquare other)
        {
            if (!IsLoop || !other.IsLoop) return false;
            // 構成する4つのコンポーネントのIDが全て一致していれば同じLOOPとみなす
            return this.L.c.ID == other.L.c.ID &&
                   this.O1.c.ID == other.O1.c.ID &&
                   this.O2.c.ID == other.O2.c.ID &&
                   this.P.c.ID == other.P.c.ID;
        }
        public override bool Equals(object obj) => obj is LoopSquare other && Equals(other);
        public override int GetHashCode()
        {
            if (!IsLoop) return 0;
            return HashCode.Combine(L.c.ID, O1.c.ID, O2.c.ID, P.c.ID);
        }
    }

    public class SMTPoints
    {

        SignedMaximTracker xTracker;
        SignedMaximTracker yTracker;
        public SMTPoints(Point initial)
        {
            xTracker = new SignedMaximTracker();
            yTracker = new SignedMaximTracker();
            Add(initial);
        }
        public void Add(Point p)
        {
            xTracker.Add(p.X);
            yTracker.Add(p.Y);
        }
        public Point Value => new Point(xTracker.Value, yTracker.Value);
    }

    [Serializable]
    public class Component
    {
        private static int _nextId = 0;

        // 変数名をアンダースコア付きにして明確に分ける
        [SerializeField] private int _id;
        public int ID => _id; // シリアライズされた変数を返すだけにする

        [SerializeField] private PuzzleComponent _type;
        public PuzzleComponent Type => _type;

        public Component(PuzzleComponent type, int _id = 0)
        {
            if (_id != -1)
            {
                _id = _nextId++;
            }
            _type = type;
        }

        public Component(int id, PuzzleComponent type)
        {
            _id = id;
            _type = type;
            if (id >= _nextId)
            {
                _nextId = id + 1;
            }
        }

        public static readonly Component Null = new Component(PuzzleComponent.Null, -1);
    }
    public enum PuzzleComponent
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
        Pillar,
        InitialPos,
        Null
    }
    public struct Point : IComparable<Point>
    {
        public int X { get; }
        public int Y { get; }
        public float norm => (float)Math.Sqrt(X * X + Y * Y);

        public int CompareTo(Point other)
        {
            if (this.X != other.X)
                return this.X.CompareTo(other.X);
            return this.Y.CompareTo(other.Y);
        }

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
        public static bool operator ==(Point a, Point b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }
        public static bool operator <(Point a, Point b) //辞書式
        {
            return a.X < b.X || (a.X == b.X && a.Y < b.Y);
        }
        public static bool operator >(Point a, Point b)
        {
            return a.X > b.X || (a.X == b.X && a.Y > b.Y);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Point)) return false;
            Point p = (Point)obj;
            return X == p.X && Y == p.Y;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    public struct Tag
    {
        public int First { get; }
        public int Second { get; }
        public int Third { get; }
        public Tag(int first, int second, int third = -1)
        {
            First = first;
            Second = second;
            Third = third;
        }
        public static bool operator ==(Tag a, Tag b)
        {
            return a.First == b.First && a.Second == b.Second && a.Third == b.Third;
        }
        public static bool operator !=(Tag a, Tag b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Tag)) return false;
            Tag t = (Tag)obj;
            return this == t;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(First, Second, Third);
        }
        public static bool operator <(Tag a, Tag b)
        {
            return a.First < b.First || (a.First == b.First && (a.Second < b.Second || (a.Second == b.Second && a.Third < b.Third)));
        }
        public static bool operator >(Tag a, Tag b)
        {
            return a.First > b.First || (a.First == b.First && (a.Second > b.Second || (a.Second == b.Second && a.Third > b.Third)));
        }
        public static bool operator <=(Tag a, Tag b)
        {
            return a < b || a == b;
        }
        public static bool operator >=(Tag a, Tag b)
        {
            return a > b || a == b;
        }

        public struct TagSquare
        {
            public Tag TL { get; }
            public Tag TO1 { get; }
            public Tag TO2 { get; }
            public Tag TP { get; }
            public TagSquare(Tag tl, Tag to1, Tag to2, Tag tp)
            {
                TL = tl;
                TO1 = to1;
                TO2 = to2;
                TP = tp;
            }

        }
    }



    public class TaggedGrid<T>
    {
        int gridSize;
        (int, int) tagNum; //縦のタグの個数×横のタグの個数
        T[,] grid; //全体のグリッド
        List<Tag>[,] tagGrid; //座標ごとのタグを管理するグリッド。List<Tag>にできる
        Func<Tag, List<Point>> TagFunc; //タグからPointを生成する関数。Func<Tag>にできる。
        public TaggedGrid(int gridSize, (int, int) tagNum, Func<Tag, List<Point>> TagFunc)
        {
            this.gridSize = gridSize;
            this.tagNum = tagNum;
            grid = new T[gridSize, gridSize];
            tagGrid = new List<Tag>[gridSize, gridSize];
            this.TagFunc = TagFunc;
            for (int i = 0; i < tagNum.Item1; i++)
            {
                for (int j = 0; j < tagNum.Item2; j++)
                {
                    var points = TagFunc(new Tag(i, j));
                    foreach (var p in points)
                    {
                        if (p.X >= 0 && p.X < gridSize && p.Y >= 0 && p.Y < gridSize)
                        {
                            if (tagGrid[p.X, p.Y] == null)
                                tagGrid[p.X, p.Y] = new List<Tag>();
                            tagGrid[p.X, p.Y].Add(new Tag(i, j));
                        }
                    }
                }
            }
        }
        public void Build(T[,] data)
        {
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    grid[i, j] = data[i, j];
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

        public List<Tag> GetTag(int x, int y)
        {
            return tagGrid[x, y];
        }
        public List<Tag> GetTag(Point p)
        {
            return GetTag(p.X, p.Y);
        }
        public Tag GetPrimaryTag(Point p)
        {
            var tags = GetTag(p.X, p.Y);
            return tags.Count > 0 ? tags[0] : new Tag(-1, -1); //タグがない場合は(-1, -1)を返す
        }

        public HashSet<(Point, T)> GetPointsByTag(Tag tag)
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

        public int TagArea(LoopSquare loopSquare)
        {
            Tag tag1 = GetPrimaryTag(loopSquare.L.p);
            Tag tag2 = GetPrimaryTag(loopSquare.O1.p);
            Tag tag3 = GetPrimaryTag(loopSquare.O2.p);
            Tag tag4 = GetPrimaryTag(loopSquare.P.p);
            return Utility.Surface((tag1.First, tag1.Second), (tag2.First, tag2.Second), (tag3.First, tag3.Second), (tag4.First, tag4.Second));
        }

    }
    public class WholeBoardData
    {
        int size;
        int smallSize;
        TaggedGrid<Component> components;
        (bool, bool, bool, bool) isConnected; //上下左右のつながりを管理
        public Point playerPosition;
        HashSet<LoopSquare> loops; //ループの管理
        public int Size { get => size; }
        public int SmallSize { get => smallSize; }
        public TaggedGrid<Component> Components { get => components; }
        public WholeBoardData(int size, int smallSize, Map map, Point initialPlayerPosition)
        {
            this.size = size;
            this.smallSize = smallSize;
            components = new TaggedGrid<Component>(size, (smallSize, smallSize), TagSolver);
            loops = new HashSet<LoopSquare>();
            this.playerPosition = initialPlayerPosition;
            components.Build(map.mapData);
        }
        public Tag GetPlayerTag()
        {
            return components.GetPrimaryTag(playerPosition);
        }

        readonly Direction[] directions = new Direction[] { Direction.North, Direction.South, Direction.East, Direction.West };


        public List<Point> TagSolver(Tag tag)
        {
            List<Point> points = new List<Point>();
            for (int i = 2 * smallSize * tag.First; i <= 2 * smallSize * (tag.First + 1); i++)
            {
                for (int j = 2 * smallSize * tag.Second; j <= 2 * smallSize * (tag.Second + 1); j++)
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
        public Point GetNextPoint(Point point, Direction dir)
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
            if ((point.X == 1 && dir == Direction.West) || (point.X == size - 1 && dir == Direction.East) || (point.Y == 1 && dir == Direction.North) || (point.Y == size - 1 && dir == Direction.South))
            {
                return Component.Null; //盤面外は壁として扱う
            }
            return components.Get(GetNextPoint(point, dir));
        }
        Component GetNextWall(Point point, Direction dir)
        {
            return components.Get(GetNextWallPoint(point, dir));
        }

        bool IsConnectable(Point p)
        {
            Component c = components.Get(p);
            switch (c.Type)
            {
                case PuzzleComponent.Block:
                case PuzzleComponent.L:
                case PuzzleComponent.O:
                case PuzzleComponent.P:
                case PuzzleComponent.Lp:
                case PuzzleComponent.Op:
                case PuzzleComponent.Pp:
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
            var connectedDirs = ConnectDirections();

            // 今回同時に移動する座標のリスト（プレイヤー自身の座標 ＋ 接続されているブロックの座標）
            List<Point> movingPoints = new List<Point>();
            movingPoints.Add(point);
            foreach (var d in connectedDirs)
            {
                movingPoints.Add(GetNextPoint(point, d));
            }

            // 各移動マスに対して、壁・移動先の状態・タグのルールをチェックする
            foreach (var p in movingPoints)
            {
                // 1. 進行方向の壁チェック
                if (GetNextWall(p, dir).Type != PuzzleComponent.Empty)
                {
                    return false;
                }

                // 2. 移動先のコンポーネント（空きマス）チェック
                Point nextP = GetNextPoint(p, dir);
                Component targetComp = GetNextComponent(p, dir); // 盤面外判定込み

                // 盤面外の場合は移動不可
                if (targetComp.Type == PuzzleComponent.Null)
                {
                    return false;
                }

                // 移動先が空マスではなく、かつ「今回一緒に動く仲間の現在地」でもない場合は移動不可
                // （これにより、押す時や引く時に自分たちを障害物として誤判定しなくなります）
                if (targetComp.Type != PuzzleComponent.Empty && !movingPoints.Contains(nextP))
                {
                    return false;
                }

                // 3. 接続ブロック特有の「タグ」重複チェック（プレイヤー自身は判定から除外）
                if (p != point)
                {
                    Tag preTag = components.GetPrimaryTag(p);
                    Tag nextTag = components.GetPrimaryTag(nextP);

                    // タグが取得できない、または移動によってタグ（エリア）が変わってしまう場合は移動不可
                    if (preTag.First == -1 || nextTag.First == -1 || preTag != nextTag)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void MovePL(Point point, Direction dir, List<IEvent> events)
        {
            (Component? up, Component? down, Component? left, Component? right) Components = (null, null, null, null); //上下左右
            switch (dir)
            {
                case Direction.North:
                    if (ConnectDirections().Contains(Direction.North))
                    {
                        Point northP = GetNextPoint(point, Direction.North);
                        Point nextNorthP = GetNextPoint(northP, Direction.North);
                        components.Exchange(northP, nextNorthP);
                        Components.up = components.Get(nextNorthP);
                    }

                    Point nextP = GetNextPoint(point, Direction.North);
                    components.Exchange(point, nextP);

                    if (ConnectDirections().Contains(Direction.South))
                    {
                        Point southP = GetNextPoint(point, Direction.South);
                        Point nextSouthP = GetNextPoint(southP, Direction.North);
                        components.Exchange(southP, nextSouthP);
                        Components.down = components.Get(nextSouthP);
                    }
                    if (ConnectDirections().Contains(Direction.East))
                    {
                        Point eastP = GetNextPoint(point, Direction.East);
                        Point nextEastP = GetNextPoint(eastP, Direction.North);
                        components.Exchange(eastP, nextEastP);
                        Components.right = components.Get(nextEastP);
                    }
                    if (ConnectDirections().Contains(Direction.West))
                    {
                        Point westP = GetNextPoint(point, Direction.West);
                        Point nextWestP = GetNextPoint(westP, Direction.North);
                        components.Exchange(westP, nextWestP);
                        Components.left = components.Get(nextWestP);
                    }

                    playerPosition = GetNextPoint(point, dir);
                    events.Add(new PLMoveEvent(point, GetNextPoint(point, dir), Components));
                    break;
                case Direction.South:
                    if (ConnectDirections().Contains(Direction.South))
                    {
                        Point southP = GetNextPoint(point, Direction.South);
                        Point nextSouthP = GetNextPoint(southP, Direction.South);
                        components.Exchange(southP, nextSouthP);
                        Components.down = components.Get(nextSouthP);
                    }

                    nextP = GetNextPoint(point, Direction.South);
                    components.Exchange(point, nextP);

                    if (ConnectDirections().Contains(Direction.North))
                    {
                        Point northP = GetNextPoint(point, Direction.North);
                        Point nextNorthP = GetNextPoint(northP, Direction.South);
                        components.Exchange(northP, nextNorthP);
                        Components.up = components.Get(nextNorthP);
                    }
                    if (ConnectDirections().Contains(Direction.East))
                    {
                        Point eastP = GetNextPoint(point, Direction.East);
                        Point nextEastP = GetNextPoint(eastP, Direction.South);
                        components.Exchange(eastP, nextEastP);
                        Components.right = components.Get(nextEastP);
                    }
                    if (ConnectDirections().Contains(Direction.West))
                    {
                        Point westP = GetNextPoint(point, Direction.West);
                        Point nextWestP = GetNextPoint(westP, Direction.South);
                        components.Exchange(westP, nextWestP);
                        Components.left = components.Get(nextWestP);
                    }

                    playerPosition = GetNextPoint(point, dir);
                    events.Add(new PLMoveEvent(point, GetNextPoint(point, dir), Components));
                    break;
                case Direction.East:
                    if (ConnectDirections().Contains(Direction.East))
                    {
                        Point eastP = GetNextPoint(point, Direction.East);
                        Point nextEastP = GetNextPoint(eastP, Direction.East);
                        components.Exchange(eastP, nextEastP);
                        Components.right = components.Get(nextEastP);
                    }

                    nextP = GetNextPoint(point, Direction.East);
                    components.Exchange(point, nextP);

                    if (ConnectDirections().Contains(Direction.North))
                    {
                        Point northP = GetNextPoint(point, Direction.North);
                        Point nextNorthP = GetNextPoint(northP, Direction.East);
                        components.Exchange(northP, nextNorthP);
                        Components.up = components.Get(nextNorthP);
                    }
                    if (ConnectDirections().Contains(Direction.South))
                    {
                        Point southP = GetNextPoint(point, Direction.South);
                        Point nextSouthP = GetNextPoint(southP, Direction.East);
                        components.Exchange(southP, nextSouthP);
                        Components.down = components.Get(nextSouthP);
                    }
                    if (ConnectDirections().Contains(Direction.West))
                    {
                        Point westP = GetNextPoint(point, Direction.West);
                        Point nextWestP = GetNextPoint(westP, Direction.East);
                        components.Exchange(westP, nextWestP);
                        Components.left = components.Get(nextWestP);
                    }

                    playerPosition = GetNextPoint(point, dir);
                    events.Add(new PLMoveEvent(point, playerPosition, Components));
                    break;
                case Direction.West:
                    if (ConnectDirections().Contains(Direction.West))
                    {
                        Point westP = GetNextPoint(point, Direction.West);
                        Point nextWestP = GetNextPoint(westP, Direction.West);
                        components.Exchange(westP, nextWestP);
                        Components.left = components.Get(nextWestP);
                    }

                    nextP = GetNextPoint(point, Direction.West);
                    components.Exchange(point, nextP);

                    if (ConnectDirections().Contains(Direction.North))
                    {
                        Point northP = GetNextPoint(point, Direction.North);
                        Point nextNorthP = GetNextPoint(northP, Direction.West);
                        components.Exchange(northP, nextNorthP);
                        Components.up = components.Get(nextNorthP);
                    }
                    if (ConnectDirections().Contains(Direction.South))
                    {
                        Point southP = GetNextPoint(point, Direction.South);
                        Point nextSouthP = GetNextPoint(southP, Direction.West);
                        components.Exchange(southP, nextSouthP);
                        Components.down = components.Get(nextSouthP);
                    }
                    if (ConnectDirections().Contains(Direction.East))
                    {
                        Point eastP = GetNextPoint(point, Direction.East);
                        Point nextEastP = GetNextPoint(eastP, Direction.West);
                        components.Exchange(eastP, nextEastP);
                        Components.right = components.Get(nextEastP);
                    }

                    playerPosition = GetNextPoint(point, dir);
                    events.Add(new PLMoveEvent(point, playerPosition, Components));
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
                if (p.X < 0 || p.X >= size || p.Y < 0 || p.Y >= size)
                {
                    continue; //範囲外は無視
                }
                Point wallP = GetNextWallPoint(playerPosition, d);
                var tag = components.GetTag(playerPosition);
                var tag2 = components.GetTag(p);
                if (IsConnectable(p) && (tag[0] == tag2[0]) && components.Get(wallP).Type == PuzzleComponent.Empty) //接続可能なコンポーネントは仮実装
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
            return (north, south, west, east);
        }
        public void Connect(List<IEvent> events)
        {
            isConnected = CanConnect();
            if (IsConnecting())
            {
                events.Add(new PLConnectEvent(isConnected));
            }
        }

        public void DisConnect(List<IEvent> events)
        {
            events.Add(new PLDisConnectEvent(isConnected));
            isConnected = (false, false, false, false);
        }


        void LoopProcess(List<IEvent> events) //!ループの再利用周りのバグ直してなさそう
        {
            HashSet<(Point p, Component c, Tag tag)> L = new HashSet<(Point p, Component c, Tag tag)>();
            HashSet<(Point p, Component c, Tag tag)> O = new HashSet<(Point p, Component c, Tag tag)>();
            HashSet<(Point p, Component c, Tag tag)> P = new HashSet<(Point p, Component c, Tag tag)>();

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var p = new Point(x, y);
                    var c = components.Get(p);
                    var tag = components.GetTag(p)[0];
                    switch (c.Type)
                    {
                        case PuzzleComponent.L:
                        case PuzzleComponent.Lp:
                            L.Add((p, c, tag));
                            break;
                        case PuzzleComponent.O:
                        case PuzzleComponent.Op:
                            O.Add((p, c, tag));
                            break;
                        case PuzzleComponent.P:
                        case PuzzleComponent.Pp:
                            P.Add((p, c, tag));
                            break;
                    }
                }
            }
            HashSet<LoopSquare> newRloops = new HashSet<LoopSquare>();
            HashSet<LoopSquare> newLloops = new HashSet<LoopSquare>();
            foreach (var l in L)
            {
                if (l.c.Type == PuzzleComponent.L)
                {
                    var Os = O.Where(o => o.tag == l.tag);
                    var Ps = P.Where(p => (p.tag == l.tag) && (p.p.X == l.p.X || p.p.Y == l.p.Y)); //LとPは同じ行か列にある必要がある
                    var Tuples = from o1 in Os from o2 in Os from p in Ps where o1.p < o2.p select (o1, o2, p);
                    foreach (var t in Tuples)
                    {
                        LoopSquare loopSquare = new LoopSquare((l.p, l.c), (t.o1.p, t.o1.c), (t.o2.p, t.o2.c), (t.p.p, t.p.c));
                        if (loopSquare.IsLoop)
                        {
                            if (loops.Contains(loopSquare))
                            {
                                //どうしようね
                            }
                            else
                            {
                                if (loopSquare.LoopTurn == LoopSquare.Turn.Right)
                                {
                                    newRloops.Add(loopSquare);
                                }
                                else if (loopSquare.LoopTurn == LoopSquare.Turn.Left)
                                {
                                    newLloops.Add(loopSquare);
                                }
                                loops.Add(loopSquare);
                            }
                        }
                    }
                }
                else if (l.c.Type == PuzzleComponent.Lp)
                {
                    var Os = O.Where(o => o.c.Type == PuzzleComponent.Op);
                    var Ps = P.Where(p => p.c.Type == PuzzleComponent.Pp && (p.p.X == l.p.X || p.p.Y == l.p.Y)); //LとPは同じ行か列にある必要がある
                    var Tuples = from o1 in Os from o2 in Os from p in Ps where o1.p < o2.p select (o1, o2, p);
                    foreach (var t in Tuples)
                    {
                        LoopSquare loopSquare = new LoopSquare((l.p, l.c), (t.o1.p, t.o1.c), (t.o2.p, t.o2.c), (t.p.p, t.p.c));
                        if (loopSquare.IsLoop)
                        {
                            if (loops.Contains(loopSquare))
                            {
                                //どうしようね
                            }
                            else
                            {
                                if (loopSquare.LoopTurn == LoopSquare.Turn.Right)
                                {
                                    newRloops.Add(loopSquare);
                                }
                                else if (loopSquare.LoopTurn == LoopSquare.Turn.Left)
                                {
                                    newLloops.Add(loopSquare);
                                }
                                loops.Add(loopSquare);
                            }
                        }
                    }
                }
                if (newRloops.Count == 0 && newLloops.Count == 0)
                {
                    Debug.Log("ループなし");
                    return;
                }
                Debug.Log("ループアリ");
                foreach (var loop in newRloops)
                {
                    Debug.Log(loop.ToString());
                }
                foreach (var loop in newLloops)
                {
                    Debug.Log(loop.ToString());
                }
                var list = newRloops.ToList();
                list.AddRange(newLloops.ToList());
                list = list.OrderBy(l => components.TagArea(l)).ThenBy(a => a.Surface).ThenBy(l => l.L.p).ToList(); //面積⇒タグ面積⇒Lの位置で全順序
                for (int i = 0; i < list.Count; i++)
                {
                    var loop = list[i];
                    var Ltag = components.GetTag(loop.L.p)[0];
                    var O1tag = components.GetTag(loop.O1.p)[0];
                    var O2tag = components.GetTag(loop.O2.p)[0];
                    var Ptag = components.GetTag(loop.P.p)[0];

                    int minTagX = Math.Min(Math.Min(Ltag.First, O1tag.First), Math.Min(O2tag.First, Ptag.First));
                    int maxTagX = Math.Max(Math.Max(Ltag.First, O1tag.First), Math.Max(O2tag.First, Ptag.First));
                    int minTagY = Math.Min(Math.Min(Ltag.Second, O1tag.Second), Math.Min(O2tag.Second, Ptag.Second));
                    int maxTagY = Math.Max(Math.Max(Ltag.Second, O1tag.Second), Math.Max(O2tag.Second, Ptag.Second));

                    var upWalls = new List<(Point, Component)>();
                    var downWalls = new List<(Point, Component)>();
                    var rightWalls = new List<(Point, Component)>();
                    var leftWalls = new List<(Point, Component)>();

                    for (int x = minTagX * 2 * smallSize; x <= maxTagX * 2 * smallSize; x++)
                    {
                        if (components.Get(new Point(x, (maxTagY + 1) * 2 * smallSize)).Type == PuzzleComponent.Wall)
                        {
                            upWalls.Add((new Point(x, (maxTagY + 1) * 2 * smallSize), components.Get(new Point(x, (maxTagY + 1) * 2 * smallSize))));
                        }
                        if (components.Get(new Point(x, minTagY * 2 * smallSize)).Type == PuzzleComponent.Wall)
                        {
                            downWalls.Add((new Point(x, minTagY * 2 * smallSize), components.Get(new Point(x, minTagY * 2 * smallSize))));
                        }
                    }
                    for (int y = minTagY * 2 * smallSize; y <= (maxTagY + 1) * 2 * smallSize; y++)
                    {
                        if (components.Get(new Point((maxTagX + 1) * 2 * smallSize, y)).Type == PuzzleComponent.Wall)
                        {
                            rightWalls.Add((new Point((maxTagX + 1) * 2 * smallSize, y), components.Get(new Point((maxTagX + 1) * 2 * smallSize, y))));
                        }
                        if (components.Get(new Point(minTagX * 2 * smallSize, y)).Type == PuzzleComponent.Wall)
                        {
                            leftWalls.Add((new Point(minTagX * 2 * smallSize, y), components.Get(new Point(minTagX * 2 * smallSize, y))));
                        }
                    }
                    if (loop.LoopTurn == LoopSquare.Turn.Right)
                    {
                        var deltaUp = (maxTagX + 1) * 2 * smallSize - loop.MaxX - 1;
                        var deltaDown = minTagX * 2 * smallSize - loop.MinX + 1;
                        var deltaRight = minTagY * 2 * smallSize - loop.MinY + 1;
                        var deltaLeft = (maxTagY + 1) * 2 * smallSize - loop.MaxY - 1;
                        upWalls = upWalls.OrderByDescending(w => w.Item1.X).ToList();
                        downWalls = downWalls.OrderBy(w => w.Item1.X).ToList();
                        rightWalls = rightWalls.OrderByDescending(w => w.Item1.Y).ToList();
                        leftWalls = leftWalls.OrderBy(w => w.Item1.Y).ToList();
                        //upは左から右に置換
                        //downは右から左に置換
                        //rightは下から上に置換
                        //leftは上から下に置換
                        foreach (var w in upWalls)
                        {
                            var newX = w.Item1.X + deltaUp;
                            var newY = w.Item1.Y;
                            components.Exchange(w.Item1, new Point(newX, newY));
                        }
                        foreach (var w in downWalls)
                        {
                            var newX = w.Item1.X + deltaDown;
                            var newY = w.Item1.Y;
                            components.Exchange(w.Item1, new Point(newX, newY));
                        }
                        foreach (var w in rightWalls)
                        {
                            var newX = w.Item1.X;
                            var newY = w.Item1.Y + deltaRight;
                            components.Exchange(w.Item1, new Point(newX, newY));
                        }
                        foreach (var w in leftWalls)
                        {
                            var newX = w.Item1.X;
                            var newY = w.Item1.Y + deltaLeft;
                            components.Exchange(w.Item1, new Point(newX, newY));
                        }



                    }
                    else if (loop.LoopTurn == LoopSquare.Turn.Left)
                    {
                        var deltaDown = (maxTagX + 1) * 2 * smallSize - loop.MaxX - 1;
                        var deltaUp = minTagX * 2 * smallSize - loop.MinX + 1;
                        var deltaLeft = minTagY * 2 * smallSize - loop.MinY + 1;
                        var deltaRight = (maxTagY + 1) * 2 * smallSize - loop.MaxY - 1;
                        //upは右から左に置換
                        //downは左から右に置換
                        //rightは上から下に置換
                        //leftは下から上に置換
                        Debug.Log($"{upWalls.Count} {downWalls.Count} {rightWalls.Count} {leftWalls.Count}");
                        foreach (var w in upWalls)
                        {
                            var newX = w.Item1.X + deltaUp;
                            var newY = w.Item1.Y;
                            components.Exchange(w.Item1, new Point(newX, newY));
                        }
                        foreach (var w in downWalls)
                        {
                            var newX = w.Item1.X + deltaDown;
                            var newY = w.Item1.Y;
                            components.Exchange(w.Item1, new Point(newX, newY));
                        }
                        foreach (var w in rightWalls)
                        {
                            var newX = w.Item1.X;
                            var newY = w.Item1.Y + deltaRight;
                            components.Exchange(w.Item1, new Point(newX, newY));
                        }
                        foreach (var w in leftWalls)
                        {
                            var newX = w.Item1.X;
                            var newY = w.Item1.Y + deltaLeft;
                            components.Exchange(w.Item1, new Point(newX, newY));
                        }
                    }
                }
            }
        }

        public List<IEvent> Process(InputType input)
        {
            var events = new List<IEvent>();
            if (input != InputType.Connect)
            {
                if (CanPLMove(playerPosition, (Direction)input))
                {
                    MovePL(playerPosition, (Direction)input, events);
                }
            }
            else
            {
                if (IsConnecting())
                {
                    DisConnect(events);
                    LoopProcess(events);
                }
                else
                {
                    Connect(events);
                }
            }
            return events;
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
    public enum EventType
    {
        PLMove,
        PLConnect,
        PLDisConnect,
        LOOPDetect,
        WallMove,
    }

    public enum InputType
    {
        Up,
        Down,
        Right,
        Left,
        Connect
    }

    public interface IEvent
    {
        public EventType Type { get; }
    }

    public class PLMoveEvent : IEvent
    {
        public EventType Type => EventType.PLMove;
        public Point From { get; }
        public Point To { get; }
        public Direction direction
        {
            get
            {
                if (To.X == From.X && To.Y == From.Y - 2) return Direction.North;
                else if (To.X == From.X && To.Y == From.Y + 2) return Direction.South;
                else if (To.X == From.X + 2 && To.Y == From.Y) return Direction.East;
                else if (To.X == From.X - 2 && To.Y == From.Y) return Direction.West;
                else throw new ArgumentException("Invalid move");
            }
        }
        public (Component?, Component?, Component?, Component?) ConnectComponents { get; } //接続しているコンポーネントを管理。上、下、左、右の順番。
        public PLMoveEvent(Point from, Point to, (Component?, Component?, Component?, Component?) connectComponents)
        {
            From = from;
            To = to;
            ConnectComponents = connectComponents;
        }

        public override string ToString()
        {
            return $"PLMoveEvent: From({From.X}, {From.Y}) To({To.X}, {To.Y}) Direction({direction}), ConnectComponents({ConnectComponents.Item1?.Type}, {ConnectComponents.Item2?.Type}, {ConnectComponents.Item3?.Type}, {ConnectComponents.Item4?.Type})";
        }
    }
    public class PLConnectEvent : IEvent
    {
        public EventType Type => EventType.PLConnect;
        public (bool, bool, bool, bool) ConnectDirections { get; }
        public PLConnectEvent((bool, bool, bool, bool) connectDirections)
        {
            ConnectDirections = connectDirections;
        }
    }
    public class PLDisConnectEvent : IEvent
    {
        public (bool, bool, bool, bool) PreConnectDirections { get; }
        public PLDisConnectEvent((bool, bool, bool, bool) connectDirections)
        {
            PreConnectDirections = connectDirections;
        }
        public EventType Type => EventType.PLDisConnect;
    }
    public class LoopDetectEvent : IEvent
    {
        public EventType Type => EventType.LOOPDetect;
        public LoopSquare LoopSquare { get; }
        public LoopDetectEvent(LoopSquare loopSquare)
        {
            LoopSquare = loopSquare;
        }
    }
    public class WallMoveEvent : IEvent //まとめて処理しよかな
    {
        public EventType Type => EventType.WallMove;
        public List<Component> MoveWalls { get; }
        public Direction MoveDirection { get; }
        public int Delta { get; } //ループの中心座標。ル
        public WallMoveEvent(List<Component> moveWalls, Direction moveDirection, int delta)
        {
            MoveWalls = moveWalls;
            MoveDirection = moveDirection;
            Delta = delta;
        }

    }


}
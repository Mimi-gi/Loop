using System;
using System.Collections.Generic;
using System.Linq;

public struct ModInt
{
    int n;
    int mod;

    public ModInt(int n, int mod)
    {
        this.n = n % mod;
        this.mod = mod;
    }

    public static ModInt operator +(ModInt a, ModInt b)
    {
        if (a.mod != b.mod) throw new System.Exception("ModInt同士の演算は同じmodでなければなりません");
        return new ModInt(a.n + b.n, a.mod);
    }

    public static ModInt operator -(ModInt a, ModInt b)
    {
        if (a.mod != b.mod) throw new System.Exception("ModInt同士の演算は同じmodでなければなりません");
        return new ModInt(a.n - b.n, a.mod);
    }

    public static ModInt operator *(ModInt a, ModInt b)
    {
        if (a.mod != b.mod) throw new System.Exception("ModInt同士の演算は同じmodでなければなりません");
        return new ModInt(a.n * b.n, a.mod);
    }

    public static ModInt operator ++(ModInt a)
    {
        return new ModInt(a.n + 1, a.mod);
    }
    public static ModInt operator --(ModInt a)
    {
        return new ModInt(a.n - 1, a.mod);
    }
    public static implicit operator int(ModInt a)
    {
        return a.n;
    }
}

public class SignedMaximTracker
{
    public SignedMaximTracker()
    {
        factorials = new List<int>();
    }
    List<int> factorials;
    public int Value => Helper();
    public void Add(int value)
    {
        factorials.Add(value);
    }
    int Helper()
    {
        if (factorials.Count(x => x < 0) == factorials.Count(x => x > 0)) return 0;
        if (factorials.Count(x => x < 0) > factorials.Count(x => x > 0))
        {
            return factorials.Min();
        }
        else
        {
            return factorials.Max();
        }
    }
}

public class Utility
{
    public static int Surface((int,int)p1,(int,int)p2,(int,int)p3,(int,int)p4)
    {
        var l = new List<(int,int)>{p1,p2,p3,p4};
        l = l.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();
        var ll = new List<(int,int)>{l[0],l[1],l[3],l[2]}; // 左下、左上、右上、右下
        
        // 各辺のベクトル
        var edges = new []
        {
            (ll[1].Item1 - ll[0].Item1, ll[1].Item2 - ll[0].Item2), // 左下 → 左上
            (ll[2].Item1 - ll[1].Item1, ll[2].Item2 - ll[1].Item2), // 左上 → 右上
            (ll[3].Item1 - ll[2].Item1, ll[3].Item2 - ll[2].Item2), // 右上 → 右下
            (ll[0].Item1 - ll[3].Item1, ll[0].Item2 - ll[3].Item2)  // 右下 → 左下
        };
        
        // ①格子上に辺が乗るか（水平または垂直）
        foreach (var edge in edges)
        {
            if (edge.Item1 != 0 && edge.Item2 != 0)
                return -1; // 斜めの辺があれば格子に乗っていない
        }
        
        // ②長方形判定（向かい合う辺の長さが等しい）
        var lengths = edges.Select(e => Math.Abs(e.Item1) + Math.Abs(e.Item2)).ToList();
        if (lengths[0] != lengths[2] || lengths[1] != lengths[3])
            return -1; // 長方形でない
        
        // ③面積を求める（縦 × 横）
        return lengths[0] * lengths[1];
    }
}
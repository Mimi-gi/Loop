using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using LitMotion.Adapters;
using LitMotion;

public struct ModInt
{
    int n;
    int mod;
    public static string ToString(ModInt a)
    {
        return $"{a.n} mod {a.mod}";
    }
    public ModInt(int n, int mod)
    {
        this.n = (n % mod + mod) % mod;
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

public static class Utility
{
    public static int Surface((int, int) p1, (int, int) p2, (int, int) p3, (int, int) p4)
    {
        var l = new List<(int, int)> { p1, p2, p3, p4 };
        l = l.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();
        var ll = new List<(int, int)> { l[0], l[1], l[3], l[2] }; // 左下、左上、右上、右下

        // 各辺のベクトル
        var edges = new[]
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


    public static float Pixelize(this float value,int resolution)
    {
        return Mathf.Round(value * resolution) / resolution;
    }

    public static Vector3 Pixelize(this Vector3 value, int resolution)
    {
        return new Vector3(value.x.Pixelize(resolution), value.y.Pixelize(resolution), value.z.Pixelize(resolution));
    }

    public static Vector2 Pixelize(this Vector2 value, int resolution)
    {
        return new Vector2(value.x.Pixelize(resolution), value.y.Pixelize(resolution));
    }
}


public class AsyncAnimator
{
    Animator _animator;
    public AsyncAnimator(Animator animator)
    {
        _animator = animator;
    }

    public async UniTask PlayAsync(AsyncAnimationClip clip, CancellationToken token = default)
    {
        try
        {
            _animator.Play(clip.Clip.name);
            await UniTask.Delay(TimeSpan.FromSeconds(clip.Length), cancellationToken: token);
        }
        finally
        {
            _animator.Play(clip.Name, 0, 1f);//最後のフレームにしたい
        }
    }
}

public struct AsyncAnimationClip
{
    AnimationClip _clip;
    public AnimationClip Clip => _clip;
    public float Length => _clip.length;
    public string Name => _clip.name;
    public AsyncAnimationClip(AnimationClip clip)
    {
        _clip = clip;
    }
}

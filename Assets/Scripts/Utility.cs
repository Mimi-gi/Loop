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
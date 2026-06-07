using UnityEngine;


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
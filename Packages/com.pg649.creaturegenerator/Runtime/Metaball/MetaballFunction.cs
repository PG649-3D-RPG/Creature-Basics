using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface FalloffFunction
{
    public float Calc(float l2_norm, float R);
    public int GetMinimumNumBalls(float segmentLength, float segmentThickness);
}

public class Polynomial: FalloffFunction
{
    private float exp;
    public Polynomial(float exp = 2f)
    {
        this.exp = exp;
    }

    public float Calc(float l2_norm, float R)
    {
        float r = Mathf.Sqrt(l2_norm);
        return Mathf.Pow(R / r, exp);
    }

    public int GetMinimumNumBalls(float segmentLength, float segmentThickness)
    {
        return Mathf.CeilToInt(segmentLength / (Mathf.Pow(2, 1 / exp) * segmentThickness * 2));
    }
}

public class Perlin : FalloffFunction
{
    private float b;
    public Perlin(float b = 0.5f)
    {
        this.b = b;
    }

    public float Calc(float l2_norm, float R)
    {
        return Mathf.Exp(b - (b * (l2_norm)) / (R * R));
    }

    public int GetMinimumNumBalls(float segmentLength, float segmentThickness)
    {
        float minDist = Mathf.Sqrt(segmentThickness * segmentThickness * (Mathf.Log(2f) + b) / b);
        return Mathf.CeilToInt(segmentLength / (minDist * segmentThickness)) + 1;
    }
}

public static class FalloffFunctions
{
    public static readonly FalloffFunction POLYNOMIAL2 = new Polynomial(2f);
    public static readonly FalloffFunction POLYNOMIAL3 = new Polynomial(3f);
    public static readonly FalloffFunction PERLIN_THICK = new Perlin(0.5f);
    public static readonly FalloffFunction PERLIN_THIN = new Perlin(0.9f);
}
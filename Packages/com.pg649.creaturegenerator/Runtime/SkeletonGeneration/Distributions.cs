using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class NormalDistribution
{
    public float Mean, StandardDeviation;

    public float Min, Max;

    public NormalDistribution(float mean, float standardDeviation, float min, float max)
    {
        Mean = mean;
        StandardDeviation = standardDeviation;
        Min = min;
        Max = max;
    }
    
    private static float BoxMuller()
    {
        var u1 = Random.Range(0.0f, 1.0f);
        var u2 = Random.Range(0.0f, 1.0f);

        return Mathf.Sqrt(-2 * Mathf.Log(u1)) * Mathf.Cos(2 * Mathf.PI * u2);
    }
    
    public float Sample()
    {
        var value = Mean + BoxMuller() * StandardDeviation;
        if (Min > Mean - StandardDeviation && Max < Mean + StandardDeviation || Min > Mean || Max < Mean)
        {
            return MathF.Min(Max, MathF.Max(Min, value));
        }
        else
        {
            do
            {
                value = Mean + BoxMuller() * StandardDeviation;
            } while (value < Min || value > Max);
            return value;
        }
    }

    public List<float> Samples(int n)
    {
        var result = new List<float>();
        for (var i = 0; i < n; i++)
        {
            result.Add(Sample()); 
        }

        return result;
    }
}

[Serializable]
public class UniformIntegerDistribution
{
    public int Min, Max;

    public UniformIntegerDistribution(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public int Sample()
    {
        return Random.Range(Min, Max);
    }
    
    public List<int> Samples(int n)
    {
        var result = new List<int>();
        for (var i = 0; i < n; i++)
        {
            result.Add(Sample()); 
        }

        return result;
    }
}

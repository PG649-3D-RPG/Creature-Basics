using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Contains shared code for creating instances from ParametricCreatureSettings
/// </summary>
public class InstanceUtils
{
    public static float ClampedThickness(float min, float max, float length)
    {
        //return Mathf.Clamp(Random.Range(min, max), 0.0f, 0.5f * length);
        return Mathf.Clamp(Random.Range(min, max), 0.0f, max);
    }

    public static List<float> ClampedLimbThicknesses(float min, float max, List<float> lengths)
    {
        return lengths.ConvertAll(l => ClampedThickness(min, max, l));
    }
}
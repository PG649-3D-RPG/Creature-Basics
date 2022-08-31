using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains shared code for creating instances from ParametricCreatureSettings
/// </summary>
public class InstanceUtils
{
    public static float ClampedThickness(float min, float max, float length)
    {
        return Mathf.Clamp(Random.Range(min, max), 0.0f, 0.5f * length);
    }

    public static List<float> ClampedLimbThicknesses(float min, float max, List<float> lengths)
    {
        return lengths.ConvertAll(l => ClampedThickness(min, max, l));
    }

    public static (float, List<float>, List<float>) InstanceTorso(ParametricCreatureSettings settings)
    {
        var totalTorsoLength = Random.Range(settings.minimumTotalTorsoLength, settings.maximumTotalTorsoLength);
        List<float> torsoSplits = new()
        {
            Random.Range(0.1f * totalTorsoLength, 0.9f * totalTorsoLength)
        };
        do
        {
            var split = Random.Range(0.1f * totalTorsoLength, 0.9f * totalTorsoLength);
            if (Mathf.Abs(split - torsoSplits[0]) >= 0.1f * totalTorsoLength)
                torsoSplits.Add(split);
        } while (torsoSplits.Count < 2);

        torsoSplits.Sort();

        List<float> torsoLengths = new()
        {
            torsoSplits[0],
            torsoSplits[1] - torsoSplits[0],
            totalTorsoLength - torsoSplits[1]
        };
        var torsoThicknesses =
            ClampedLimbThicknesses(settings.minimumTorsoThickness, settings.maximumTorsoThickness, torsoLengths);

        return (totalTorsoLength, torsoLengths, torsoThicknesses);
    }
}
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

    public static (float, List<float>, List<float>, int) InstanceTorso(ParametricCreatureSettings settings)
    {
        var numTorsoSegments = Random.Range(settings.minimumTorsoSegments, settings.maximumTorsoSegments + 1);

        var totalTorsoLength = Random.Range(settings.minimumTotalTorsoLength, settings.maximumTotalTorsoLength);
        List<float> torsoSplits = new();
        while (torsoSplits.Count < numTorsoSegments - 1)
        {
            var split = Random.Range(0.1f * totalTorsoLength, 0.9f * totalTorsoLength);

            // Make sure splits are not too close to each other
            bool tooClose = false;
            foreach (var s in torsoSplits)
            {
                if (Mathf.Abs(split - torsoSplits[0]) < 0.1f * totalTorsoLength)
                {
                    tooClose = true;
                    break;
                }
            }
            if (!tooClose)
                torsoSplits.Add(split);
        }

        torsoSplits.Sort();

        List<float> torsoLengths = new();

        if (numTorsoSegments > 1)
            torsoLengths.Add(torsoSplits[0]);
        else
            torsoLengths.Add(totalTorsoLength);

        for (int i = 1; i < numTorsoSegments - 1; i++)
            torsoLengths.Add(torsoSplits[i] - torsoSplits[i - 1]);

        if (numTorsoSegments > 1)
            torsoLengths.Add(totalTorsoLength - torsoSplits[torsoSplits.Count - 1]);

        var torsoThicknesses =
            ClampedLimbThicknesses(settings.minimumTorsoThickness, settings.maximumTorsoThickness, torsoLengths);

        return (totalTorsoLength, torsoLengths, torsoThicknesses, numTorsoSegments);
    }
}
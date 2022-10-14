using System.Collections.Generic;
using UnityEngine;

public class QuadrupedInstance : ISettingsInstance
{
    [ObservationOrder(0)] public readonly float TotalFrontLegHeight;
    [ObservationOrder(1)] public readonly List<float> FrontLegHeights;
    [ObservationOrder(2)] public readonly List<float> FrontLegThicknesses;

    [ObservationOrder(3)] public readonly float TotalHindLegHeight;
    [ObservationOrder(4)] public readonly List<float> HindLegHeights;
    [ObservationOrder(5)] public readonly List<float> HindLegThicknesses;
    [ObservationOrder(6)] public readonly int NumLegSegments;

    [ObservationOrder(7)] public readonly int NumTorsoSegments;
    [ObservationOrder(8)] public readonly float TotalTorsoLength;
    [ObservationOrder(9)] public readonly List<float> TorsoThicknesses;
    [ObservationOrder(10)] public readonly List<float> TorsoLengths;

    [ObservationOrder(11)] public readonly int NeckBones;
    [ObservationOrder(12)] public readonly float NeckBoneLength;
    [ObservationOrder(13)] public readonly float NeckThickness;

    [ObservationOrder(14)] public readonly float HeadSize;

    [ObservationOrder(15)] public readonly float HipThickness;
    [ObservationOrder(16)] public readonly float HipLength;

    private static (float, List<float>, List<float>) InstanceLeg(ParametricCreatureSettings settings, int numLegSegments)
    {
        var height = Random.Range(settings.minimumTotalLegLength, settings.maximumTotalLegLength);

        List<float> legSplits = new();
        while (legSplits.Count < numLegSegments - 1)
        {
            var split = Random.Range(0.1f * height, 0.9f * height);

            // Make sure splits are not too close to each other
            bool tooClose = false;
            foreach (var s in legSplits)
            {
                if (Mathf.Abs(split - legSplits[0]) < 0.1f * height)
                {
                    tooClose = true;
                    break;
                }
            }
            if (!tooClose)
                legSplits.Add(split);
        }

        legSplits.Sort();

        List<float> heights = new();

        if (numLegSegments > 1)
            heights.Add(legSplits[0]);
        else
            heights.Add(height);

        for (int i = 1; i < numLegSegments - 1; i++)
            heights.Add(legSplits[i] - legSplits[i - 1]);

        if (numLegSegments > 1)
            heights.Add(height - legSplits[legSplits.Count - 1]);

        var thicknesses =
            InstanceUtils.ClampedLimbThicknesses(settings.minimumLegThickness, settings.maximumLegThickness, heights);
        thicknesses.Sort();

        var numSegments = Random.Range(settings.minimumLegSegments, settings.maximumLegSegments + 1);
        return (height, heights, thicknesses);
    }

    public QuadrupedInstance(ParametricCreatureSettings settings, int? seed)
    {
        if (seed.HasValue)
        {
            Random.InitState(seed.Value);
        }

        var numLegSegments = Random.Range(settings.minimumLegSegments, settings.maximumLegSegments + 1);

        var (hindLegHeight, hindLegHeights, hindLegThicknesses) = InstanceLeg(settings, numLegSegments);
        var (frontLegHeight, frontLegHeights, frontLegThicknesses) = InstanceLeg(settings, numLegSegments);
        var (totalTorsoLength, torsoLengths, torsoThicknesses, numTorsoSegments) = InstanceUtils.InstanceTorso(settings);


        var neckBones = Random.Range(settings.minimumNeckBones, settings.maximumNeckBones + 1);
        var totalNeckLength = Random.Range(settings.minimumNeckLength, settings.maximumNeckLength);
        var neckBoneLength = totalNeckLength / neckBones;
        var neckThickness =
            InstanceUtils.ClampedThickness(settings.minimumNeckThickness, settings.maximumNeckThickness, neckBoneLength);

        var headSize = Random.Range(settings.minimumHeadSize, settings.maximumHeadSize);

        var hipLength = Random.Range(settings.minimumHipLength, settings.maximumHipLength);
        var hipThickness = InstanceUtils.ClampedThickness(settings.minimumHipThickness, settings.maximumHipThickness, hipLength);

        NumTorsoSegments = numTorsoSegments;
        TotalTorsoLength = totalTorsoLength;
        TorsoThicknesses = torsoThicknesses;
        TorsoLengths = torsoLengths;
        NeckBones = neckBones;
        NeckBoneLength = neckBoneLength;
        NeckThickness = neckThickness;
        HeadSize = headSize;
        HipLength = hipLength;
        HipThickness = hipThickness;

        NumLegSegments = numLegSegments;
        TotalFrontLegHeight = frontLegHeight;
        FrontLegHeights = frontLegHeights;
        FrontLegThicknesses = frontLegThicknesses;
        TotalHindLegHeight = hindLegHeight;
        HindLegHeights = hindLegHeights;
        HindLegThicknesses = hindLegThicknesses;
    }
}
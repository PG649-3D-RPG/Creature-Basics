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

    [ObservationOrder(6)] public readonly float TotalTorsoLength;
    [ObservationOrder(7)] public readonly List<float> TorsoThicknesses;
    [ObservationOrder(8)] public readonly List<float> TorsoLengths;

    [ObservationOrder(9)] public readonly int NeckBones;
    [ObservationOrder(10)] public readonly float NeckBoneLength;
    [ObservationOrder(11)] public readonly float NeckThickness;

    [ObservationOrder(12)] public readonly float HeadSize;

    [ObservationOrder(13)] public readonly float HipThickness;
    [ObservationOrder(14)] public readonly float HipLength;

    private static (float, List<float>, List<float>) InstanceLeg(ParametricCreatureSettings settings)
    {
        var height = Random.Range(settings.minimumTotalLegLength, settings.maximumTotalLegLength);
        var split = Random.Range(0.1f * height, 0.5f * height);
        var lowerLegSplit = Random.Range(split + 0.25f * (height - split),
            split + 0.75f * (height - split));
        var upperLegSplit = Random.Range(0.25f * split, 0.75f * split);

        List<float> heights = new()
        {
            upperLegSplit, split - upperLegSplit, lowerLegSplit - split,
            height - lowerLegSplit
        };

        var thicknesses =
            InstanceUtils.ClampedLimbThicknesses(settings.minimumLegThickness, settings.maximumLegThickness, heights);
        thicknesses.Sort();
        return (height, heights, thicknesses);
    }

    public QuadrupedInstance(ParametricCreatureSettings settings, int? seed)
    {
        if (seed.HasValue)
        {
            Random.InitState(seed.Value);
        }

        var (hindLegHeight, hindLegHeights, hindLegThicknesses) = InstanceLeg(settings);
        var (frontLegHeight, frontLegHeights, frontLegThicknesses) = InstanceLeg(settings);
        var (totalTorsoLength, torsoLengths, torsoThicknesses) = InstanceUtils.InstanceTorso(settings);

        var neckBones = Random.Range(settings.minimumNeckBones, settings.maximumNeckBones + 1);
        var totalNeckLength = Random.Range(settings.minimumNeckLength, settings.maximumNeckLength);
        var neckBoneLength = totalNeckLength / neckBones;
        var neckThickness =
            InstanceUtils.ClampedThickness(settings.minimumNeckThickness, settings.maximumNeckThickness, neckBoneLength);

        var headSize = Random.Range(settings.minimumHeadSize, settings.maximumHeadSize);

        var hipLength = Random.Range(settings.minimumHipLength, settings.maximumHipLength);
        var hipThickness = InstanceUtils.ClampedThickness(settings.minimumHipThickness, settings.maximumHipThickness, hipLength);

        TotalTorsoLength = totalTorsoLength;
        TorsoThicknesses = torsoThicknesses;
        TorsoLengths = torsoLengths;
        NeckBones = neckBones;
        NeckBoneLength = neckBoneLength;
        NeckThickness = neckThickness;
        HeadSize = headSize;
        HipLength = hipLength;
        HipThickness = hipThickness;

        TotalFrontLegHeight = frontLegHeight;
        FrontLegHeights = frontLegHeights;
        FrontLegThicknesses = frontLegThicknesses;
        TotalHindLegHeight = hindLegHeight;
        HindLegHeights = hindLegHeights;
        HindLegThicknesses = hindLegThicknesses;
    }
}
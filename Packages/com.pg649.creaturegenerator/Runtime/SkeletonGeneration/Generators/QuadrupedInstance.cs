using System.Collections.Generic;
using UnityEngine;

public class QuadrupedInstance : ISettingsInstance
{
    public readonly float TotalFrontLegHeight;
    public readonly List<float> FrontLegHeights;
    public readonly List<float> FrontLegThicknesses;

    public readonly float TotalHindLegHeight;
    public readonly List<float> HindLegHeights;
    public readonly List<float> HindLegThicknesses;

    public readonly float TotalTorsoLength;
    public readonly List<float> TorsoThicknesses;
    public readonly List<float> TorsoLengths;

    public readonly int NeckBones;
    public readonly float NeckBoneLength;
    public readonly float NeckThickness;

    public readonly float HeadSize;

    public readonly float HipThickness;
    public readonly float HipLength;

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
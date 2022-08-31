using System.Collections.Generic;
using UnityEngine;

public class BipedInstance : ISettingsInstance
{
    public readonly float ShoulderThickness;
    public readonly float ShoulderLength;

    public readonly List<float> ArmThicknesses;
    public readonly List<float> ArmLengths;

    public readonly float HandRadius;

    public readonly List<float> LegLengths;
    public readonly List<float> LegThicknesses;

    public readonly float FeetWidth;
    public readonly float FeetLength;

    public readonly List<float> TorsoThicknesses;
    public readonly List<float> TorsoLengths;

    public readonly int NeckBones;
    public readonly float NeckBoneLength;
    public readonly float NeckThickness;

    public readonly float HeadSize;

    public readonly float HipThickness;
    public readonly float HipLength;

    private static (List<float>, List<float>) InstanceLimb(ParametricCreatureSettings settings)
    {
        var totalLength = Random.Range(settings.minimumTotalArmLength, settings.maximumTotalArmLength);
        var split = Random.Range(0.25f * totalLength, 0.75f * totalLength);
        List<float> lengths = new() { split, totalLength - split };
        var thicknesses =
            InstanceUtils.ClampedLimbThicknesses(settings.minimumArmThickness, settings.maximumArmThickness, lengths);
        thicknesses.Sort();
        return (lengths, thicknesses);
    }

    public BipedInstance(ParametricCreatureSettings settings, int? seed)
    {
        if (seed.HasValue) {
            Random.InitState(seed.Value);
        }
        var (armLengths, armThicknesses) = InstanceLimb(settings);
        var (legLengths, legThicknesses) = InstanceLimb(settings);
        var (_, torsoLengths, torsoThicknesses) = InstanceUtils.InstanceTorso(settings);
        
        var neckBones = Random.Range(settings.minimumNeckBones, settings.maximumNeckBones + 1);
        var totalNeckLength = Random.Range(settings.minimumNeckLength, settings.maximumNeckLength);
        var neckBoneLength = totalNeckLength / neckBones;
        var neckThickness =
            InstanceUtils.ClampedThickness(settings.minimumNeckThickness, settings.maximumNeckThickness, neckBoneLength);
        
        var headSize = Random.Range(settings.minimumHeadSize, settings.maximumHeadSize);
        
        var hipLength = Random.Range(settings.minimumHipLength, settings.maximumHipLength);
        var hipThickness = InstanceUtils.ClampedThickness(settings.minimumHipThickness, settings.maximumHipThickness, hipLength);

        var shoulderLength = Random.Range(settings.minimumShoulderLength, settings.maximumShoulderLength);
        var shoulderThickness = InstanceUtils.ClampedThickness(settings.minimumShoulderThickness, settings.maximumShoulderThickness,
            shoulderLength);

        ShoulderLength = shoulderLength;
        ShoulderThickness = shoulderThickness;
        ArmLengths = armLengths;
        ArmThicknesses = armThicknesses;
        HandRadius = Random.Range(settings.minimumHandRadius, settings.maximumHandRadius);
        LegLengths = legLengths;
        LegThicknesses = legThicknesses;
        FeetWidth = Random.Range(settings.minimumFeetWidth, settings.maximumFeetWidth);
        FeetLength = Random.Range(settings.minimumFeetLength, settings.maximumFeetLength);
        TorsoThicknesses = torsoThicknesses;
        TorsoLengths = torsoLengths;
        NeckBones = neckBones;
        NeckBoneLength = neckBoneLength;
        NeckThickness = neckThickness;
        HeadSize = headSize;
        HipLength = hipLength;
        HipThickness = hipThickness;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class BipedInstance : ISettingsInstance
{ 
    [ObservationOrder(0)] public readonly float ShoulderThickness;
    [ObservationOrder(1)] public readonly float ShoulderLength;

    [ObservationOrder(2)] public readonly List<float> ArmThicknesses;
    [ObservationOrder(3)] public readonly List<float> ArmLengths;

    [ObservationOrder(4)] public readonly float HandRadius;

    [ObservationOrder(5)] public readonly List<float> LegLengths;
    [ObservationOrder(6)] public readonly List<float> LegThicknesses;

    [ObservationOrder(7)] public readonly float FeetWidth;
    [ObservationOrder(8)] public readonly float FeetLength;

    [ObservationOrder(9)] public readonly int NumTorsoSegments;
    [ObservationOrder(10)] public readonly List<float> TorsoThicknesses;
    [ObservationOrder(11)] public readonly List<float> TorsoLengths;

    [ObservationOrder(12)] public readonly int NeckBones;
    [ObservationOrder(13)] public readonly float NeckBoneLength;
    [ObservationOrder(14)] public readonly float NeckThickness;

    [ObservationOrder(15)] public readonly float HeadSize;

    [ObservationOrder(16)] public readonly float HipThickness;
    [ObservationOrder(17)] public readonly float HipLength;

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
        var (_, torsoLengths, torsoThicknesses, numTorsoSegments) = InstanceUtils.InstanceTorso(settings);
        
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
        NumTorsoSegments = numTorsoSegments;
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

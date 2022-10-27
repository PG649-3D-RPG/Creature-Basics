using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuadrupedInstance : ISettingsInstance
{
    [ObservationOrder(0)] public readonly float TotalFrontLegHeight;
    [ObservationOrder(1)] public readonly List<float> FrontLegHeights;
    [ObservationOrder(2)] public readonly List<float> FrontLegThicknesses;

    [ObservationOrder(3)] public readonly float TotalHindLegHeight;
    [ObservationOrder(4)] public readonly List<float> HindLegHeights;
    [ObservationOrder(5)] public readonly List<float> HindLegThicknesses;
    [ObservationOrder(15)] public readonly int NumLegBones;

    [ObservationOrder(16)] public readonly int NumTorsoBones;
    [ObservationOrder(6)] public readonly float TotalTorsoLength;
    [ObservationOrder(7)] public readonly List<float> TorsoThicknesses;
    [ObservationOrder(8)] public readonly List<float> TorsoLengths;

    [ObservationOrder(9)] public readonly int NeckBones;
    [ObservationOrder(10)] public readonly float NeckBoneLength;
    [ObservationOrder(11)] public readonly float NeckThickness;

    [ObservationOrder(12)] public readonly float HeadSize;

    [ObservationOrder(13)] public readonly float HipThickness;
    [ObservationOrder(14)] public readonly float HipLength;

    public QuadrupedInstance(ParametricCreatureSettings settings, int? seed)
    {
        if (seed.HasValue)
        {
            Random.InitState(seed.Value);
        }

        var numLegBones = settings.LegBones.Sample();

        var hindLegHeights = settings.LegLength.Samples(numLegBones);
        var hindLegThicknesses = settings.LegThickness.Samples(numLegBones);
        hindLegThicknesses.Sort();
        
        var frontLegHeights = settings.LegLength.Samples(numLegBones);
        var frontLegThicknesses = settings.LegThickness.Samples(numLegBones);
        frontLegThicknesses.Sort();

        var numTorsoBones = settings.TorsoBones.Sample();
        var torsoLengths = settings.TorsoLength.Samples(numTorsoBones);
        var torsoThicknesses = settings.TorsoLength.Samples(numTorsoBones);

        TotalTorsoLength = torsoLengths.Sum();
        TorsoThicknesses = torsoThicknesses;
        NumTorsoBones = numTorsoBones;
        TorsoLengths = torsoLengths;
        NeckBones = settings.NeckBones.Sample();
        NeckBoneLength = settings.NeckLength.Sample();
        NeckThickness = settings.NeckThickness.Sample();
        HeadSize = settings.HeadSize.Sample();
        HipLength = settings.HipLength.Sample();
        HipThickness = settings.HipThickness.Sample();
        TotalFrontLegHeight = frontLegHeights.Sum();
        FrontLegHeights = frontLegHeights;
        FrontLegThicknesses = frontLegThicknesses;
        TotalHindLegHeight = hindLegHeights.Sum();
        HindLegHeights = hindLegHeights;
        HindLegThicknesses = hindLegThicknesses;
        NumLegBones = numLegBones;
    }
}
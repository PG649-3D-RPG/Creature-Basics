using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class QuadrupedGenerator {
    private QuadrupedInstance instance;

    private BoneDefinition neckAttachmentBone;

    private static Dictionary<(BoneCategory, BoneCategory), JointLimits> quadrupedJointLimits = new Dictionary<(BoneCategory, BoneCategory), JointLimits>() {
        {(BoneCategory.Torso, BoneCategory.Torso), new JointLimits { XAxisMin = -10, XAxisMax = 10, YAxisSymmetric = 10 }},
        {(BoneCategory.Torso, BoneCategory.Hip), new JointLimits { XAxisMin = -10, XAxisMax = 10, YAxisSymmetric = 10 }},
        {(BoneCategory.Torso, BoneCategory.Head), new JointLimits() { XAxisMin = -90, XAxisMax = 90, YAxisSymmetric = 45 }},
        {(BoneCategory.Hip, BoneCategory.Leg), new JointLimits { XAxisMin = -90, XAxisMax = 0, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.Leg, BoneCategory.LowerLeg1), new JointLimits { XAxisMin = 0, XAxisMax = 90, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.LowerLeg1, BoneCategory.LowerLeg2), new JointLimits { XAxisMin = -90, XAxisMax = 0, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.LowerLeg2, BoneCategory.Paw), new JointLimits { XAxisMin = 0, XAxisMax = 90, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.LowerLeg1, BoneCategory.Paw), new JointLimits { XAxisMin = 0, XAxisMax = -90, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.Leg, BoneCategory.Paw), new JointLimits { XAxisMin = 0, XAxisMax = 90, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.Hip, BoneCategory.Paw), new JointLimits { XAxisMin = 0, XAxisMax = -90, YAxisSymmetric = 0, ZAxisSymmetric = 0}}
    };

    public SkeletonDefinition BuildCreature(ParametricCreatureSettings settings, int? seed, JointLimitOverrides limitOverrides) {
        instance = new QuadrupedInstance(settings, seed);
        var legs = buildLegs();

        var root = buildTorso();
        attachLegs(legs, root);
        
        var neck = buildNeck(neckAttachmentBone);
        buildHead(neck);

        // Offset root bone so that feet are at ground level
        root.AttachmentHint.Offset = new Vector3(0, instance.TotalHindLegHeight + (instance.TotalHindLegHeight < instance.TotalFrontLegHeight ? 1f: -1f)
            * Mathf.Sqrt(instance.HipLength * instance.HipLength * 0.25f - Mathf.Pow(instance.HipLength* 0.5f * Mathf.Cos(root.AttachmentHint.Rotation.GetValueOrDefault().x), 2f)), 0);

        LimitTable jointLimits = new(quadrupedJointLimits);
        if (limitOverrides != null)
            jointLimits.Add(limitOverrides.ToLimitTable());
        return new SkeletonDefinition(root, jointLimits, instance);
    }

    private List<BoneDefinition> buildLegs() {
        List<BoneDefinition> legs = new()
        {
            buildLeg(instance.HindLegHeights, instance.HindLegThicknesses),
            buildLeg(instance.HindLegHeights, instance.HindLegThicknesses),
            buildLeg(instance.FrontLegHeights, instance.FrontLegThicknesses),
            buildLeg(instance.FrontLegHeights, instance.FrontLegThicknesses)
        };
        return legs;
    }

    private BoneDefinition buildLeg(List<float> lengths, List<float> thicknesses)
    {
        Dictionary<int, (BoneCategory, BoneCategory?)> indexMap = new() {
            { 0, (BoneCategory.Leg, null) },
            { 1, (BoneCategory.Leg, BoneCategory.LowerLeg1) },
            { 2, (BoneCategory.Leg, BoneCategory.LowerLeg2) }
        };
        indexMap[instance.NumLegSegments - 1] = (BoneCategory.Foot, BoneCategory.Paw);

        return GeneratorUtils.BuildLimb(lengths, thicknesses, (length, thickness, index) => new BoneDefinition()
        {
            Length = length,
            Thickness = thickness,
            DistalAxis = Vector3.down,
            VentralAxis = Vector3.forward,
            Category = indexMap[index].Item1,
            SubCategory = indexMap[index].Item2,
            AttachmentHint = new AttachmentHint(),
        }).Item1;
    }

    private BoneDefinition buildTorso()
    {
        var (bottom, top) = GeneratorUtils.BuildLimb(instance.TorsoLengths, instance.TorsoThicknesses, (length, thickness, _) =>
            new BoneDefinition()
            {
                // Construct Torso pointing up, with front side facing forward.
                Length = length,
                Thickness = thickness,
                DistalAxis = Vector3.forward,
                VentralAxis = Vector3.down,
                Category = BoneCategory.Torso,
                AttachmentHint = new AttachmentHint(),
            });
        neckAttachmentBone = top;
        return bottom;
    }
    private BoneDefinition buildNeck(BoneDefinition attachTo) {
        var angle = Random.Range(-90f, 0f);

        var prev = attachTo;
        for (var i = 0; i < instance.NeckBones; i++)
        {
            var neckPart = buildNeckPart(instance.NeckBoneLength, instance.NeckThickness);
            prev.LinkChild(neckPart);
            neckPart.AttachmentHint.Rotation = Quaternion.Euler(angle, 0f, 0f);
            prev = neckPart;
            angle = Random.Range(-20f, 20f);
        }
        return prev;
    }

    private BoneDefinition buildNeckPart(float length, float thickness) {
        // Construct Neck pointing up, with front side facing forward.
        BoneDefinition part = new()
        {
            Length = length,
            DistalAxis = Vector3.up,
            VentralAxis = Vector3.forward,
            Category = BoneCategory.Torso,
            AttachmentHint = new AttachmentHint(),
            Thickness = thickness
        };

        return part;
    }

    private BoneDefinition buildHead(BoneDefinition attachTo) {
        BoneDefinition head = new()
        {
            Length = instance.HeadSize,
            DistalAxis = Vector3.up,
            VentralAxis = Vector3.forward,
            Category = BoneCategory.Head,
            AttachmentHint = new AttachmentHint(),
            Thickness = instance.HeadSize
        };
        attachTo.LinkChild(head);

        return head;
    }

    private void attachLegs(List<BoneDefinition> legs, BoneDefinition torso)
    {
        buildHip(torso, legs[0], legs[1], Vector3.back, Vector3.down, RelativePositions.ProximalPoint);

        var frontHip = buildHip(neckAttachmentBone, legs[2], legs[3], Vector3.forward, Vector3.down, RelativePositions.DistalPoint);
        neckAttachmentBone = frontHip;

        // rotate torso
        var legDiff = instance.TotalHindLegHeight - instance.TotalFrontLegHeight;
        var angle = - Mathf.Atan(legDiff / (instance.TotalTorsoLength + instance.HipLength)) * Mathf.Rad2Deg;
        torso.AttachmentHint.Rotation = Quaternion.Euler(angle, 0.0f, 0.0f);

        foreach (var leg in legs)
        {
            leg.AttachmentHint.Rotation = Quaternion.Euler(angle, 0.0f, 0.0f);
        }
    }
    
    private BoneDefinition buildHip(BoneDefinition attachTo, BoneDefinition leg1, BoneDefinition leg2, Vector3 distalAxis, Vector3 ventralAxis, RelativePosition pos)
    {
        BoneDefinition hip = new()
        {
            Length = instance.HipLength,
            Category = BoneCategory.Hip,
            DistalAxis = distalAxis,
            VentralAxis = ventralAxis,
            Thickness = instance.HipThickness,
            AttachmentHint =
            {
                Position = pos
            }
        };

        attachTo.LinkChild(hip);
        hip.LinkChild(leg1);
        leg1.AttachmentHint.Position = new RelativePosition(1.0f, 0.5f, 0.5f);
        hip.LinkChild(leg2);
        leg2.AttachmentHint.Position = new RelativePosition(-1.0f, 0.5f, 0.5f);
        return hip;
    }
}
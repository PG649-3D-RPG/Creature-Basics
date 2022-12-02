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
        {(BoneCategory.Hip, BoneCategory.FrontLeg), new JointLimits { XAxisMin = -90, XAxisMax = 0, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.FrontLeg, BoneCategory.FrontLeg1), new JointLimits { XAxisMin = 0, XAxisMax = 90, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.FrontLeg1, BoneCategory.FrontLeg2), new JointLimits { XAxisMin = -90, XAxisMax = 0, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.FrontLeg2, BoneCategory.Paw), new JointLimits { XAxisMin = 0, XAxisMax = 90, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.FrontLeg1, BoneCategory.Paw), new JointLimits { XAxisMin = 0, XAxisMax = -90, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.FrontLeg, BoneCategory.Paw), new JointLimits { XAxisMin = 0, XAxisMax = 90, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.Hip, BoneCategory.HindLeg), new JointLimits { XAxisMin = -90, XAxisMax = 0, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.HindLeg, BoneCategory.HindLeg1), new JointLimits { XAxisMin = 0, XAxisMax = 90, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.HindLeg1, BoneCategory.HindLeg2), new JointLimits { XAxisMin = -90, XAxisMax = 0, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.HindLeg2, BoneCategory.Paw), new JointLimits { XAxisMin = 0, XAxisMax = 90, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.HindLeg1, BoneCategory.Paw), new JointLimits { XAxisMin = 0, XAxisMax = -90, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.HindLeg, BoneCategory.Paw), new JointLimits { XAxisMin = 0, XAxisMax = 90, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.Hip, BoneCategory.Paw), new JointLimits { XAxisMin = 0, XAxisMax = -90, YAxisSymmetric = 0, ZAxisSymmetric = 0}}
    };

    public SkeletonDefinition BuildCreature(QuadrupedSettings settings, int? seed, JointLimitOverrides limitOverrides) {
        instance = new QuadrupedInstance(settings, seed);
        var legs = buildLegs();

        var root = buildTorso();
        var hips = attachLegs(legs, root);
        
        var neck = buildNeck(neckAttachmentBone);
        buildHead(neck);

        var legOffset = Mathf.Abs(0.5f * instance.HipThickness * Mathf.Sin((90 - root.AttachmentHint.Rotation.GetValueOrDefault().eulerAngles.x) * Mathf.Deg2Rad));
        // Offset root bone so that feet are at ground level
        root.AttachmentHint.Offset = new Vector3(0, instance.TotalHindLegHeight + legOffset + (instance.TotalHindLegHeight < instance.TotalFrontLegHeight ? 1f : -1f)
            * Mathf.Abs((instance.HipLength * 0.5f + hips.Item1.AttachmentHint.Offset.GetValueOrDefault().magnitude) * Mathf.Sin(root.AttachmentHint.Rotation.GetValueOrDefault().eulerAngles.x * Mathf.Deg2Rad)), 0);

        LimitTable jointLimits = new(quadrupedJointLimits);
        if (limitOverrides != null)
            jointLimits.Add(limitOverrides.ToLimitTable());
        return new SkeletonDefinition(root, jointLimits, instance);
    }

    private List<BoneDefinition> buildLegs() {
        List<BoneDefinition> legs = new()
        {
            buildLeg(instance.HindLegHeights, instance.HindLegThicknesses, true),
            buildLeg(instance.HindLegHeights, instance.HindLegThicknesses, true),
            buildLeg(instance.FrontLegHeights, instance.FrontLegThicknesses, false),
            buildLeg(instance.FrontLegHeights, instance.FrontLegThicknesses, false)
        };
        return legs;
    }

    private BoneDefinition buildLeg(List<float> lengths, List<float> thicknesses, bool isHindLeg)
    {
        Dictionary<int, (BoneCategory, BoneCategory?)> indexMap = null;
        if (isHindLeg)
        {
            indexMap = new()
            {
                { 0, (BoneCategory.Leg, BoneCategory.HindLeg) },
                { 1, (BoneCategory.Leg, BoneCategory.HindLeg1) },
                { 2, (BoneCategory.Leg, BoneCategory.HindLeg2) }
            };
            indexMap[instance.NumHindLegBones - 1] = (BoneCategory.Foot, BoneCategory.Paw);
        }
        else
        {
            indexMap = new()
            {
                { 0, (BoneCategory.Leg, BoneCategory.FrontLeg) },
                { 1, (BoneCategory.Leg, BoneCategory.FrontLeg1) },
                { 2, (BoneCategory.Leg, BoneCategory.FrontLeg2) }
            };
            indexMap[instance.NumFrontLegBones - 1] = (BoneCategory.Foot, BoneCategory.Paw);
        }

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
        var (bottom, top) = GeneratorUtils.BuildLimb(instance.TorsoLengths, instance.TorsoWidths, (length, width, _) =>
            new BoneDefinition()
            {
                // Construct Torso pointing up, with front side facing forward.
                Length = length,
                Thickness = width,
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

    private (BoneDefinition, BoneDefinition) attachLegs(List<BoneDefinition> legs, BoneDefinition torso)
    {
        var backHip = buildHip(torso, legs[0], legs[1], Vector3.back, Vector3.down, RelativePositions.ProximalPoint);
        // Add small offset to hip bone to avoid having two bones with identical positions
        backHip.AttachmentHint.Offset += backHip.DistalAxis * 0.01f;

        var frontHip = buildHip(neckAttachmentBone, legs[2], legs[3], Vector3.forward, Vector3.down, RelativePositions.DistalPoint);
        neckAttachmentBone = frontHip;

        // rotate torso so that both feet are at the same height
        var legDiff = Mathf.Abs(instance.TotalHindLegHeight - instance.TotalFrontLegHeight);
        var torsoLength = instance.TotalTorsoLength + instance.HipLength + backHip.AttachmentHint.Offset.GetValueOrDefault().magnitude;
        var angle = Mathf.Atan(Mathf.Sqrt(torsoLength*torsoLength - legDiff*legDiff) / legDiff) * Mathf.Rad2Deg - 90;
        if (instance.TotalFrontLegHeight > instance.TotalHindLegHeight)
            angle = -angle;

        torso.AttachmentHint.Rotation = Quaternion.Euler(angle, 0.0f, 0.0f);
        foreach (var leg in legs)
        {
            leg.AttachmentHint.Rotation = Quaternion.Euler(angle, 0.0f, 0.0f);
        }
        return (backHip, frontHip);
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

        //avoid overlapping legs
        if (2f * leg1.Thickness > hip.Thickness)
        {
            leg1.AttachmentHint.Position = new RelativePosition(1.5f * leg1.Thickness / hip.Thickness, 0.5f, 0.5f);
            leg2.AttachmentHint.Position = new RelativePosition(-1.5f * leg2.Thickness / hip.Thickness, 0.5f, 0.5f);
        }
        return hip;
    }
}
using System.Collections.Generic;
using System.Linq;
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
        var legs = BuildLegs();

        var root = BuildTorso();
        var hips = AttachLegs(legs, root);
        
        var neck = BuildNeck(neckAttachmentBone);
        BuildHead(neck);

        var legOffset = Mathf.Abs(0.25f * hips.Item1.Thickness * Mathf.Sin((90 - root.AttachmentHint.Rotation.GetValueOrDefault().eulerAngles.x) * Mathf.Deg2Rad));
        // Offset root bone so that feet are at ground level
        Debug.Log(hips.Item1.DistalAxis);
        var rootOffset = instance.TotalHindLegHeight + (hips.Item1.AttachmentHint.Rotation.GetValueOrDefault() * Vector3.up * hips.Item1.Thickness * 0.5f).y
            + (hips.Item1.AttachmentHint.Rotation.GetValueOrDefault() * Vector3.back * hips.Item1.Length * 0.5f).y;
        root.AttachmentHint.Offset = new Vector3(0, rootOffset, 0);

        LimitTable jointLimits = new(quadrupedJointLimits);
        if (limitOverrides != null)
            jointLimits.Add(limitOverrides.ToLimitTable());
        return new SkeletonDefinition(root, jointLimits, instance);
    }

    private List<BoneDefinition> BuildLegs() {
        List<BoneDefinition> legs = new()
        {
            BuildLeg(instance.HindLegHeights, instance.HindLegThicknesses, true),
            BuildLeg(instance.HindLegHeights, instance.HindLegThicknesses, true),
            BuildLeg(instance.FrontLegHeights, instance.FrontLegThicknesses, false),
            BuildLeg(instance.FrontLegHeights, instance.FrontLegThicknesses, false)
        };
        return legs;
    }

    private BoneDefinition BuildLeg(List<float> lengths, List<float> thicknesses, bool isHindLeg)
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

    private BoneDefinition BuildTorso()
    {
        var (bottom, top) = GeneratorUtils.BuildLimb(instance.TorsoLengths, instance.TorsoWidths, (length, width, _) =>
            new BoneDefinition()
            {
                // Construct Torso pointing up, with front side facing forward.
                Length = length,
                Width = width,
                Thickness = width * instance.TorsoRatio,
                DistalAxis = Vector3.forward,
                VentralAxis = Vector3.down,
                Category = BoneCategory.Torso,
                AttachmentHint = new AttachmentHint(),
            });
        neckAttachmentBone = top;
        return bottom;
    }
    private BoneDefinition BuildNeck(BoneDefinition attachTo) {
        var angle = Random.Range(-90f, 0f);

        var prev = attachTo;
        for (var i = 0; i < instance.NeckBones; i++)
        {
            var neckPart = BuildNeckPart(instance.NeckBoneLength, instance.NeckThickness);
            if (i == 0)
                neckPart.AttachmentHint.Position = new RelativePosition(0f, -0.5f, 1f);
            prev.LinkChild(neckPart);
            neckPart.AttachmentHint.Rotation = Quaternion.Euler(angle, 0f, 0f);
            prev = neckPart;
            angle = Random.Range(-20f, 20f);
        }
        return prev;
    }

    private BoneDefinition BuildNeckPart(float length, float thickness) {
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

    private BoneDefinition BuildHead(BoneDefinition attachTo) {
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

    private (BoneDefinition, BoneDefinition) AttachLegs(List<BoneDefinition> legs, BoneDefinition torso)
    {
        var backHip = BuildHip(torso, legs[0], legs[1], Vector3.back, Vector3.down, RelativePositions.ProximalPoint);
        // Add small offset to hip bone to avoid having two bones with identical positions
        backHip.AttachmentHint.Offset += backHip.DistalAxis * 0.01f;

        var frontHip = BuildHip(neckAttachmentBone, legs[2], legs[3], Vector3.forward, Vector3.down, RelativePositions.DistalPoint);
        neckAttachmentBone = frontHip;

        // rotate torso and legs so that both feet are at the same height
        var torsoLength = instance.TotalTorsoLength + frontHip.Length * 0.5f + backHip.Length * 0.5f;
        var angle = 0f;
        if (instance.TotalHindLegHeight  + frontHip.Thickness * 0.5f + backHip.Thickness * 0.5f < instance.TotalFrontLegHeight + frontHip.Thickness * 0.5f + backHip.Thickness * 0.5f)
        {
            if (backHip.Thickness > frontHip.Thickness)
            {
                var alpha = Mathf.Atan((backHip.Thickness - frontHip.Thickness) * 0.5f / (torsoLength)) * Mathf.Rad2Deg;
                var beta = Mathf.Acos((instance.TotalFrontLegHeight - instance.TotalHindLegHeight) / (torsoLength)) * Mathf.Rad2Deg;
                angle = 90 - alpha - beta;
            }
            else
            {
                var alpha = Mathf.Atan((frontHip.Thickness - backHip.Thickness) * 0.5f / (torsoLength));
                var hipConnection = (frontHip.Thickness - backHip.Thickness) * 0.5f / Mathf.Sin(alpha);
                if (alpha.Equals(0))
                    hipConnection = torsoLength;
                alpha *= Mathf.Rad2Deg;
                var beta = Mathf.Asin((instance.TotalFrontLegHeight - instance.TotalHindLegHeight) / hipConnection) * Mathf.Rad2Deg;
                angle = alpha + beta;
            }
        }
        else
        {
            if (frontHip.Thickness > backHip.Thickness)
            {
                var alpha = Mathf.Atan((frontHip.Thickness - backHip.Thickness) * 0.5f / (torsoLength)) * Mathf.Rad2Deg;
                var beta = Mathf.Acos((instance.TotalHindLegHeight - instance.TotalFrontLegHeight) / (torsoLength)) * Mathf.Rad2Deg;
                angle = -90 + alpha + beta;
            }
            else
            {
                var alpha = Mathf.Atan((backHip.Thickness - frontHip.Thickness) * 0.5f / (torsoLength));
                var hipConnection = (backHip.Thickness - frontHip.Thickness) * 0.5f / Mathf.Sin(alpha);
                if (alpha.Equals(0))
                    hipConnection = torsoLength;

                alpha *= Mathf.Rad2Deg;
                var beta = Mathf.Asin((instance.TotalHindLegHeight - instance.TotalFrontLegHeight) / hipConnection) * Mathf.Rad2Deg;
                angle = -alpha - beta;
            }
        }
        foreach (var leg in legs)
            leg.AttachmentHint.Rotation = Quaternion.Euler(angle, 0, 0);
        torso.AttachmentHint.Rotation = Quaternion.Euler(angle, 0, 0);
        return (backHip, frontHip);
    }
    
    private BoneDefinition BuildHip(BoneDefinition attachTo, BoneDefinition leg1, BoneDefinition leg2, Vector3 distalAxis, Vector3 ventralAxis, RelativePosition pos)
    {
        float width = Mathf.Max(instance.TorsoWidths.Average(), leg1.Thickness * 5f);
        BoneDefinition hip = new()
        {
            Length = leg1.Thickness * 2f,
            Category = BoneCategory.Hip,
            DistalAxis = distalAxis,
            VentralAxis = ventralAxis,
            Width = width,
            Thickness = instance.TorsoWidths.Average(),
            AttachmentHint =
            {
                Position = pos
            }
        };

        attachTo.LinkChild(hip);
        hip.LinkChild(leg1);
        leg1.AttachmentHint.Position = new RelativePosition(1f - (leg1.Thickness / (width / 2f)), 0.5f, 0.5f);

        hip.LinkChild(leg2);
        leg2.AttachmentHint.Position = new RelativePosition(-1f + (leg1.Thickness / (width / 2f)), 0.5f, 0.5f);

        return hip;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using System;

using Random = UnityEngine.Random;

enum Mode {
    Biped,
    Quadruped
}

public class ParametricGenerator {
    private CreatureParameters parameters;

    private Mode mode;

    private BoneDefinition neckAttachmentBone;

    private BoneDefinition armAttachmentBone;

    private static readonly Dictionary<(BoneCategory, BoneCategory), JointLimits> HumanoidJointLimits = new Dictionary<(BoneCategory, BoneCategory), JointLimits>() {
        {(BoneCategory.Arm, BoneCategory.Arm), new JointLimits { XAxisMin = -10, XAxisMax = 160, Axis = Vector3.up, SecondaryAxis = Vector3.forward }},
        {(BoneCategory.Torso, BoneCategory.Arm), new JointLimits { XAxisMin = -90, XAxisMax = 160, Axis = Vector3.up, SecondaryAxis = Vector3.forward }},
        {(BoneCategory.Torso, BoneCategory.Torso), new JointLimits { XAxisMin = -10, XAxisMax = 10, YAxisSymmetric = 10 }},
        {(BoneCategory.Torso, BoneCategory.Hip), new JointLimits { XAxisMin = -10, XAxisMax = 10, YAxisSymmetric = 10 }},
        {(BoneCategory.Torso, BoneCategory.Head), new JointLimits() { XAxisMin = -90, XAxisMax = 90, YAxisSymmetric = 45 }},
        {(BoneCategory.Hip, BoneCategory.Leg), new JointLimits() { XAxisMin = -90, XAxisMax = 45, YAxisSymmetric = 45 }},
        {(BoneCategory.Leg, BoneCategory.Leg), new JointLimits() { XAxisMin = -10, XAxisMax = 90 }},
        {(BoneCategory.Arm, BoneCategory.Hand), new JointLimits() { XAxisMin = -180, XAxisMax = 180, YAxisSymmetric = 45, ZAxisSymmetric = 90, Axis = Vector3.forward }},
        {(BoneCategory.Leg, BoneCategory.Foot), new JointLimits() { XAxisMin = -10, XAxisMax = 10, YAxisSymmetric = 45 }}
    };


    public ParametricGenerator(CreatureParameters parameters) {
        this.parameters = parameters;
    }

    public SkeletonDefinition BuildCreature(int seed = 0) {
        if (seed != 0) {
            Random.InitState(seed);
        }
        int pairs = Random.Range(parameters.minLegPairs, parameters.maxLegPairs + 1);
        if (pairs == 1) {
            mode = Mode.Biped;
        } else {
            mode = Mode.Quadruped;
        }

        List<BoneDefinition> legs = buildLegs();
        List<BoneDefinition> arms = buildArms();

        BoneDefinition root = buildTorso();
        BoneDefinition neck = buildNeck(neckAttachmentBone);
        buildHead(neck);

        attachLegs(legs, root);

        foreach (var arm in arms)
            armAttachmentBone.LinkChild(arm);

        return new SkeletonDefinition(root, new LimitTable(HumanoidJointLimits));
    }

    private List<BoneDefinition> buildArms() {
        List<BoneDefinition> arms = new();
        if (mode == Mode.Biped) {
            float armHeight = Random.Range(parameters.minArmSize, parameters.maxArmSize);
            float armSplit = Random.Range(0.25f * armHeight, 0.75f * armHeight);
            //legHeights = new List<float>() { legHeight };

            List<float> heights = new() {armSplit, armHeight - armSplit};

            List<float> thicknesses = new();
            thicknesses.Add(Random.Range(parameters.minArmThickness, parameters.maxArmThickness));
            thicknesses.Add(Random.Range(parameters.minArmThickness, parameters.maxArmThickness));
            thicknesses.Sort();

            BoneDefinition leftRoot = buildArm(heights, thicknesses, false);
            BoneDefinition rightRoot = buildArm(heights, thicknesses, true);

            leftRoot.AttachmentHint.Position = new RelativePosition(1.0f, 0.0f, 0.5f);
            leftRoot.AttachmentHint.VentralDirection = Vector3.right;
            leftRoot.AttachmentHint.Rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

            rightRoot.AttachmentHint.Position = new RelativePosition(-1.0f, 0.0f, 0.5f);
            rightRoot.AttachmentHint.VentralDirection = Vector3.left;
            rightRoot.AttachmentHint.Rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

            arms.Add(leftRoot);
            arms.Add(rightRoot);
        }
        return arms;
    }

    private BoneDefinition buildArm(List<float> lengths, List<float> thicknesses, bool mirrored = false) {
        Assert.AreEqual(lengths.Count, thicknesses.Count);
        Assert.IsTrue(lengths.Count > 0);

        BoneDefinition root = buildArmPart(lengths[0], thicknesses[0], mirrored);
        root.Mirrored = mirrored;
        BoneDefinition prev = root;

        foreach (var (length, thickness) in lengths.Zip(thicknesses, (a, b) => (a, b)).Skip(1)) {
            BoneDefinition part = buildArmPart(length, thickness, mirrored);
            prev.LinkChild(part);
            prev = part;
        }

        BoneDefinition hand = new();
        hand.DistalAxis = Vector3.down;
        hand.VentralAxis = Vector3.forward;
        hand.Length = 0.5f;
        hand.Thickness = 0.5f;
        hand.Category = BoneCategory.Hand;
        hand.AttachmentHint = new();

        prev.LinkChild(hand);

        return root;
    }

    private BoneDefinition buildArmPart(float length, float thickness, bool mirrored) {
        BoneDefinition part = new();

        part.Length = length;
        part.DistalAxis = Vector3.down;
        part.VentralAxis = Vector3.forward;
        part.Category = BoneCategory.Arm;
        part.Thickness = thickness;
        part.Mirrored = mirrored;

        return part;
    }

    private List<BoneDefinition> buildLegs() {
        List<BoneDefinition> legs = new();
        if (mode == Mode.Biped) {
            float legHeight = Random.Range(parameters.minLegSize, parameters.maxLegSize);
            float legSplit = Random.Range(0.25f * legHeight, 0.75f * legHeight);
            //legHeights = new List<float>() { legHeight };

            List<float> heights = new() {legSplit, legHeight - legSplit};

            List<float> thicknesses = new();
            thicknesses.Add(Random.Range(parameters.minLegThickness, parameters.maxLegThickness));
            thicknesses.Add(Random.Range(parameters.minLegThickness, parameters.maxLegThickness));
            thicknesses.Sort();

            BoneDefinition leftLeg = buildLeg(heights, thicknesses);
            BoneDefinition rightLeg = buildLeg(heights, thicknesses);

            legs.Add(leftLeg);
            legs.Add(rightLeg);
        }
        // TODO: Port quadruped code
        else if (mode == Mode.Quadruped)
        {
            // quadruped
            float frontLegHeight = Random.Range(parameters.minLegSize, parameters.maxLegSize);
            float hindLegHeight = Random.Range(parameters.minLegSize, parameters.maxLegSize);
            //legHeights = new List<float>() { hindLegHeight, frontLegHeight };

            for (int i = 0; i < 2; i++)
            {
                float height = hindLegHeight;
                if (i == 1)
                    height = frontLegHeight;
                float legSplit = Random.Range(0.1f * height, 0.5f * height);
                float lowerLegSplit = Random.Range(legSplit + 0.25f * (height - legSplit), legSplit + 0.75f * (height - legSplit));
                float upperLegSplit = Random.Range(0.25f * legSplit, 0.75f * legSplit);

                List<float> heights = new() {upperLegSplit, legSplit-upperLegSplit, lowerLegSplit-legSplit, height-lowerLegSplit};

                List<float> thicknesses = new();

                thicknesses.Add(Random.Range(parameters.minLegThickness, parameters.maxLegThickness));
                thicknesses.Add(Random.Range(parameters.minLegThickness, parameters.maxLegThickness));
                thicknesses.Add(Random.Range(parameters.minLegThickness, parameters.maxLegThickness));
                thicknesses.Add(Random.Range(parameters.minLegThickness, parameters.maxLegThickness));
                thicknesses.Sort();

                for (int j = -1; j < 2; j+=2)
                {
                    BoneDefinition leg = buildLeg(heights, thicknesses);
                    legs.Add(leg);
                }
            }
        }
        return legs;
    }

    private BoneDefinition buildLeg(List<float> lengths, List<float> thicknesses) {
        Assert.AreEqual(lengths.Count, thicknesses.Count);
        Assert.IsTrue(lengths.Count > 0);

        BoneDefinition root = buildLegPart(lengths[0], thicknesses[0]);
        BoneDefinition prev = root;

        foreach (var (length, thickness) in lengths.Zip(thicknesses, (a, b) => (a, b)).Skip(1)) {
            BoneDefinition part = buildLegPart(length, thickness);
            prev.LinkChild(part);
            prev = part;
        }

        if (mode == Mode.Biped)
        {
            BoneDefinition foot = new();
            foot.DistalAxis = Vector3.forward;
            foot.VentralAxis = Vector3.up;
            foot.Length = 2.0f;
            foot.Thickness = 0.5f;
            foot.Category = BoneCategory.Foot;
            foot.AttachmentHint = new();
            foot.AttachmentHint.Offset = new Vector3(0.0f, 0.0f, -0.5f);

            prev.LinkChild(foot);
        }
        return root;
    }

    private BoneDefinition buildLegPart(float length, float thickness) {
        BoneDefinition part = new();

        part.Length = length;
        // Construct Legs pointing down, with front side facing forward.
        part.DistalAxis = Vector3.down;
        part.VentralAxis = Vector3.forward;
        part.Category = BoneCategory.Leg;
        part.AttachmentHint = new();
        part.Thickness = thickness;

        return part;
    }

    private BoneDefinition buildTorso() {
        float torsoSize = Random.Range(parameters.minTorsoSize, parameters.maxTorsoSize);

        List<float> torsoSplits = new List<float>()
        {
            Random.Range(0.1f * torsoSize, 0.9f * torsoSize)
        };
        do
        {
            float split = Random.Range(0.1f * torsoSize, 0.9f * torsoSize);
            if (Mathf.Abs(split - torsoSplits[0]) >= 0.1f * torsoSize)
                torsoSplits.Add(split);
        } while (torsoSplits.Count < 2);
        torsoSplits.Sort();

        if(mode == Mode.Biped || mode == Mode.Quadruped) {
            float thicknessBottom = Random.Range(parameters.minTorsoThickness, parameters.maxTorsoThickness);
            float thicknessMiddle = Random.Range(parameters.minTorsoThickness, parameters.maxTorsoThickness);
            float thicknessTop = Random.Range(parameters.minTorsoThickness, parameters.maxTorsoThickness);
            BoneDefinition bottom = buildTorsoPart(torsoSplits[0], thicknessBottom);
            BoneDefinition middle = buildTorsoPart(torsoSplits[1] - torsoSplits[0], thicknessMiddle);
            BoneDefinition top = buildTorsoPart(torsoSize - torsoSplits[1], thicknessTop);

            bottom.LinkChild(middle);
            middle.LinkChild(top);

            neckAttachmentBone = top;
            armAttachmentBone = top;

            return bottom;
        }
        return null;
    }

    private BoneDefinition buildTorsoPart(float length, float thickness) {
        BoneDefinition part = new();

        part.Length = length;
        // Construct Torso pointing up, with front side facing forward.
        if (mode == Mode.Biped)
        {
            part.DistalAxis = Vector3.up;
            part.VentralAxis = Vector3.forward;
        }
        else if (mode == Mode.Quadruped)
        {
            part.DistalAxis = Vector3.forward;
            part.VentralAxis = Vector3.down;
        }
        part.Category = BoneCategory.Torso;
        part.AttachmentHint = new();
        part.Thickness = thickness;

        return part;
    }

    private BoneDefinition buildNeck(BoneDefinition attachTo) {
        int neckSegments = Random.Range(parameters.minNeckSegments, parameters.maxNeckSegments + 1);
        float neckSize = Random.Range(parameters.minNeckSize, parameters.maxNeckSize);
        float segmentLength = neckSize / neckSegments;
        float neckThickness = 0.2f;

        //Vector3 fwd = 0.5f * ((torso[2].endPoint - torso[2].startPoint).normalized + Vector3.up);

        BoneDefinition prev = attachTo;
        for (int i=0; i<neckSegments; i++)
        {
            BoneDefinition neckPart = buildNeckPart(segmentLength, neckThickness);
            prev.LinkChild(neckPart);
            prev = neckPart;
        }
        return prev;
    }

    private BoneDefinition buildNeckPart(float length, float thickness) {
        BoneDefinition part = new();

        part.Length = length;
        // Construct Neck pointing up, with front side facing forward.
        part.DistalAxis = Vector3.up;
        part.VentralAxis = Vector3.forward;
        part.Category = BoneCategory.Torso;
        part.AttachmentHint = new();
        part.Thickness = thickness;

        return part;
    }

    private BoneDefinition buildHead(BoneDefinition attachTo) {
        float headSize = Random.Range(parameters.minHeadSize, parameters.maxHeadSize);

        BoneDefinition head = new();
        attachTo.LinkChild(head);

        head.Length = headSize;
        head.DistalAxis = Vector3.up;
        head.VentralAxis = Vector3.forward;
        head.Category = BoneCategory.Head;
        head.AttachmentHint = new();
        head.Thickness = headSize;

        return head;
    }

    private void attachLegs(List<BoneDefinition> legs, BoneDefinition torso)
    {
        if (mode == Mode.Quadruped)
        {
            buildHip(torso, legs[0], Vector3.left, RelativePositions.DistalPoint);
            buildHip(torso, legs[1], Vector3.right, RelativePositions.DistalPoint);

            buildHip(neckAttachmentBone, legs[2], Vector3.left, RelativePositions.DistalPoint);
            buildHip(neckAttachmentBone, legs[3], Vector3.right, RelativePositions.DistalPoint);
        } else
        {
            BoneDefinition hip = new();
            hip.Length = 1.25f;
            hip.DistalAxis = Vector3.down;
            hip.VentralAxis = Vector3.forward;
            hip.Category = BoneCategory.Hip;
            hip.Thickness = torso.Thickness;
            hip.AttachmentHint.Position = RelativePositions.ProximalPoint;

            torso.LinkChild(hip);

            hip.LinkChild(legs[0]);
            legs[0].AttachmentHint.Position = new RelativePosition(1.0f, 0.0f, 1.0f);

            hip.LinkChild(legs[1]);
            legs[1].AttachmentHint.Position = new RelativePosition(-1.0f, 0.0f, 1.0f);
        }
    }
    
    private BoneDefinition buildHip(BoneDefinition attachTo, BoneDefinition leg, Vector3 proximalAxis, RelativePosition pos)
    {
        BoneDefinition hip = new();
        hip.Length = attachTo.Thickness;
        hip.DistalAxis = proximalAxis;
        hip.VentralAxis = Vector3.forward;
        hip.Category = BoneCategory.Hip;
        hip.Thickness = 0f; // For now to avoid metaball generation around hip
        hip.AttachmentHint.Position = pos;
        attachTo.LinkChild(hip);
        hip.LinkChild(leg);
        return hip;
    }
}
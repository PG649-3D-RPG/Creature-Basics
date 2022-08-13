using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

using Random = UnityEngine.Random;


public class ParametricGenerator {
    public enum Mode {
        Biped,
        Quadruped
    }

    private struct BipedSettingsInstance
    {
        public float ShoulderThickness;
        public float ShoulderLength;
        
        public List<float> ArmThicknesses;
        public List<float> ArmLengths;
        
        public float HandRadius;
        
        public List<float> LegLengths;
        public List<float> LegThicknesses;
        
        public float FeetWidth;
        public float FeetLength;
        
        public List<float> TorsoThicknesses;
        public List<float> TorsoLengths;

        public int NeckBones;
        public float NeckBoneLength;
        public float NeckThickness;

        public float HeadSize;

        public float HipThickness;
        public float HipLength;
    }
    
    private ParametricCreatureSettings settings;

    private BipedSettingsInstance bipedSettings;

    private Mode mode;

    private BoneDefinition neckAttachmentBone;

    private BoneDefinition armAttachmentBone;

    private float hindLegHeight;

    private float frontLegHeight;

    private float torsoSize;

    private static readonly Dictionary<(BoneCategory, BoneCategory), JointLimits> HumanoidJointLimits = new Dictionary<(BoneCategory, BoneCategory), JointLimits>() {
        {(BoneCategory.Arm, BoneCategory.Arm), new JointLimits { XAxisMin = -10, XAxisMax = 160, Axis = Vector3.up, SecondaryAxis = Vector3.forward }},
        {(BoneCategory.Shoulder, BoneCategory.Arm), new JointLimits { XAxisMin = -90, XAxisMax = 160, ZAxisSymmetric = 90, Axis = Vector3.up, SecondaryAxis = Vector3.forward }},
        {(BoneCategory.Torso, BoneCategory.Torso), new JointLimits { XAxisMin = -10, XAxisMax = 10, YAxisSymmetric = 10 }},
        {(BoneCategory.Torso, BoneCategory.Hip), new JointLimits { XAxisMin = -10, XAxisMax = 10, YAxisSymmetric = 10 }},
        {(BoneCategory.Torso, BoneCategory.Head), new JointLimits() { XAxisMin = -90, XAxisMax = 90, YAxisSymmetric = 45 }},
        {(BoneCategory.Hip, BoneCategory.Leg), new JointLimits() { XAxisMin = -45, XAxisMax = 120, YAxisSymmetric = 45 }},
        {(BoneCategory.Leg, BoneCategory.Leg), new JointLimits() { XAxisMin = -90, XAxisMax = 10 }},
        {(BoneCategory.Arm, BoneCategory.Hand), new JointLimits() { XAxisMin = -180, XAxisMax = 180, YAxisSymmetric = 45, ZAxisSymmetric = 90, Axis = Vector3.forward }},
        {(BoneCategory.Leg, BoneCategory.Foot), new JointLimits() { XAxisMin = -10, XAxisMax = 10, YAxisSymmetric = 45 }}
    };

    private static Dictionary<(BoneCategory, BoneCategory), JointLimits> quadrupedJointLimits = new Dictionary<(BoneCategory, BoneCategory), JointLimits>() {
        {(BoneCategory.Torso, BoneCategory.Torso), new JointLimits { XAxisMin = -10, XAxisMax = 10, YAxisSymmetric = 10 }},
        {(BoneCategory.Torso, BoneCategory.Hip), new JointLimits { XAxisMin = -10, XAxisMax = 10, YAxisSymmetric = 10 }},
        {(BoneCategory.Torso, BoneCategory.Head), new JointLimits() { XAxisMin = -90, XAxisMax = 90, YAxisSymmetric = 45 }},
        {(BoneCategory.Hip, BoneCategory.Leg), new JointLimits { XAxisMin = -90, XAxisMax = 0, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.Leg, BoneCategory.LowerLeg1), new JointLimits { XAxisMin = 0, XAxisMax = 90, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.LowerLeg1, BoneCategory.LowerLeg2), new JointLimits { XAxisMin = -90, XAxisMax = 0, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.LowerLeg2, BoneCategory.Leg), new JointLimits { XAxisMin = 0, XAxisMax = 90, YAxisSymmetric = 0, ZAxisSymmetric = 0}}
    };

    private static BipedSettingsInstance RollBipedSettings(ParametricCreatureSettings settings)
    {
        var totalArmLength = Random.Range(settings.minimumTotalArmLength, settings.maximumTotalArmLength);
        var armSplit = Random.Range(0.25f * totalArmLength, 0.75f * totalArmLength);
        List<float> armLengths = new() { armSplit, totalArmLength - armSplit };
        List<float> armThicknesses = new()
        {
            Random.Range(settings.minimumArmThickness, settings.maximumArmThickness),
            Random.Range(settings.minimumArmThickness, settings.maximumArmThickness)
        };
        armThicknesses.Sort();
        
        var totalLegLength = Random.Range(settings.minimumTotalLegLength, settings.maximumTotalLegLength);
        var legSplit = Random.Range(0.25f * totalLegLength, 0.75f * totalLegLength);
        List<float> legLengths = new() {legSplit, totalLegLength - legSplit};
        List<float> legThicknesses = new()
        {
            Random.Range(settings.minimumLegThickness, settings.maximumLegThickness),
            Random.Range(settings.minimumLegThickness, settings.maximumLegThickness)
        };
        legThicknesses.Sort();
        
        var totalTorsoLength = Random.Range(settings.minimumTotalTorsoLength, settings.maximumTotalTorsoLength);

        List<float> torsoSplits = new()
        {
            Random.Range(0.1f * totalTorsoLength, 0.9f * totalTorsoLength)
        };
        do
        {
            var split = Random.Range(0.1f * totalTorsoLength, 0.9f * totalTorsoLength);
            if (Mathf.Abs(split - torsoSplits[0]) >= 0.1f * totalTorsoLength)
                torsoSplits.Add(split);
        } while (torsoSplits.Count < 2);
        torsoSplits.Sort();

        List<float> torsoLengths = new()
        {
            torsoSplits[0],
            torsoSplits[1] - torsoSplits[0],
            totalTorsoLength - torsoSplits[1]
        };
        List<float> torsoThicknesses = new()
        {
            Random.Range(settings.minimumTorsoThickness, settings.maximumTorsoThickness),
            Random.Range(settings.minimumTorsoThickness, settings.maximumTorsoThickness),
            Random.Range(settings.minimumTorsoThickness, settings.maximumTorsoThickness)
        };

        var neckBones = Random.Range(settings.minimumNeckBones, settings.maximumNeckBones + 1);
        var totalNeckLength = Random.Range(settings.minimumNeckLength, settings.maximumNeckLength);
        var neckBoneLength = totalNeckLength / neckBones;
        var neckThickness = Random.Range(settings.minimumNeckThickness, settings.maximumNeckThickness);
        
        var headSize = Random.Range(settings.minimumHeadSize, settings.maximumHeadSize);
        
        var hipLength = Random.Range(settings.minimumHipLength, settings.maximumHipLength);
        var hipThickness = Random.Range(settings.minimumHipThickness, settings.maximumHipThickness);
        
        return new BipedSettingsInstance
        {
            ShoulderThickness = Random.Range(settings.minimumShoulderThickness, settings.maximumShoulderThickness),
            ShoulderLength = Random.Range(settings.minimumShoulderLength, settings.maximumShoulderLength),
            ArmLengths = armLengths,
            ArmThicknesses = armThicknesses,
            HandRadius = Random.Range(settings.minimumHandRadius, settings.maximumHandRadius),
            LegLengths = legLengths,
            LegThicknesses = legThicknesses,
            FeetWidth = Random.Range(settings.minimumFeetWidth, settings.maximumFeetWidth),
            FeetLength = Random.Range(settings.minimumFeetLength, settings.maximumFeetLength),
            TorsoThicknesses = torsoThicknesses,
            TorsoLengths = torsoLengths,
            NeckBones = neckBones,
            NeckBoneLength = neckBoneLength,
            NeckThickness = neckThickness,
            HeadSize = headSize,
            HipLength = hipLength,
            HipThickness = hipThickness,
        };
    }

    public ParametricGenerator(ParametricCreatureSettings settings) {
        this.settings = settings;
        this.bipedSettings = RollBipedSettings(settings);
    }

    public SkeletonDefinition BuildCreature(Mode mode, int seed = 0) {
        if (seed != 0) {
            Random.InitState(seed);
        }

        this.mode = mode;
        List<BoneDefinition> legs = buildLegs();
        List<BoneDefinition> arms = buildArms();

        BoneDefinition root = buildTorso();
        attachLegs(legs, root);

        BoneDefinition neck = buildNeck(neckAttachmentBone);
        buildHead(neck);


        foreach (var arm in arms)
            armAttachmentBone.LinkChild(arm);

        if (mode == Mode.Biped)
            return new SkeletonDefinition(root, new LimitTable(HumanoidJointLimits));
        else
            return new SkeletonDefinition(root, new LimitTable(quadrupedJointLimits));
    }

    private List<BoneDefinition> buildArms() {
        List<BoneDefinition> arms = new();
        if (mode == Mode.Biped) {
            var leftRoot = buildArm(bipedSettings.ArmLengths, bipedSettings.ArmThicknesses, false);
            leftRoot.AttachmentHint.VentralDirection = Vector3.right;
            
            var rightRoot = buildArm(bipedSettings.ArmLengths, bipedSettings.ArmThicknesses, true);
            rightRoot.AttachmentHint.VentralDirection = Vector3.left;

            var leftShoulder = buildShoulder();
            leftShoulder.LinkChild(leftRoot);
            leftShoulder.AttachmentHint.Position = new RelativePosition(0.7f, 0.0f, 0.5f);
            leftShoulder.AttachmentHint.Rotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
            
            var rightShoulder = buildShoulder();
            rightShoulder.LinkChild(rightRoot);
            rightShoulder.AttachmentHint.Position = new RelativePosition(-0.7f, 0.0f, 0.5f);
            rightShoulder.AttachmentHint.Rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);

            arms.Add(leftShoulder);
            arms.Add(rightShoulder);
        }
        return arms;
    }

    private BoneDefinition buildShoulder()
    {
        return new BoneDefinition
        {
            Length = bipedSettings.ShoulderLength,
            DistalAxis = Vector3.down,
            VentralAxis = Vector3.forward,
            Category = BoneCategory.Shoulder,
            Thickness = bipedSettings.ShoulderThickness,
            Mirrored = false
        };
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

        BoneDefinition hand = new()
        {
            DistalAxis = Vector3.down,
            VentralAxis = Vector3.forward,
            Length = bipedSettings.HandRadius,
            Thickness = bipedSettings.HandRadius,
            Category = BoneCategory.Hand,
            AttachmentHint = new AttachmentHint()
        };

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

            BoneDefinition leftLeg = buildLeg(bipedSettings.LegLengths, bipedSettings.LegThicknesses);
            BoneDefinition rightLeg = buildLeg(bipedSettings.LegLengths, bipedSettings.LegThicknesses);

            legs.Add(leftLeg);
            legs.Add(rightLeg);
        }
        // TODO: Port quadruped code
        else if (mode == Mode.Quadruped)
        {
            // quadruped
            frontLegHeight = Random.Range(settings.minimumTotalLegLength, settings.maximumTotalLegLength);
            hindLegHeight = Random.Range(settings.minimumTotalLegLength, settings.maximumTotalLegLength);
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

                thicknesses.Add(Random.Range(settings.minimumLegThickness, settings.maximumLegThickness));
                thicknesses.Add(Random.Range(settings.minimumLegThickness, settings.maximumLegThickness));
                thicknesses.Add(Random.Range(settings.minimumLegThickness, settings.maximumLegThickness));
                thicknesses.Add(Random.Range(settings.minimumLegThickness, settings.maximumLegThickness));
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

        for (int i=1; i<thicknesses.Count; i++) {
            BoneDefinition part = buildLegPart(lengths[i], thicknesses[i], i);
            prev.LinkChild(part);
            prev = part;
        }

        if (mode == Mode.Biped)
        {
            BoneDefinition foot = new()
            {
                DistalAxis = Vector3.forward,
                VentralAxis = Vector3.up,
                Length = bipedSettings.FeetLength,
                Thickness = bipedSettings.FeetWidth,
                Category = BoneCategory.Foot,
                AttachmentHint = new AttachmentHint
                {
                    Offset = new Vector3(0.0f, 0.0f, -0.5f)
                }
            };

            prev.LinkChild(foot);
        }
        return root;
    }

    private BoneDefinition buildLegPart(float length, float thickness, int index=0) {
        BoneDefinition part = new();

        part.Length = length;
        // Construct Legs pointing down, with front side facing forward.
        part.DistalAxis = Vector3.down;
        part.VentralAxis = Vector3.forward;
        part.Category = BoneCategory.Leg;
        part.AttachmentHint = new();
        part.Thickness = thickness;

        if (mode == Mode.Quadruped)
        {
            if (index == 1)
                part.SubCategory = BoneCategory.LowerLeg1;
            else if (index == 2)
                part.SubCategory = BoneCategory.LowerLeg2;
        }

        return part;
    }

    private BoneDefinition buildTorso() {
        var bottom = buildTorsoPart(bipedSettings.TorsoLengths[0], bipedSettings.TorsoThicknesses[0]);
        var middle = buildTorsoPart(bipedSettings.TorsoLengths[1], bipedSettings.TorsoThicknesses[1]);
        var top = buildTorsoPart(bipedSettings.TorsoLengths[2], bipedSettings.TorsoThicknesses[2]);
        torsoSize = bipedSettings.TorsoLengths.Sum();
        bottom.LinkChild(middle);
        middle.LinkChild(top);

        neckAttachmentBone = top;
        armAttachmentBone = top;

        return bottom;
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
        float angle = 0f;
        if (mode == Mode.Quadruped)
            angle = Random.Range(-90f, 0f);

        BoneDefinition prev = attachTo;
        for (var i = 0; i < bipedSettings.NeckBones; i++)
        {
            var neckPart = buildNeckPart(bipedSettings.NeckBoneLength, bipedSettings.NeckThickness);
            prev.LinkChild(neckPart);
            neckPart.AttachmentHint.Rotation = Quaternion.Euler(angle, 0f, 0f);
            prev = neckPart;
            if (mode == Mode.Quadruped)
                angle = Random.Range(-20f, 20f);
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
        BoneDefinition head = new()
        {
            Length = bipedSettings.HeadSize,
            DistalAxis = Vector3.up,
            VentralAxis = Vector3.forward,
            Category = BoneCategory.Head,
            AttachmentHint = new AttachmentHint(),
            Thickness = bipedSettings.HeadSize
        };
        attachTo.LinkChild(head);

        return head;
    }

    private void attachLegs(List<BoneDefinition> legs, BoneDefinition torso)
    {
        if (mode == Mode.Quadruped)
        {
            buildHip(torso, legs[0], legs[1], Vector3.back, Vector3.down, RelativePositions.ProximalPoint);

            BoneDefinition frontHip = buildHip(neckAttachmentBone, legs[2], legs[3], Vector3.forward, Vector3.down, RelativePositions.DistalPoint);
            neckAttachmentBone = frontHip;

            // rotate torso
            float legDiff = hindLegHeight - frontLegHeight;
            float angle = - Mathf.Atan(legDiff / torsoSize) * Mathf.Rad2Deg;
            torso.AttachmentHint.Rotation = Quaternion.Euler(angle, 0.0f, 0.0f);

            foreach (var leg in legs)
            {
                leg.AttachmentHint.Rotation = Quaternion.Euler(angle, 0.0f, 0.0f);
            }
        } else
        {
            buildHip(torso, legs[0], legs[1], Vector3.down, Vector3.forward, RelativePositions.ProximalPoint);
        }
    }
    
    private BoneDefinition buildHip(BoneDefinition attachTo, BoneDefinition leg1, BoneDefinition leg2, Vector3 distalAxis, Vector3 ventralAxis, RelativePosition pos)
    {
        BoneDefinition hip = new()
        {
            Length = bipedSettings.HipLength,
            Category = BoneCategory.Hip,
            DistalAxis = distalAxis,
            VentralAxis = ventralAxis,
            Thickness = bipedSettings.HipThickness,
            AttachmentHint =
            {
                Position = pos
            }
        };

        attachTo.LinkChild(hip);
        hip.LinkChild(leg1);
        leg1.AttachmentHint.Position = new RelativePosition(1.0f, 0.0f, 0.5f);
        hip.LinkChild(leg2);
        leg2.AttachmentHint.Position = new RelativePosition(-1.0f, 0.0f, 0.5f);
        return hip;

        // old double hip bones

        /*BoneDefinition hip = new();
        hip.Length = attachTo.Thickness;
        hip.DistalAxis = proximalAxis;
        hip.VentralAxis = Vector3.forward;
        hip.Category = BoneCategory.Hip;
        hip.Thickness = 0f; // For now to avoid metaball generation around hip
        hip.AttachmentHint.Position = pos;
        attachTo.LinkChild(hip);
        hip.LinkChild(leg);
        return hip;*/
    }
}
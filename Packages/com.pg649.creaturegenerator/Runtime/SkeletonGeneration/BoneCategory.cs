using System;

public enum BoneCategory
{
    Leg,
    [Obsolete("Use FrontLeg1/HindLeg1 instead")]
    LowerLeg1,
    [Obsolete("Use FrontLeg2/HindLeg2 instead")]
    LowerLeg2,
    Arm,
    LowerArm,
    Torso,
    Head,
    Hand,
    Foot,
    Shoulder,
    Hip,
    Paw,
    FrontLeg,
    FrontLeg1,
    FrontLeg2,
    HindLeg,
    HindLeg1,
    HindLeg2,
    Other
}
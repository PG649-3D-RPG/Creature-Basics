using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonDefinition
{
    public BoneDefinition RootBone;

    public LimitTable JointLimits;

    public ISettingsInstance SettingsInstance;
    public SkeletonDefinition(BoneDefinition root, LimitTable limits, ISettingsInstance instance) {
        this.RootBone = root;
        this.JointLimits = limits;
        this.SettingsInstance = instance;
    }
}

public struct JointLimits {
    public float XAxisMax;
    public float XAxisMin;
    public float YAxisSymmetric;
    public float ZAxisSymmetric;

    public Vector3? Axis;
    public Vector3? SecondaryAxis;
}

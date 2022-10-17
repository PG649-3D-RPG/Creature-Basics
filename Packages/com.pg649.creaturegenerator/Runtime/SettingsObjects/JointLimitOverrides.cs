using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "JointLimitOverrides", menuName = "PG649-CreatureGenerator/Joint Limit Overrides")]
public class JointLimitOverrides : ScriptableObject
{
    [System.Serializable]
    public class JointTypeLimit
    {
        public BoneCategory parentBone;
        public BoneCategory childBone;
        public int XAxisMin;
        public int XAxisMax;
        public int YAxisSymmetric;
        public int ZAxisSymmetric;
    }

    public JointTypeLimit[] limitOverrides;

    public LimitTable ToLimitTable()
    {
        LimitTable table = new();
        foreach (var l in limitOverrides)
        {
            table.Add((l.parentBone, l.childBone), new JointLimits { XAxisMin = l.XAxisMin, XAxisMax = l.XAxisMax, YAxisSymmetric = l.YAxisSymmetric, ZAxisSymmetric = l.ZAxisSymmetric });
        }
        return table;
    }
}

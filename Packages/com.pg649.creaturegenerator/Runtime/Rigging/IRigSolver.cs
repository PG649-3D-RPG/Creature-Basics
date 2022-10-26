using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRigSolver
{

    public enum RigSolverType {
        ClosestBone, BoneHeat
    }

    public static IRigSolver Get(RigSolverType type) {
        if (type == RigSolverType.ClosestBone)
            return new ClosestBoneRigSolver();
        else if (type == RigSolverType.BoneHeat)
            return new BoneHeatRigSolver();
        else
            return null;
    }


    void CalcBoneWeights(Mesh mesh, Transform[] bones, Transform meshTransform);

}
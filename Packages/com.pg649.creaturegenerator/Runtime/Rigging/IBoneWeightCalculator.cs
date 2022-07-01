using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBoneWeightCalculator
{

    BoneWeight[] CalcBoneWeights(Mesh mesh, Transform[] bones, Transform meshTransform);

}
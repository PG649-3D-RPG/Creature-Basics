using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrivialBoneWeightCalculator : IBoneWeightCalculator
{

    public BoneWeight[] CalcBoneWeights(Mesh mesh, Transform[] bones, Transform meshTransform) {
		Vector3[] vertices = mesh.vertices;

        BoneWeight[] weights = new BoneWeight[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            float distMin = float.PositiveInfinity;
            int jMin = 0;
            for (int j = 0; j < bones.Length; j++) {
                Vector3 bonePos = bones[j].position;
                Vector3 vertexPos = meshTransform.TransformPoint(vertices[i]);

                float dist = Vector3.Distance(bonePos, vertexPos);
                if (dist < distMin) {
                    distMin = dist;
                    jMin = j;
                }
            }
            //Debug.Log(jMin + "     " + distMin);
            weights[i].boneIndex0 = jMin;
            weights[i].weight0 = 1;
        }

        return weights;
    }

}
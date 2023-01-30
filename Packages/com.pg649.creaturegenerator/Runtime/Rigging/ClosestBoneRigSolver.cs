using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class ClosestBoneRigSolver : IRigSolver
{

    public void CalcBoneWeights(Mesh mesh, IVisibilityTester tester, Bone[] bones, Transform meshTransform, Metaball metaball) {
		Vector3[] vertices = mesh.vertices;

        byte[] bonesPerVertex = new byte[vertices.Length];
        BoneWeight1[] weights = new BoneWeight1[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            float distMin = float.PositiveInfinity;
            int jMin = 0;
            for (int j = 0; j < bones.Length; j++) {
                Vector3 bonePos = bones[j].WorldProximalPoint();
                Vector3 vertexPos = vertices[i];
                //Vector3 vertexPos = meshTransform.TransformPoint(vertices[i]);

                float dist = Vector3.Distance(bonePos, vertexPos);
                if (dist < distMin) {
                    distMin = dist;
                    jMin = j;
                }
            }
            //Debug.Log(jMin + "     " + distMin);
            weights[i].boneIndex = jMin;
            weights[i].weight = 1;

            bonesPerVertex[i] = 1;
        }

        mesh.SetBoneWeights(new NativeArray<byte>(bonesPerVertex, Allocator.Temp), 
                            new NativeArray<BoneWeight1>(weights, Allocator.Temp));
    }

}
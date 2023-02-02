using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;

public class MetaballRigSolver : IRigSolver
{
    public MetaballRigSolver() { }
    public void CalcBoneWeights(Mesh mesh, IVisibilityTester tester, Bone[] bones, Transform meshTransform, Metaball metaball)
    {
        int nv = mesh.vertexCount;
        byte[] bonesPerVertex = new byte[nv];
        List<BoneWeight1> finalWeights = new List<BoneWeight1>();
        for (int i=0; i<nv; i++)
        {
            Ball[] vertexBalls;
            float[] weights;
            (vertexBalls, weights) = metaball.GetWeights(mesh.vertices[i].x, mesh.vertices[i].y, mesh.vertices[i].z);
            float sum = 0;
            for (int j = 0; j < weights.Length; ++j)
                sum += weights[j];

            for (int j = 0; j < weights.Length; ++j)
            {
                BoneWeight1 w = new BoneWeight1();
                w.boneIndex = Array.IndexOf(bones, vertexBalls[j].bone);
                w.weight = weights[j] / sum;
                finalWeights.Add(w);
            }
            bonesPerVertex[i] = (byte) weights.Length;
        }
        mesh.SetBoneWeights(new NativeArray<byte>(bonesPerVertex, Allocator.Temp),
                            new NativeArray<BoneWeight1>(finalWeights.ToArray(), Allocator.Temp));
    }
}

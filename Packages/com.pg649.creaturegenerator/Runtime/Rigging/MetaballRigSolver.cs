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
        Color[] colors = new Color[nv];
        for (int i=0; i<nv; i++)
        {
            colors[i] = new Color(0,0,0);
            Bone[] vertexBones;
            float[] weights;
            (vertexBones, weights) = metaball.GetWeights(mesh.vertices[i].x, mesh.vertices[i].y, mesh.vertices[i].z);
            float sum = 0;
            for (int j = 0; j < weights.Length; ++j)
                sum += weights[j];

            for (int j = 0; j < weights.Length; ++j)
            {
                BoneWeight1 w = new BoneWeight1();
                w.boneIndex = Array.IndexOf(bones, vertexBones[j]);
                w.weight = weights[j] / sum;
                finalWeights.Add(w);
                if (vertexBones[j].category == BoneCategory.Shoulder)
                    colors[i] += new Color(w.weight, 0, 0);
                if (w.weight < 0)
                    Debug.Log("aaaaaaaaaaaaaaaa");
            }
            bonesPerVertex[i] = (byte) weights.Length;
        }
        mesh.colors = colors;
        mesh.SetBoneWeights(new NativeArray<byte>(bonesPerVertex, Allocator.Temp),
                            new NativeArray<BoneWeight1>(finalWeights.ToArray(), Allocator.Temp));
    }
}

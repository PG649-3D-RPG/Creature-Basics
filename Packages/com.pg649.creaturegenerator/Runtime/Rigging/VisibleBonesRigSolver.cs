using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;
using System.Linq;

public class VisibleBonesRigSolver : IRigSolver
{

    public void CalcBoneWeights(Mesh mesh, IVisibilityTester tester, Bone[] bones, Transform meshTransform)
    {
        // nv = numVertices
        int nv = mesh.vertices.Length;


        // create a timer
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        float[,] boneDists = new float[nv, bones.Length];
        bool[,] boneVis = new bool[nv, bones.Length];
        //Calculate bone distances and bone visibilities
        for (int i = 0; i < nv; ++i)
        {
            Vector3 cPos = mesh.vertices[i];

            //calculate boneDists and minDist
            float minDist = float.PositiveInfinity;
            for (int j = 1; j < bones.Length; ++j)
            {
                Vector3 v1 = bones[j].WorldProximalPoint();
                Vector3 v2 = bones[j].WorldDistalPoint();

                Vector3 dir = v2 - v1;
                Vector3 difv1 = cPos - v1;
                Vector3 difv2 = v2 - cPos;
                float distToSeg;
                if (Vector3.Dot(dir, difv2) < 0)
                {
                    distToSeg = Vector3.Distance(v2, cPos);
                }
                else if (Vector3.Dot(difv1, dir) <= 0)
                {
                    distToSeg = Vector3.Distance(v1, cPos);
                }
                else
                {
                    //distToSeg = Vector3.Cross((cPos - v1), (cPos - v2)).magnitude / dir.magnitude;
                    distToSeg = Mathf.Max(0, difv1.sqrMagnitude - Mathf.Pow(Vector3.Dot(difv1, dir), 2) / dir.sqrMagnitude);
                }

                boneDists[i, j] = distToSeg;
                minDist = Mathf.Min(boneDists[i, j], minDist);
            }

            // project cPos on the segment and test if the projected point is visible from cPos
            for (int j = 1; j < bones.Length; ++j)
            {
                //the reason we don't just pick the closest bone is so that if two are
                //equally close, both are factored in.
                if (boneDists[i, j] > minDist * 1.5f)
                    continue;

                Vector3 v1 = bones[j].WorldProximalPoint();
                Vector3 v2 = bones[j].WorldDistalPoint();
                Vector3 dir = v2 - v1;
                Vector3 projToSeg;

                if (Vector3.Dot((v2 - cPos), dir) < 0)
                    projToSeg = v2;
                else if (Vector3.Dot((cPos - v1), dir) <= 0)
                    projToSeg = v1;
                else
                    projToSeg = v1 + (Vector3.Dot((cPos - v1), dir) / dir.sqrMagnitude) * dir;

                boneVis[i, j] = tester.CanSee(cPos, projToSeg);
            }
        }
        stopwatch.Stop();
        Debug.Log($"Bone distance and visibility calculation took: {stopwatch.Elapsed.TotalSeconds} seconds.");

        stopwatch.Restart();
        List<BoneWeight1>[] weights = new List<BoneWeight1>[nv];
        for (int i = 0; i < nv; ++i) {
            weights[i] = new List<BoneWeight1>();
        }

        for (int j = 1; j < bones.Length; ++j)
        {
            for (int i = 0; i < nv; ++i)
            {
                if (boneVis[i,j])
                {
                    BoneWeight1 w = new BoneWeight1();
                    w.boneIndex = j;
                    w.weight = 1.0f / boneDists[i,j];
                    weights[i].Add(w);
                }
            }
        }
        stopwatch.Stop();
        Debug.Log($"Matrix solving took: {stopwatch.Elapsed.TotalSeconds} seconds.");

        byte[] bonesPerVertex = new byte[nv];
        List<BoneWeight1> finalWeights = new List<BoneWeight1>();
        for(int i = 0; i < nv; ++i) {
            if (weights[i].Count == 0) { // vertex is not attached to any bone
                /*float minDist = float.PositiveInfinity;
                int bone = 0;
                for (int j = 1; j < bones.Length; ++j)
                {
                    if (boneVis[i,j] && boneDists[i,j] < minDist)
                    {
                        bone = j;
                        minDist = boneDists[i,j];
                    }
                }*/
                BoneWeight1 w = new BoneWeight1();
                w.boneIndex = 0;
                w.weight = 1.0f;
                finalWeights.Add(w);

                bonesPerVertex[i] = 1;
            } else {
                weights[i].Sort((a, b) => b.weight.CompareTo(a.weight));

                float sum = 0;
                for(int j = 0; j < weights[i].Count; ++j)
                    sum += weights[i][j].weight;

                for(int j = 0; j < weights[i].Count; ++j) {
                    BoneWeight1 w = new BoneWeight1();
                    w.boneIndex = weights[i][j].boneIndex;
                    w.weight = weights[i][j].weight / sum;
                    finalWeights.Add(w);
                }
                bonesPerVertex[i] = (byte) weights[i].Count;
            }
        }

        mesh.SetBoneWeights(new NativeArray<byte>(bonesPerVertex, Allocator.Temp), 
                            new NativeArray<BoneWeight1>(finalWeights.ToArray(), Allocator.Temp));
    }
}

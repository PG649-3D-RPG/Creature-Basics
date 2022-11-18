using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;
using System.Linq;

// for reference see: https://people.csail.mit.edu/ibaran/papers/2007-SIGGRAPH-Pinocchio.pdf
public class BoneHeatRigSolver : IRigSolver
{

    private bool vectorInCone(Vector3 v, List<Vector3> cone)
    {
        Vector3 avg = Vector3.zero;
        for(int i = 0; i < cone.Count; ++i)
            avg += cone[i];

        return Vector3.Dot(v.normalized, avg.normalized) > 0.5;
    }

    // TODO meshTransform must be respected when accessing mesh.vertices because bone position might not be in the same coordinate system as vertices
    public void CalcBoneWeights(Mesh mesh, IVisibilityTester tester, Bone[] bones, Transform meshTransform)
    {
        //BoneHeatNativePluginInterface.PreprocessMesh(mesh);

        // nv = numVertices
        int nv = mesh.vertices.Length;

        Debug.Log($"Starting Bone Heat Weighting with {nv} vertices and {mesh.triangles.Length} triangles...");
        
        //float initialHeatWeight = 0.22f;
        float initialHeatWeight = 1f;

        // create a timer
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        //first index: vertex i
        //second index: jth edge adjacent to vertex i
        List<int>[] edges = new List<int>[nv];
        for (int i = 0; i < nv; ++i)
        {
            edges[i] = new List<int>(8);
        }
        for (int j = 0; j < mesh.triangles.Length; ++j)
        {
            if (j % 3 == 0)
            {
                if (!edges[mesh.triangles[j]].Contains(mesh.triangles[j + 1])) 
                    edges[mesh.triangles[j]].Add(mesh.triangles[j + 1]);
                if (!edges[mesh.triangles[j]].Contains(mesh.triangles[j + 2])) 
                    edges[mesh.triangles[j]].Add(mesh.triangles[j + 2]);
            }
            else if (j % 3 == 1)
            {
                if (!edges[mesh.triangles[j]].Contains(mesh.triangles[j - 1])) 
                    edges[mesh.triangles[j]].Add(mesh.triangles[j - 1]);
                if (!edges[mesh.triangles[j]].Contains(mesh.triangles[j + 1])) 
                    edges[mesh.triangles[j]].Add(mesh.triangles[j + 1]);
            }
            else
            {
                if (!edges[mesh.triangles[j]].Contains(mesh.triangles[j - 2])) 
                    edges[mesh.triangles[j]].Add(mesh.triangles[j - 2]);
                if (!edges[mesh.triangles[j]].Contains(mesh.triangles[j - 1])) 
                    edges[mesh.triangles[j]].Add(mesh.triangles[j - 1]);
            }
        }
        stopwatch.Stop();
        Debug.Log($"Edge calculation took: {stopwatch.Elapsed.TotalSeconds} seconds.");

        stopwatch.Restart();
        float[,] boneDists = new float[nv, bones.Length];
        bool[,] boneVis = new bool[nv, bones.Length];
        //Calculate bone distances and bone visibilities
        for (int i = 0; i < nv; ++i)
        {
            Vector3 cPos = mesh.vertices[i];

            // calculate normal vectors of all adjacent triangles
            List<Vector3> normals = new List<Vector3>(edges[i].Count);
            for (int j = 0; j < edges[i].Count; ++j)
            {
                int nj = (j + 1) % edges[i].Count;
                Vector3 v1 = mesh.vertices[edges[i][j]] - cPos;
                Vector3 v2 = mesh.vertices[edges[i][nj]] - cPos;
                normals.Add(Vector3.Normalize(Vector3.Cross(v1, v2)));
            }

            //calculate boneDists and minDist
            float minDist = float.PositiveInfinity;
            for (int j = 0; j < bones.Length; ++j)
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
                    distToSeg = Mathf.Max(0, difv1.sqrMagnitude - Mathf.Pow(Vector3.Dot(difv1, dir), 2) / dir.sqrMagnitude);
                }

                boneDists[i, j] = distToSeg;
                minDist = Mathf.Min(boneDists[i, j], minDist);
            }

            // project cPos on the segment and test if the projected point is visible from cPos
            for (int j = 0; j < bones.Length; ++j)
            {
                //the reason we don't just pick the closest bone is so that if two are
                //equally close, both are factored in.
                if (boneDists[i, j] > minDist * 1.001f)
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

                boneVis[i, j] = tester.CanSee(cPos, projToSeg) /*&& vectorInCone(cPos - projToSeg, normals)*/;
            }
        }
        stopwatch.Stop();
        Debug.Log($"Bone distance and visibility calculation took: {stopwatch.Elapsed.TotalSeconds} seconds.");

        stopwatch.Restart();
        SparseMatrix matrix = new SparseMatrix(nv, nv); // The heat matrix to solve.
        float[] D = new float[nv];
        float[] H = new float[nv];
        int[] closest = new int[nv];
        for (int i = 0; i < nv; ++i)
        {
            //get areas
            for (int j = 0; j < edges[i].Count; ++j)
            {
                int nj = (j + 1) % edges[i].Count;

                D[i] += (Vector3.Cross((mesh.vertices[edges[i][j]] - mesh.vertices[i]), (mesh.vertices[edges[i][nj]] - mesh.vertices[i]))).magnitude;
            }
            D[i] = 1 / (1e-5f + D[i]);

            //get bones
            float minDist = float.PositiveInfinity;
            for (int j = 0; j < bones.Length; ++j)
            {
                if (boneDists[i,j] < minDist)// && boneVis[i,j])
                {
                    closest[i] = j;
                    minDist = boneDists[i,j];
                }
            }
            for (int j = 0; j < bones.Length; ++j)
            {
                if (boneVis[i,j] && boneDists[i,j] <= minDist * 1.001f)
                {
                    H[i] += initialHeatWeight / Mathf.Pow(1e-3f + boneDists[i, closest[i]], 2);
                }
            }

            //get laplacian
            //for reference see: https://en.wikipedia.org/wiki/Discrete_Laplace_operator#Mesh_Laplacians
            float sum = 0;
            for (int j = 0; j < edges[i].Count; ++j)
            {
                int nj = (j + 1) % edges[i].Count;
                int pj = (j + edges[i].Count - 1) % edges[i].Count;

                // these are the cotangents of the two interior angles opposite to the edge
                float cot1 = 0.0f;
                float cot2 = 0.0f;
                if (edges[edges[i][j]].Contains(edges[i][pj]))
                {
                    Vector3 v1 = mesh.vertices[i] - mesh.vertices[edges[i][pj]];
                    Vector3 v2 = mesh.vertices[edges[i][j]] - mesh.vertices[edges[i][pj]];
                    
                    cot1 = Vector3.Dot(v1, v2) / (1e-6f + Vector3.Cross(v1, v2).magnitude);
                }
                if (edges[edges[i][j]].Contains(edges[i][nj]))
                {
                    Vector3 v3 = mesh.vertices[i] - mesh.vertices[edges[i][nj]];
                    Vector3 v4 = mesh.vertices[edges[i][j]] - mesh.vertices[edges[i][nj]];
                    cot2 = Vector3.Dot(v3, v4) / (1e-6f + Vector3.Cross(v3, v4).magnitude);
                }

                cot1 = Mathf.Max(0.0f, cot1);
                cot2 = Mathf.Max(0.0f, cot2);

                sum += (cot1 + cot2);

                if (edges[i][j] > i)
                    continue;
                matrix.triplets.Add(new SparseMatrix.Triplet(i, edges[i][j], -(cot1 + cot2)));
            }
            matrix.triplets.Add(new SparseMatrix.Triplet(i, i, sum + H[i] / D[i]));
        }
        stopwatch.Stop();
        Debug.Log($"Heat and laplacian calculation took: {stopwatch.Elapsed.TotalSeconds} seconds.");

        stopwatch.Restart();
        List<BoneWeight1>[] weights = new List<BoneWeight1>[nv];
        for(int i = 0; i < nv; ++i) {
            weights[i] = new List<BoneWeight1>();
        }

        for (int j = 0; j < bones.Length; ++j)
        {
            float[] rhs = new float[nv];
            for (int i = 0; i < nv; ++i)
            {
                if (boneVis[i,j] && boneDists[i,j] <= boneDists[i, closest[i]] * 1.001f)
                    rhs[i] = H[i] / D[i];
            }

            float[] solved = BoneHeatNativePluginInterface.SolveSPDMatrix(matrix, rhs);
            for (int i = 0; i < nv; ++i)
            {
                if (solved[i] > 1)
                    solved[i] = 1; //clip just in case
                if (solved[i] > 1e-5) {
                    BoneWeight1 w = new BoneWeight1();
                    w.boneIndex = j;
                    w.weight = solved[i];
                    weights[i].Add(w);
                }
            }
        }
        stopwatch.Stop();
        Debug.Log($"Matrix solving took: {stopwatch.Elapsed.TotalSeconds} seconds.");

        for (int i = 0; i < nv; ++i)
        {
            if (weights[i].Count == 0) { // if vertex is not attached to any bone
                for (int j = 0; j < bones.Length; ++j)
                {
                    if (boneDists[i,j] <= 1.05f * boneDists[i, closest[i]])
                    {
                        BoneWeight1 w = new BoneWeight1();
                        w.boneIndex = j;
                        w.weight = 1.0f / boneDists[i,j];
                        weights[i].Add(w);
                    }
                }
            }
        }

        byte[] bonesPerVertex = new byte[nv];
        List<BoneWeight1> finalWeights = new List<BoneWeight1>();
        for(int i = 0; i < nv; ++i) {
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

        mesh.SetBoneWeights(new NativeArray<byte>(bonesPerVertex, Allocator.Temp), 
                            new NativeArray<BoneWeight1>(finalWeights.ToArray(), Allocator.Temp));
    }
}

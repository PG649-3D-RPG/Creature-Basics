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

    private Vector3 projectToSegment(Vector3 p, Vector3 v1, Vector3 v2) {
        Vector3 dir = v2 - v1;
        Vector3 dirv1 = p - v1;
        Vector3 dirv2 = p - v2;
        float dotv1 = Vector3.Dot(dir, dirv1);
        float dotv2 = Vector3.Dot(dir, dirv2);
        if (dotv1 > 0)
        {
            return v1;
        }
        else if (dotv2 < 0)
        {
            return v2;
        }
        else
        {
            return v1 + Vector3.Project(dirv1, dir.normalized);
        }
    }

    private float distToSegment(Vector3 p, Vector3 v1, Vector3 v2) {
        Vector3 dir = v2 - v1;
        Vector3 dirv1 = p - v1;
        Vector3 dirv2 = p - v2;
        float dotv1 = Vector3.Dot(dir, dirv1);
        float dotv2 = Vector3.Dot(dir, dirv2);
        float dist;
        if (dotv1 > 0)
        {
            dist = dirv1.magnitude;
        }
        else if (dotv2 < 0)
        {
            dist = dirv2.magnitude;
        }
        else
        {
            dist = (Vector3.Cross(dir, dirv1) / dir.magnitude).magnitude;
        }

        return dist;
    }

    private static readonly float DistanceEpsilon = 1.1f;
    private static readonly float CWeight = 0.22f;

    // TODO meshTransform must be respected when accessing mesh.vertices because bone position might not be in the same coordinate system as vertices
    public void CalcBoneWeights(Mesh mesh, IVisibilityTester tester, Bone[] bones, Transform meshTransform)
    {
        // nv = numVertices
        int nv = mesh.vertices.Length;

        Debug.Log($"Starting Bone Heat Weighting with {nv} vertices and {mesh.triangles.Length} triangles...");
        
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

                float distToSeg = distToSegment(cPos, v1, v2);

                /*Vector3 projToSeg = projectToSegment(cPos, v1, v2);
                float cosine = Vector3.Dot(cPos - projToSeg, mesh.normals[i]);
                distToSeg = distToSeg / (0.5f * (cosine + 1.001f));*/

                boneDists[i, j] = distToSeg;
                minDist = Mathf.Min(boneDists[i, j], minDist);
            }

            // project cPos on the segment and test if the projected point is visible from cPos
            for (int j = 0; j < bones.Length; ++j)
            {
                //the reason we don't just pick the closest bone is so that if two are
                //equally close, both are factored in.
                if (boneDists[i, j] > minDist * DistanceEpsilon)
                    continue;

                Vector3 v1 = bones[j].WorldProximalPoint();
                Vector3 v2 = bones[j].WorldDistalPoint();
                Vector3 projToSeg = projectToSegment(cPos, v1, v2);

                boneVis[i, j] = tester.CanSee(cPos, projToSeg) /*&& vectorInCone(cPos - projToSeg, normals)*/;
            }
        }
        stopwatch.Stop();
        Debug.Log($"Bone distance and visibility calculation took: {stopwatch.Elapsed.TotalSeconds} seconds.");

        stopwatch.Restart();
        SparseMatrix matrix = new SparseMatrix(nv, nv); // The heat matrix to solve.
        float[] area = new float[nv];
        float[] H = new float[nv];
        int[] closest = new int[nv];
        int[] numclosest = new int[nv];
        for (int i = 0; i < nv; ++i)
        {
            //get areas
            for (int j = 0; j < edges[i].Count; ++j)
            {
                int nj = (j + 1) % edges[i].Count;

                area[i] += (Vector3.Cross((mesh.vertices[edges[i][j]] - mesh.vertices[i]), (mesh.vertices[edges[i][nj]] - mesh.vertices[i]))).magnitude;
            }
            //area[i] /= 3.0f;

            //get bones
            float minDist = float.PositiveInfinity;
            for (int j = 0; j < bones.Length; ++j)
            {
                if (boneDists[i,j] < minDist && boneVis[i,j])
                {
                    closest[i] = j;
                    minDist = boneDists[i,j];
                }
            }
            minDist = Mathf.Max(minDist, 1e-4f);
            for (int j = 0; j < bones.Length; ++j)
            {
                if (boneVis[i,j] && boneDists[i,j] <= minDist * DistanceEpsilon)
                {
                    numclosest[i]++;
                    H[i] += CWeight / (minDist * minDist);
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
            matrix.triplets.Add(new SparseMatrix.Triplet(i, i, (sum) + H[i]));
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
                if (boneVis[i,j] && boneDists[i,j] <= boneDists[i, closest[i]] * DistanceEpsilon) {
                    float p = (numclosest[i] > 0) ? 1.0f / numclosest[i] : 0.0f;
                    rhs[i] = p * H[i];
                }
            }

            float[] solved = BoneHeatNativePluginInterface.SolveSPDMatrix(matrix, rhs);
            for (int i = 0; i < nv; ++i)
            {
                if (solved[i] > 1)
                    solved[i] = 1; //clip just in case

                if (solved[i] > 1e-8f) {
                    BoneWeight1 w = new BoneWeight1();
                    w.boneIndex = j;
                    w.weight = solved[i];
                    weights[i].Add(w);
                }
            }
        }
        stopwatch.Stop();
        Debug.Log($"Matrix solving took: {stopwatch.Elapsed.TotalSeconds} seconds.");

        int misattached = 0;
        for (int i = 0; i < nv; ++i)
        {
            if (weights[i].Count == 0) { // if vertex is not attached to any bone
                misattached++;
                /*for (int j = 0; j < bones.Length; ++j)
                {
                    if (boneDists[i,j] <= 1.05f * boneDists[i, closest[i]])
                    {
                        BoneWeight1 w = new BoneWeight1();
                        w.boneIndex = j;
                        w.weight = 1.0f / boneDists[i,j];
                        weights[i].Add(w);
                    }
                }*/
                BoneWeight1 w = new BoneWeight1();
                w.boneIndex = 0;
                w.weight = 1.0f;
                weights[i].Add(w);
            }
        }
        Debug.Log($"BoneHeat: Had to fix {misattached} misattached vertices using fallback method.");

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

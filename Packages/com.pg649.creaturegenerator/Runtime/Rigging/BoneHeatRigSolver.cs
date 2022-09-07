using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    public BoneWeight[] CalcBoneWeights(Mesh mesh, Transform[] bones, Transform meshTransform)
    {
        // nv = numVertices
        int nv = mesh.vertices.Length;

        Debug.Log($"Starting Bone Heat Weighting with {nv} vertices...");

        VisibilityTester tester = new VisibilityTester(mesh, 64);
        
        float initialHeatWeight = 1;

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
            for (int j = 1; j < bones.Length; ++j)
            {
                //weights[i].Add(-1);
                Vector3 v1 = bones[j].position;
                Vector3 v2 = bones[j].parent.position;

                Vector3 dir = v2 - v1;
                Vector3 difv1 = cPos - v1;
                Vector3 difv2 = v2 - cPos;
                float distToSeg;
                if (Vector3.Dot(dir, difv2) < 0)
                {
                    distToSeg = Vector3.Distance(v2, cPos);
                }
                else if (Vector3.Dot(dir, difv1) < 0)
                {
                    distToSeg = Vector3.Distance(v1, cPos);
                }
                else
                {
                    distToSeg = Vector3.Magnitude(Vector3.Cross((cPos - v1), (cPos - v2)));
                    distToSeg = distToSeg / Vector3.Magnitude(v2 - v1);
                    Mathf.Max(0, distToSeg);//TODO: ??? why should distance be negative
                }

                boneDists[i, j] = distToSeg;
                minDist = Mathf.Min(boneDists[i, j], minDist);
            }

            // project cPos on the segment and test if the projected point is visible from cPos
            for (int j = 1; j < bones.Length; ++j)
            {
                //the reason we don't just pick the closest bone is so that if two are
                //equally close, both are factored in.
                if (boneDists[i, j] > minDist * 1.0001)
                    continue;

                Vector3 v1 = bones[j].position;
                Vector3 v2 = bones[j].parent.position;
                Vector3 dir = v2 - v1;
                Vector3 projToSeg;

                if (Vector3.Dot((v2 - cPos), dir) < 0)
                    projToSeg = v2;
                else if (Vector3.Dot((cPos - v1), dir) < 0)
                    projToSeg = v1;
                else
                    projToSeg = v1 + Vector3.Dot((cPos - v1), dir) / dir.sqrMagnitude * dir;

                boneVis[i, j] = tester.CanSee(cPos, projToSeg) ;//&& vectorInCone(cPos - projToSeg, normals);
                //TODO why is vector in cone broken???????
            }
        }
        stopwatch.Stop();
        Debug.Log($"Bone distance and visibility calculation took: {stopwatch.Elapsed.TotalSeconds} seconds.");

        stopwatch.Restart();
        SparseMatrix matrix = new SparseMatrix(nv, nv); // The heat matrix to solve.
        float[] distance = new float[nv];
        float[] heat = new float[nv];
        int[] closest = new int[nv];
        for (int i = 0; i < nv; ++i)
        {
            //get areas
            for (int j = 0; j < edges[i].Count; ++j)
            {
                int nj = (j + 1) % edges[i].Count;

                distance[i] += (Vector3.Cross((mesh.vertices[edges[i][j]] - mesh.vertices[i]), (mesh.vertices[edges[i][nj]] - mesh.vertices[i]))).magnitude;
            }
            distance[i] = 1 / (1e-10f + distance[i]);

            //get bones
            float minDist = float.PositiveInfinity;
            for (int j = 1; j < bones.Length; ++j)
            {
                if (boneDists[i,j] < minDist)
                {
                    closest[i] = j;
                    minDist = boneDists[i,j];
                }
            }
            for (int j = 1; j < bones.Length; ++j)
            {
                if (boneVis[i,j] && boneDists[i,j] <= minDist * 1.001)
                {
                    heat[i] += initialHeatWeight / Mathf.Pow(1e-8f + boneDists[i, closest[i]], 2);
                }
            }

            //get laplacian
            float sum = 0;
            for (int j = 0; j < edges[i].Count; ++j)
            {
                int nj = (j + 1) % edges[i].Count;
                int pj = (j + edges[i].Count - 1) % edges[i].Count;

                Vector3 v1 = mesh.vertices[i] - mesh.vertices[edges[i][pj]];
                Vector3 v2 = mesh.vertices[edges[i][j]] - mesh.vertices[edges[i][pj]];
                Vector3 v3 = mesh.vertices[i] - mesh.vertices[edges[i][nj]];
                Vector3 v4 = mesh.vertices[edges[i][j]] - mesh.vertices[edges[i][nj]];

                float cot1 = (Vector3.Dot(v1, v2)) / (1e-6f + (Vector3.Cross(v1, v2)).magnitude);
                float cot2 = (Vector3.Dot(v3, v4)) / (1e-6f + (Vector3.Cross(v3, v4)).magnitude);
                sum += (cot1 + cot2);

                if (edges[i][j] > i)
                    continue;
                matrix.triplets.Add(new SparseMatrix.Triplet(i, edges[i][j], -cot1 - cot2));
            }
            matrix.triplets.Add(new SparseMatrix.Triplet(i, i, sum + heat[i] / distance[i]));
            Debug.Log($"Sum {sum}   Heat {heat[i]}   Closest {closest[i]}   Distance {distance[i]}");
        }
        stopwatch.Stop();
        Debug.Log($"Heat and laplacian calculation took: {stopwatch.Elapsed.TotalSeconds} seconds.");

        stopwatch.Restart();
        List<Tuple<int, float>>[] weights = new List<Tuple<int, float>>[nv];
        for (int i = 0; i < nv; ++i) {
            weights[i] = new List<Tuple<int, float>>();
        }

        for (int j = 1; j < bones.Length; ++j)
        {
            float[] rhs = new float[nv];
            for (int i = 0; i < nv; ++i)
            {
                if (boneVis[i,j] && boneDists[i,j] <= boneDists[i, closest[i]] * 1.001)
                    rhs[i] = heat[i] / distance[i];
            }

            float[] solved = BoneHeatNativePluginInterface.SolveSPDMatrix(matrix, rhs);
            for (int i = 0; i < nv; ++i)
            {
                if (solved[i] > 1)
                    solved[i] = 1; //clip just in case
                if (solved[i] > 1e-8)
                    weights[i].Add(new Tuple<int, float>(j, solved[i]));
            }
        }
        stopwatch.Stop();
        Debug.Log($"Matrix solving took: {stopwatch.Elapsed.TotalSeconds} seconds.");

        /*for(int i = 0; i < nv; ++i) {
            float sum = 0;
            for(int j = 0; j < weights[i].Count; ++j)
                sum += weights[i][j].Item2;

            for(int j = 0; j < weights[i].Count; ++j) {
                float helper = weights[i][j].Item2 / sum;
                weights[i][j] = new Tuple<int, double>(nzweights[i][j].Item1, helper);
                weights[i][nzweights[i][j].Item1] = nzweights[i][j].Item2;
            }
        }*/

        
       
       
        //TODO: only temporary to test
        BoneWeight[] boneWeights = new BoneWeight[nv];
        for (int i = 0; i < nv; ++i)
        {
            float distMin = float.PositiveInfinity;
            int jMin = 0;
            for (int j = 1; j < bones.Length; j++) {
                float dist = (float) boneDists[i,j];
                if (dist < distMin) {
                    distMin = dist;
                    jMin = j;
                }
            }
            //Debug.Log(jMin + "     " + distMin);
            boneWeights[i].boneIndex0 = jMin;
            boneWeights[i].weight0 = 1;
        }
        return boneWeights;

        // Needs some kind of Matrix computation
        //SPDMatrix Am(A);
        //LLTMatrix* Ainv = Am.factor();
        //if (Ainv == NULL)
        //    return;
        //
        /*for (int j = 0; j < bones.Length; ++j)
        {
            List<double> rhs = new List<double>(); ;
            for (int i = 0; i < nv; ++i)
            {
                if (boneVis[i,j] && boneDists[i,j] <= boneDists[i,closest[i]] * 1.00001)
                    rhs[i] = heat[i] / distance[i];
            }

            //No Matrix no solver
            //Ainv->solve(rhs);
            for (int i = 0; i < nv; ++i)
            {
                if (rhs[i] > 1)
                    rhs[i] = 1; //clip just in case
                if (rhs[i] > 1e-8)
                
                    nzweights[i].Add(new Tuple<int, double>(j, rhs[i]));
            }
        }

        for(int i = 0; i < nv; ++i) {
            double sum = 0;
            for(int j = 0; j < nzweights[i].Count; ++j)
                sum += nzweights[i][j].Item2;

            for(int j = 0; j < nzweights[i].Count; ++j) {
                double helper = nzweights[i][j].Item2 / sum;
                nzweights[i][j] = new Tuple<int, double>(nzweights[i][j].Item1, helper);
                weights[i][nzweights[i][j].Item1] = nzweights[i][j].Item2;
            }
        }*/
    }
}

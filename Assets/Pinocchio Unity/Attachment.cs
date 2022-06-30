using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;
using System;

public class Attachment
{
    List<List<double>> weights = new List<List<double>>();
    List<List<Tuple<int, double>>> nzweights;
    Attachment(Mesh mesh, GameObject skeleton, double initialHeatWeight = 1)
    {
        int i, j;

        // nv beschreibt die Anzahl von vertices im Mesh
        int nv = mesh.vertices.Length;

        //compute edges
        List<List<int>> edges = new List<List<int>>();

        for (i = 0; i < nv; ++i)
        {
            List<int> edge = new List<int>();
            for (j = 0; j < mesh.triangles.Length; ++j)
            {
                if(mesh.triangles[j] == i)
                {
                    if(j % 3 == 0)
                    {
                        edge.Add(mesh.triangles[j + 1]);
                        edge.Add(mesh.triangles[j + 2]);
                    }
                    else if (j % 3 == 1)
                    {
                        edge.Add(mesh.triangles[j - 1]);
                        edge.Add(mesh.triangles[j + 1]);
                    }
                    else
                    {
                        edge.Add(mesh.triangles[j - 2]);
                        edge.Add(mesh.triangles[j - 1]);
                    }
                }
            }
            edges.Add(edge);
        }

        //Anlegen von Liste von Bones
        List<Tuple<Vector3, Vector3>> bonelist = new List<Tuple<Vector3, Vector3>>();
        bonelist.Add(skeleton.segment);
        foreach (BoneTree bone in skeleton.children)
        {
            bonelist.Add(bone.segment);
        }

        int bones = skeleton.Count + 1;
        double[,] boneDists = new double[nv, bones];
        bool[,] boneVis = new bool[nv, bones];

        //Calculate MIN Distance
        //Für alle Vertices
        for (i = 0; i < nv; ++i)
        {
            Vector3 cPos = mesh.vertices[i];
            weights.Add(new List<double>());
            nzweights.Add(new List<Tuple<int, double>>());

            List<Vector3> normals = new List<Vector3>(); ;
            for (j = 0; j < edges[i].Count; ++j)
            {
                int nj = (j + 1) % edges[i].Count;
                Vector3 v1 = mesh.vertices[edges[i][j]] - cPos;
                Vector3 v2 = mesh.vertices[edges[i][nj]] - cPos;
                normals.Add(Vector3.Normalize(Vector3.Cross((v1,v2))));
            }

            double minDist = 1e37;

            //Für alles Bones
            for (j = 0; j < bones; ++j)
            {
                weights[i].Add(-1);
                Vector3 v1 = bonelist[j].Item1;
                Vector3 v2 = bonelist[j].Item2;

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
                    Mathf.Max(0, distToSeg);
                }

                boneDists[i,j] = distToSeg;
                minDist = Math.Min(boneDists[i,j], minDist);
            }

            for (j = 0; j < bones; ++j)
            {

                if (boneDists[i,j] > minDist * 1.0001)
                    continue;

                Vector3 v1 = bonelist[j].Item1;
                Vector3 v2 = bonelist[j].Item2;
                Vector3 dir = v2 - v1;
                Vector3 projToSeg;

                if (Vector3.Dot((v2 - cPos), dir) < 0)
                    projToSeg = v2;

                else if (Vector3.Dot((cPos - v1), dir) < 0)
                    projToSeg = v1;
                else
                    projToSeg = v1 + Vector3.Dot((cPos - v1), dir) / dir.sqrMagnitude * dir;
                Vector3 p = projToSeg;

                //This Part needs Raytracing of some kind  
                //boneVis[i][j] = tester->canSee(cPos, p) && vectorInCone(cPos - p, normals);
            }



        }

        //Part 2
        List<List<Tuple<int, double>>> valueLists = new List<List<Tuple<int, double>>>();
        double[] distance = new double[nv];
        double[] heat = new double[nv];
        int[] closest = new int[nv];

        //Für alle Vertices
        for (i = 0; i < nv; ++i)
        {
            List < Tuple<int, double>> valueList = new List<Tuple<int, double>>();
            //get areas
            for (j = 0; j < edges[i].Count; ++j)
            {
                int nj = (j + 1) % edges[i].Count;

                distance[i] += (Vector3.Cross((mesh.vertices[edges[i][j]] - mesh.vertices[i]),
                         (mesh.vertices[edges[i][nj]] - mesh.vertices[i]))).magnitude;
            }
            distance[i] = 1 / (1e-10 + distance[i]);

            //get bones
            double minDist = 1e37;
            for (j = 0; j < bones; ++j)
            {
                closest[i] = -1;
                if (boneDists[i,j] < minDist)
                {
                    closest[i] = j;
                    minDist = boneDists[i,j];
                }
            }
            for (j = 0; j < bones; ++j)

                //Needs the Ray-Tracing Input
                if (boneVis[i,j] && boneDists[i,j] <= minDist * 1.00001)
                    heat[i] += initialHeatWeight / ((1e-8 + boneDists[i,closest[i]]) * (1e-8 + boneDists[i,closest[i]]));
                else
                    heat[i] = 0;

            //get laplacian
            double sum = 0;
            for (j = 0; j < edges[i].Count; ++j)
            {
                int nj = (j + 1) % edges[i].Count;
                int pj = (j + edges[i].Count - 1) % edges[i].Count;

                Vector3 v1 = mesh.vertices[i] - mesh.vertices[edges[i][pj]];
                Vector3 v2 = mesh.vertices[edges[i][j]] - mesh.vertices[edges[i][pj]];
                Vector3 v3 = mesh.vertices[i] - mesh.vertices[edges[i][nj]];
                Vector3 v4 = mesh.vertices[edges[i][j]] - mesh.vertices[edges[i][nj]];

                double cot1 = (Vector3.Dot(v1,v2)) / (1e-6 + (Vector3.Cross(v1, v2)).magnitude);
                double cot2 = (Vector3.Dot(v3,v4)) / (1e-6 + (Vector3.Cross(v3, v4)).magnitude);
                sum += (cot1 + cot2);

                if (edges[i][j] > i)
                    continue;
                valueList.Add(new Tuple<int, double>(edges[i][j], -cot1 - cot2));
            }
            valueList.Add(new Tuple<int, double>(i, sum + heat[i] / distance[i]));
            valueList.Sort(delegate (Tuple<int, double> x, Tuple< int, double > y)
            {
                return x.Item2.CompareTo(y.Item2);
            });
            

        }
        // Needs some kind of Matrix computation
        //SPDMatrix Am(A);
        //LLTMatrix* Ainv = Am.factor();
        //if (Ainv == NULL)
        //    return;
        //
        for (j = 0; j < bones; ++j)
        {
            List<double> rhs = new List<double>(); ;
        for (i = 0; i < nv; ++i)
        {
            if (boneVis[i,j] && boneDists[i,j] <= boneDists[i,closest[i]] * 1.00001)
                rhs[i] = heat[i] / distance[i];
        }

        //No Matrix no solver
        //Ainv->solve(rhs);
        for (i = 0; i < nv; ++i)
        {
            if (rhs[i] > 1)
                rhs[i] = 1; //clip just in case
            if (rhs[i] > 1e-8)
            
                nzweights[i].Add(new Tuple<int, double>(j, rhs[i]));
        }
    }

    for(i = 0; i<nv; ++i) {
            double sum = 0;
            for(j = 0; j<nzweights[i].Count; ++j)
                sum += nzweights[i][j].Item2;

            for(j = 0; j<nzweights[i].Count; ++j) {
                double helper = nzweights[i][j].Item2 / sum;
                nzweights[i][j] = new Tuple<int, double>(nzweights[i][j].Item1, helper);
                weights[i][nzweights[i][j].Item1] = nzweights[i][j].Item2;
            }
        }

    }
}

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class BoneHeatNativePluginInterface
{

    public static void PreprocessMesh(Mesh mesh)
	{
		mesh.MarkDynamic();
        
		GCHandle gcVertices = GCHandle.Alloc(mesh.vertices, GCHandleType.Pinned);
		GCHandle gcIndices = GCHandle.Alloc(mesh.triangles, GCHandleType.Pinned);
		setMesh(gcVertices.AddrOfPinnedObject(), mesh.vertexCount, gcIndices.AddrOfPinnedObject(), mesh.triangles.Length);
		gcVertices.Free();
		gcIndices.Free();

        preprocessMesh(0.003f, 0.15f, 0.01f);

        Vector3[] resultVertices = new Vector3[numVertices()];
        int[] resultIndices = new int[numIndices()];
        
		GCHandle gcResultVertices = GCHandle.Alloc(resultVertices, GCHandleType.Pinned);
		GCHandle gcResultIndices = GCHandle.Alloc(resultIndices, GCHandleType.Pinned);
        writeMesh(gcResultVertices.AddrOfPinnedObject(), gcResultIndices.AddrOfPinnedObject());
        gcResultVertices.Free();
        gcResultIndices.Free();

        mesh.Clear();
        mesh.vertices = resultVertices;
        mesh.triangles = resultIndices;

        mesh.MarkModified();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
	}

    public static float[] SolveSPDMatrix(SparseMatrix matrix, float[] rhsArray) {
        SparseMatrix.Triplet[] tripletsArray = matrix.triplets.ToArray();

        float[] resultArray = new float[rhsArray.Length];

        GCHandle tripletsArrayHandle = GCHandle.Alloc(tripletsArray, GCHandleType.Pinned);
        GCHandle rhsArrayHandle = GCHandle.Alloc(rhsArray, GCHandleType.Pinned);
        GCHandle resultArrayHandle = GCHandle.Alloc(resultArray, GCHandleType.Pinned);

        int code = solveSPDMatrix(
                matrix.rows, 
                matrix.cols, 
                tripletsArrayHandle.AddrOfPinnedObject(), 
                tripletsArray.Length, 
                rhsArrayHandle.AddrOfPinnedObject(), 
                rhsArray.Length, 
                resultArrayHandle.AddrOfPinnedObject());

        tripletsArrayHandle.Free();
        rhsArrayHandle.Free();
        resultArrayHandle.Free();

        if (code == 0) {
            return resultArray;
        } else {
            throw new Exception($"Failed to solve matrix! Error code {code}");
        }
    }

    [DllImport("BoneHeat")]
    private static extern void setMesh(System.IntPtr vertexBuffer, int vertexCount, System.IntPtr indexBuffer, int indexCount);
    [DllImport("BoneHeat")]
    private static extern void preprocessMesh(float min_e, float max_e, float approx_error);
    [DllImport("BoneHeat")]
    private static extern int numVertices();
    [DllImport("BoneHeat")]
    private static extern int numIndices();

    [DllImport("BoneHeat")]
    private static extern void writeMesh(System.IntPtr vertexBuffer, System.IntPtr indexBuffer);

    [DllImport("BoneHeat")]
    private static extern int solveSPDMatrix(int rows, int cols, System.IntPtr triplets, int tripletsLength, System.IntPtr rhs, int rhsLength, System.IntPtr result);

}

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class BoneHeatNativePluginInterface
{

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
    private static extern int solveSPDMatrix(int rows, int cols, System.IntPtr triplets, int tripletsLength, System.IntPtr rhs, int rhsLength, System.IntPtr result);

}
